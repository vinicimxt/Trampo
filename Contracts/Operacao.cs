namespace BD_TRAMPO.Contracts;

public enum TipoFalha { Validacao, NaoAutenticado, SemPermissao, NaoEncontrado, Conflito }

public sealed class FalhaOperacao(TipoFalha tipo, string mensagem) : InvalidOperationException(mensagem)
{
    public TipoFalha Tipo { get; } = tipo;
}

public sealed record UsuarioContexto(int UsuarioId, string Tipo);