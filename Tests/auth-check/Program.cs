using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using BD_TRAMPO.Controllers.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.IdentityModel.Tokens;

const string issuer = "trampo-auth-check";
const string audience = "trampo-auth-check-client";
var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddControllers().AddApplicationPart(typeof(ServicosApiController).Assembly);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new() {
        ValidIssuer = issuer, ValidAudience = audience, IssuerSigningKey = key,
        ValidateIssuer = true, ValidateAudience = true, ValidateIssuerSigningKey = true,
        ValidateLifetime = true, RoleClaimType = "role", NameClaimType = "sub"
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
try {
    var address = app.Services.GetRequiredService<IServer>()
        .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
    using var client = new HttpClient { BaseAddress = new Uri(address) };
    foreach (var route in new[] { "/api/v1/servicos/me", "/api/v1/locais",
        "/api/v1/profissionais/me/avaliacoes" }) {
        using var anonymous = await client.GetAsync(route);
        if (anonymous.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            throw new Exception($"Sem JWT: {route} retornou {anonymous.StatusCode}.");
        using var request = new HttpRequestMessage(HttpMethod.Get, route);
        request.Headers.Authorization = new("Bearer", Token("cliente"));
        using var clientProfile = await client.SendAsync(request);
        if (clientProfile.StatusCode != System.Net.HttpStatusCode.Forbidden)
            throw new Exception($"Cliente em rota profissional: {route} retornou {clientProfile.StatusCode}.");
    }
    Console.WriteLine("PASSOU: rotas profissionais devolvem 401 sem JWT e 403 para Cliente.");
} finally { await app.StopAsync(); }

string Token(string role) {
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var jwt = new JwtSecurityToken(issuer, audience,
        [new Claim("sub", "1"), new Claim("role", role)],
        expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: credentials);
    return new JwtSecurityTokenHandler().WriteToken(jwt);
}
