using BD_TRAMPO.Contracts;

namespace BD_TRAMPO.Services;

public sealed class AvaliacaoConsultaService(AvaliacaoDAO avaliacoes, ServicoService servicos)
{
    public List<Avaliacao> Publicas(int servicoId)
    {
        var servico = servicos.BuscarPublico(servicoId);
        return avaliacoes.ListarPorProfissional(servico.ProfissionalId, servicoId);
    }

    public List<Avaliacao> Recebidas(UsuarioContexto usuario) =>
        avaliacoes.ListarPorProfissional(servicos.Profissional(usuario));
}
