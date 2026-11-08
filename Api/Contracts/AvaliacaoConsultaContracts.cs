namespace BD_TRAMPO.Api.Contracts;

public sealed record AvaliacaoPublicaResponse(int Nota, string? Comentario,
    string ClienteNome, DateTime Data, string? ServicoNome)
{
    public static AvaliacaoPublicaResponse De(Avaliacao avaliacao) =>
        new(avaliacao.Nota, avaliacao.Comentario, avaliacao.NomeUsuario, avaliacao.Data,
            avaliacao.ServicoNome);
}

public sealed record AvaliacoesPublicasResponse(double? Media, int Total,
    List<AvaliacaoPublicaResponse> Avaliacoes)
{
    public static AvaliacoesPublicasResponse De(List<Avaliacao> avaliacoes) =>
        new(avaliacoes.Count == 0 ? null : avaliacoes.Average(a => a.Nota),
            avaliacoes.Count, avaliacoes.Select(AvaliacaoPublicaResponse.De).ToList());
}
