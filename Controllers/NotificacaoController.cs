using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{

    public class NotificacaoController : BaseController
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

        [HttpPost]
        public IActionResult MarcarComoLidaAjax([FromBody] dynamic data)
        {
            int id = (int)data.id;

            NotificacaoDAO dao = new NotificacaoDAO();
            dao.MarcarComoLida(id);

            return Ok();
        }


        public IActionResult Ultimas()
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
                return PartialView("_NotificacoesDropdown", new List<Notificacao>());

            int usuarioId = int.Parse(usuarioIdStr);

            NotificacaoDAO dao = new NotificacaoDAO();
            var lista = dao.BuscarUltimas(usuarioId, 5);

            return PartialView("_NotificacoesDropdown", lista);
        }

        public IActionResult Abrir(int id)
        {
            NotificacaoDAO dao = new NotificacaoDAO();

            var notif = dao.BuscarPorId(id);

            if (notif == null)
                return RedirectToAction("Index");

            dao.MarcarComoLida(id);

            if (notif.Tipo == "Agendamento")
            {
                return RedirectToAction("Recebidos", "Agendamento");
            }

            if (notif.Tipo == "Cancelamento")
            {
                return RedirectToAction("Meus", "Agendamento");
            }

            if (notif.Tipo == "Avaliacao")
            {
                return RedirectToAction(
                    "Avaliar",
                    "Avaliacao",
                    new { agendamentoId = notif.ReferenciaId }
                );
            }

            return RedirectToAction("Index");
        }


    }


}
