using BD_TRAMPO.Services;
using BD_TRAMPO.Integrations.Enderecos;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.RateLimiting;
namespace BD_TRAMPO.Api;


public static class ConfiguracaoEnderecos
{
    public static void AdicionarEnderecos(this WebApplicationBuilder builder)
    {
        string url=builder.Configuration["Enderecos:ViaCepBaseUrl"]??"https://viacep.com.br/ws/";
        if(!Uri.TryCreate(url,UriKind.Absolute,out var endereco) ||
            !(url=="https://viacep.com.br/ws/" || builder.Environment.IsDevelopment() && endereco.IsLoopback && endereco.Scheme=="http"))
            throw new InvalidOperationException("URL do provedor de endereço não permitida.");
        builder.Services.AddHttpClient<IConsultaEnderecoProvider,ViaCepClient>(http=>{
            http.BaseAddress=endereco;http.Timeout=TimeSpan.FromSeconds(4);http.MaxResponseContentBufferSize=131072;
        }).RemoveAllLoggers(); // Não registrar ruas/cidades na URL nem detalhes da resposta.
        builder.Services.AddSingleton<CacheEnderecos>();
        builder.Services.AddSingleton<IGeocodificacaoProvider,GeocodificacaoNaoConfigurada>();
        builder.Services.AddScoped<EnderecoService>();builder.Services.AddScoped<LocalService>();
        builder.Services.AddRateLimiter(options=>{
            options.AddPolicy("enderecos",context=>RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString()??"desconhecido",
                _=>new FixedWindowRateLimiterOptions{PermitLimit=30,Window=TimeSpan.FromMinutes(1),QueueLimit=0,AutoReplenishment=true}));
        });
    }
}
