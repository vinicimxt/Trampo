using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BD_TRAMPO.Contracts;

namespace BD_TRAMPO.Api.Contracts;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record LoginRequest([Required, MaxLength(100)] string Email,
    [Required, MaxLength(1024)] string Senha);
public sealed record LoginResponse(string AccessToken, string TokenType, DateTime ExpiresAt);
public sealed record UsuarioResponse(int Id, string Nome, string Email, string Tipo);
public sealed record ErroDetalhe(string Codigo, string Mensagem);
public sealed record ErroResponse(ErroDetalhe Erro);
public sealed record RecursoCriadoResponse(int Id);
public sealed record RemocaoResponse(int Id, string Resultado);
public sealed record AgendaResponse(int ServicoId, DateOnly Data, IReadOnlyList<TimeSpan> Horarios);
public sealed record ServicoResponse(int Id, int ProfissionalId, int SubcategoriaId,
    string Nome, string Descricao, string Atendimento, string TipoPreco, decimal? PrecoBase, bool Ativo)
{
    // Link de reunião e endereço não fazem parte do catálogo público.
    public static ServicoResponse De(Servico s) => new(s.Id, s.ProfissionalId, s.SubcategoriaId,
        s.Nome, s.Descricao, s.Atendimento, s.TipoPreco, s.PrecoBase, s.Ativo);
}

public sealed record AgendamentoResponse(int Id, int ServicoId, int ProfissionalId,
    DateOnly Data, TimeSpan Hora, string Status, string Descricao, string? EnderecoCliente,
    decimal? ValorFinal, string OrigemValorFinal)
{
    // Somente valor efetivamente persistido. Não expõe preço atual como preço contratado.
    public static AgendamentoResponse De(Agendamento a) => new(a.Id, a.ServicoId, a.ProfissionalId,
        DateOnly.FromDateTime(a.Data), a.Hora, a.StatusAtual(), a.Descricao, a.EnderecoCliente,
        a.ValorFinal, "finalizacao_sem_snapshot_da_oferta");
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record FinalizarRequest(decimal ValorFinal);
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record AvaliacaoRequest(int Nota, string? Comentario);

// A rota define o ID; o contrato HTTP não aceita IDs de proprietário ou de recurso.
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ServicoRequest
{
    public int SubcategoriaId { get; set; }
    public string Nome { get; set; } = "";
    public string? Descricao { get; set; }
    public string Atendimento { get; set; } = "";
    public int? LocalId { get; set; }
    public string? LinkOnline { get; set; }
    public string TipoPreco { get; set; } = "";
    public decimal? PrecoBase { get; set; }
    public string DiasSemana { get; set; } = "";
    public TimeSpan? HoraInicio { get; set; }
    public TimeSpan? HoraFim { get; set; }
    public SalvarServicoRequest Aplicacao(int id = 0) => new() {
        Id=id, SubcategoriaId=SubcategoriaId, Nome=Nome, Descricao=Descricao,
        Atendimento=Atendimento, LocalId=LocalId, LinkOnline=LinkOnline, TipoPreco=TipoPreco,
        PrecoBase=PrecoBase, DiasSemana=DiasSemana, HoraInicio=HoraInicio, HoraFim=HoraFim
    };
}
