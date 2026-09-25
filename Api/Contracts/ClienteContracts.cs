using System.Text.Json.Serialization;
namespace BD_TRAMPO.Api.Contracts;
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CadastroClienteRequest(string Nome, string Email, string Senha, string? Telefone);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PerfilRequest(string Nome, string? Telefone);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SenhaRequest(string SenhaAtual, string NovaSenha, string ConfirmarSenha);
public sealed record PerfilResponse(string Nome, string Email, string? Telefone);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record SuporteRequest(string Tipo, string Assunto, string Mensagem);
public sealed record NotificacaoResponse(int Id, string Titulo, string Mensagem, DateTime DataCriacao, bool Lida);
public sealed record EstadoAvaliacaoResponse(bool PodeAvaliar, bool JaAvaliado);
