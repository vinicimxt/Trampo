using BD_TRAMPO.Contracts;
using BD_TRAMPO.Models;

namespace BD_TRAMPO.Services;

// MVC e API reutilizam BuscarLogin: inclusive a migração do hash legado.
public sealed class AutenticacaoService(UsuarioDAO usuarios)
{
    public Usuario Entrar(string? email, string? senha)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 100 ||
            string.IsNullOrWhiteSpace(senha) || senha.Length > 1024)
            throw new FalhaOperacao(TipoFalha.Validacao, "Email ou senha inválidos.");
        return usuarios.BuscarLogin(email, senha)
            ?? throw new FalhaOperacao(TipoFalha.NaoAutenticado, "Email ou senha inválidos.");
    }

    public Usuario Atual(UsuarioContexto contexto)
    {
        var usuario = usuarios.BuscarPorId(contexto.UsuarioId);
        if (usuario == null || !string.Equals(usuario.Tipo, contexto.Tipo, StringComparison.OrdinalIgnoreCase))
            throw new FalhaOperacao(TipoFalha.NaoAutenticado, "Autenticação inválida. Entre novamente.");
        return usuario;
    }
}
