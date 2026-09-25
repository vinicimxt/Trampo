using BD_TRAMPO.Contracts;
using BD_TRAMPO.DAO;
using BD_TRAMPO.Models;
namespace BD_TRAMPO.Services;
public sealed class ClienteAtendimentoService(NotificacaoDAO notificacoes, SuporteDAO suporte, AutenticacaoService auth)
{
    public List<Notificacao> Notificacoes(UsuarioContexto usuario, int pagina, int tamanho) {
        auth.Atual(usuario);
        if (pagina < 1 || pagina > 1000000 || tamanho < 1 || tamanho > 100)
            throw new FalhaOperacao(TipoFalha.Validacao,"Paginação inválida.");
        return notificacoes.ListarPorUsuario(usuario.UsuarioId, pagina, tamanho);
    }
    public void Ler(UsuarioContexto usuario, int id) {
        auth.Atual(usuario);
        if (!notificacoes.MarcarComoLida(id,usuario.UsuarioId))
            throw new FalhaOperacao(TipoFalha.NaoEncontrado,"Notificação não encontrada.");
    }
    public void Enviar(UsuarioContexto usuario, string tipo, string assunto, string mensagem) {
        auth.Atual(usuario);
        if (string.IsNullOrWhiteSpace(tipo) || tipo.Length>50 || string.IsNullOrWhiteSpace(assunto) || assunto.Length>150 ||
            string.IsNullOrWhiteSpace(mensagem) || mensagem.Length>10000)
            throw new FalhaOperacao(TipoFalha.Validacao,"Confira os dados do chamado.");
        suporte.Inserir(new Suporte { UsuarioId=usuario.UsuarioId, Tipo=tipo, Assunto=assunto, Mensagem=mensagem, Status="Aberto" });
    }
}
