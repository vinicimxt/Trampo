namespace BD_TRAMPO.Api.Contracts;

public sealed record ServicoProprioResponse(int Id, int SubcategoriaId, string Nome,
    string Descricao, string Atendimento, int? LocalId, string? LinkOnline,
    string TipoPreco, decimal? PrecoBase, bool Ativo, string DiasSemana,
    TimeSpan? HoraInicio, TimeSpan? HoraFim, string? Categoria, string? Subcategoria)
{
    public static ServicoProprioResponse De(Servico s) => new(s.Id, s.SubcategoriaId,
        s.Nome, s.Descricao, s.Atendimento, s.LocalId, s.LinkOnline,
        s.TipoPreco, s.PrecoBase, s.Ativo, s.DiasTexto ?? "",
        s.HoraInicio, s.HoraFim, s.Categoria, s.Subcategoria);
}
