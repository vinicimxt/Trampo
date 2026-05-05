using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{

    public class NotificacaoController : Controller
    {


        public IActionResult Index()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
                return RedirectToAction("Login", "Usuario");

            int usuarioId = int.Parse(usuarioIdStr);

            NotificacaoDAO dao = new NotificacaoDAO();
            var lista = dao.ListarPorUsuario(usuarioId);

            return View(lista);
        }

        public IActionResult Abrir(int id, int? referenciaId)
        {
            NotificacaoDAO dao = new NotificacaoDAO();
            dao.MarcarComoLida(id);

            if (referenciaId.HasValue)
            {
                return RedirectToAction("Detalhe", "Agendamento", new { id = referenciaId });
            }

            return RedirectToAction("Index");
        }

        public int Contador()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            int usuarioId = int.Parse(usuarioIdStr);

            NotificacaoDAO dao = new NotificacaoDAO();
            return dao.ContarNaoLidas(usuarioId);
        }

        public IActionResult MarcarComoLida(int id)
        {
            NotificacaoDAO dao = new NotificacaoDAO();
            dao.MarcarComoLida(id);

            return RedirectToAction("Index");
        }
        public IActionResult TesteNotificacao()
        {
            NotificacaoDAO dao = new NotificacaoDAO();

            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");
            int usuarioId = int.Parse(usuarioIdStr);

            dao.Inserir(new Notificacao
            {
                UsuarioId = usuarioId,
                Titulo = "Teste 🔥",
                Mensagem = "Essa é uma notificação de teste.",
                Tipo = "Teste"
            });

            return RedirectToAction("Index");
        }



    }


}
