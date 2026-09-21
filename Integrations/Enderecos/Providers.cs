using BD_TRAMPO.Contracts;
namespace BD_TRAMPO.Integrations.Enderecos;

public interface IConsultaEnderecoProvider
{
    Task<EnderecoConsulta?> ConsultarCEP(string cep, CancellationToken ct);
    Task<IReadOnlyList<EnderecoConsulta>> Pesquisar(string uf, string cidade, string logradouro, CancellationToken ct);
}
public sealed record Coordenadas(decimal Latitude, decimal Longitude);
public interface IGeocodificacaoProvider
{
    Task<Coordenadas?> Localizar(EnderecoDados endereco, CancellationToken ct);
}
public sealed class GeocodificacaoNaoConfigurada : IGeocodificacaoProvider
{
    public Task<Coordenadas?> Localizar(EnderecoDados endereco, CancellationToken ct) => Task.FromResult<Coordenadas?>(null);
}
