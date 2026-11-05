using BD_TRAMPO.Contracts;

namespace BD_TRAMPO.Services;

public sealed class AvaliacaoService(ServicoService servicos, AvaliacaoDAO avaliacoes)
{
    public List<Avaliacao> PublicasDoServico(int servicoId)
    {
        var servico = servicos.BuscarPublico(servicoId);
        return avaliacoes.ListarPorProfissional(servico.ProfissionalId);
    }

    public List<Avaliacao> Recebidas(UsuarioContexto usuario) =>
        avaliacoes.ListarPorProfissional(servicos.Profissional(usuario));
}
