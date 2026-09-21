using System.Text.Json.Serialization;
namespace BD_TRAMPO.Contracts;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EnderecoRequest
{
    public string? CEP { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? UF { get; set; }
}

public sealed record EnderecoDados(string? CEP, string? Logradouro, string? Numero,
    string? Complemento, string? Bairro, string? Cidade, string? UF,
    decimal? Latitude, decimal? Longitude, string EnderecoFormatado, bool Estruturado);
public sealed record EnderecoConsulta(string CEP, string Logradouro, string Bairro, string Cidade, string UF);
public enum TipoFalhaEndereco { NaoEncontrado, Indisponivel, Timeout, RespostaInvalida }
public sealed class FalhaEndereco(TipoFalhaEndereco tipo) : Exception("Não foi possível consultar o endereço. Preencha manualmente ou tente novamente.")
{ public TipoFalhaEndereco Tipo { get; } = tipo; }

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SalvarLocalRequest
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public string? Endereco { get; set; } // Compatibilidade do MVC anterior.
    public EnderecoRequest? DadosEndereco { get; set; }
}
