namespace BD_TRAMPO.Api.Contracts;

public sealed record SubcategoriaResponse(int Id, string Nome);
public sealed record CategoriaResponse(int Id, string Nome, IReadOnlyList<SubcategoriaResponse> Subcategorias)
{
    public static List<CategoriaResponse> Montar(IEnumerable<Categoria> categorias,
        IEnumerable<Subcategoria> subcategorias)
    {
        var porCategoria = subcategorias.GroupBy(s => s.CategoriaId)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Id)
                .Select(s => new SubcategoriaResponse(s.Id, s.Nome)).ToList());
        return categorias.OrderBy(c => c.Id)
            .Select(c => new CategoriaResponse(c.Id, c.Nome,
                porCategoria.GetValueOrDefault(c.Id) ?? [])).ToList();
    }
}
