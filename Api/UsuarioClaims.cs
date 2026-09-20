using BD_TRAMPO.Contracts;
using System.Security.Claims;

namespace BD_TRAMPO.Api;

public static class UsuarioClaims
{
    public static UsuarioContexto Contexto(this ClaimsPrincipal principal)
    {
        string? tipo = principal.FindFirstValue("role");
        if (principal.Identity?.IsAuthenticated != true ||
            !int.TryParse(principal.FindFirstValue("sub"), out int id) || id <= 0 ||
            tipo is not ("cliente" or "profissional" or "admin"))
            throw new FalhaOperacao(TipoFalha.NaoAutenticado, "Autenticação inválida.");
        return new(id, tipo);
    }
}
