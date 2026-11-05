using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Threading.RateLimiting;

namespace BD_TRAMPO.Api;

public static class ConfiguracaoApi
{
    public static void AdicionarApi(this WebApplicationBuilder builder)
    {
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtConfiguracao>() ?? new();
        if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience) ||
            jwt.ExpirationMinutes is < 1 or > 60)
            throw new InvalidOperationException("Configuração JWT inválida: issuer, audience ou expiração.");
        var chave = jwt.Chave(); // Configuração malformada falha cedo, sem imprimir seu conteúdo.
        builder.Services.AddSingleton(jwt);
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<AutenticacaoService>();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
            options.MapInboundClaims = false;
            options.IncludeErrorDetails = false;
            options.TokenValidationParameters = new() {
                ValidateIssuer = true, ValidIssuer = jwt.Issuer,
                ValidateAudience = true, ValidAudience = jwt.Audience,
                ValidateLifetime = true, RequireExpirationTime = true,
                ValidateIssuerSigningKey = true, RequireSignedTokens = true,
                IssuerSigningKey = chave, ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ClockSkew = TimeSpan.FromSeconds(30), NameClaimType = "sub", RoleClaimType = "role"
            };
            options.Events = new JwtBearerEvents {
                OnTokenValidated = context => {
                    try {
                        if (context.Principal == null) { context.Fail("Credenciais inválidas."); return Task.CompletedTask; }
                        var usuario = context.Principal.Contexto();
                        context.HttpContext.RequestServices.GetRequiredService<AutenticacaoService>().Atual(usuario);
                    }
                    catch (BD_TRAMPO.Contracts.FalhaOperacao) { context.Fail("Credenciais inválidas."); }
                    return Task.CompletedTask;
                },
                OnChallenge = context => {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.Headers.WWWAuthenticate = "Bearer";
                    return Task.CompletedTask;
                }
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.Configure<ApiBehaviorOptions>(options => {
            options.SuppressMapClientErrors = true;
            options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(ErrosApiMiddleware.Resposta(400));
        });
        builder.Services.AddRateLimiter(options => {
            options.RejectionStatusCode = 429;
            options.AddPolicy("api-login", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                _ => new FixedWindowRateLimiterOptions {
                    PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0,
                    AutoReplenishment = true
                }));
            options.OnRejected = async (context, cancellationToken) => {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await ErrosApiMiddleware.Escrever(context.HttpContext, 429);
            };
        });
        builder.Services.AddOpenApi("v1", options => {
            options.ShouldInclude = description => description.RelativePath?.StartsWith("api/v1/") == true;
            options.AddDocumentTransformer((document, context, cancellationToken) => {
                document.Info = new() { Title = "TRAMPO REST API", Version = "v1",
                    Description = "MVC e API compartilham Services. Valores finalizados não possuem snapshot da oferta original." };
                document.Components ??= new();
                document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme> {
                    ["Bearer"] = new OpenApiSecurityScheme { Type = SecuritySchemeType.Http,
                        Scheme = "bearer", BearerFormat = "JWT" }
                };
                foreach (var path in document.Paths)
                    foreach (var operation in path.Value.Operations ?? []) {
                        bool publico = path.Key == "/api/v1/auth/login" || path.Key == "/api/v1/auth/register" ||
                            (operation.Key == HttpMethod.Get &&
                             (path.Key == "/api/v1/servicos" || path.Key == "/api/v1/servicos/{id}"));
                        if (!publico) operation.Value.Security = [new OpenApiSecurityRequirement {
                            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                        }];
                    }
                return Task.CompletedTask;
            });
        });
    }
}
