namespace BD_TRAMPO.Contracts;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed class CriarAgendamentoRequest
{
    public EnderecoRequest? Endereco { get; set; }
    public int ServicoId { get; set; }
    public DateTime Data { get; set; }
    public TimeSpan? Hora { get; set; }
    public string? Descricao { get; set; }
    public string? Rua { get; set; }
    public string? Numero { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
}

// Na edição, a subcategoria vem do serviço persistido; só a criação a envia.
public sealed class SalvarServicoRequest
{
    public int Id { get; set; }
    public int SubcategoriaId { get; set; }
    public string Nome { get; set; } = "";
    public string? Descricao { get; set; }
    public string Atendimento { get; set; } = "";
    public int? LocalId { get; set; }
    public string? LinkOnline { get; set; }
    public string TipoPreco { get; set; } = "";
    [Microsoft.AspNetCore.Mvc.ModelBinder(BinderType = typeof(PrecoFormularioModelBinder))]
    public decimal? PrecoBase { get; set; }
    public string DiasSemana { get; set; } = "";
    public TimeSpan? HoraInicio { get; set; }
    public TimeSpan? HoraFim { get; set; }
}

public sealed class AvaliarAgendamentoRequest
{
    public int AgendamentoId { get; set; }
    public int ProfissionalId { get; set; }
    public int Nota { get; set; }
    public string? Comentario { get; set; }
}

public sealed record AgendaDisponivel(Servico Servico, string Profissional,
    DateTime Dia, IReadOnlyList<Disponibilidade> Regras, IReadOnlyList<TimeSpan> Horarios);
