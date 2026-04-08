namespace BD_TRAMPO
{
    public static class Sessao
{
    public static int? UsuarioId(HttpContext context)
    {
        var id = context.Session.GetString("UsuarioId");
        return id != null ? int.Parse(id) : (int?)null;
    }

    public static string Nome(HttpContext context)
    {
        return context.Session.GetString("UsuarioNome");
    }

    public static string Tipo(HttpContext context)
    {
        return context.Session.GetString("UsuarioTipo");
    }

    public static bool EstaLogado(HttpContext context)
    {
        return UsuarioId(context) != null;
    }
}
}