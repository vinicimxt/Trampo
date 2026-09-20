using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{

    public class NotificacaoController : BaseController
    {


        [HttpPost]
        public IActionResult MarcarTodasLidas()
        {
            new NotificacaoDAO().MarcarTodasLidas(UsuarioAtualId);
            return Ok();
        }
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


        [HttpPost]
        public IActionResult MarcarComoLida(int id)
        {
            NotificacaoDAO dao = new NotificacaoDAO();
            if (!dao.MarcarComoLida(id, UsuarioAtualId)) return NotFound();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MarcarComoLidaAjax([FromBody] System.Text.Json.JsonElement data)
        {
            if (data.ValueKind != System.Text.Json.JsonValueKind.Object ||
                !data.TryGetProperty("id", out var valor) || !valor.TryGetInt32(out int id))
                return BadRequest("ID inválido.");

            NotificacaoDAO dao = new NotificacaoDAO();
            if (!dao.MarcarComoLida(id, UsuarioAtualId)) return NotFound();

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

        [HttpPost]
        public IActionResult Abrir(int id)
        {
            NotificacaoDAO dao = new NotificacaoDAO();

            var notif = dao.BuscarPorId(id);

            if (notif == null || notif.UsuarioId != UsuarioAtualId)
                return RedirectToAction("Index");

            if (!dao.MarcarComoLida(id, UsuarioAtualId)) return NotFound();

            if (notif.Tipo == "Agendamento")
            {
                var ag = notif.ReferenciaId.HasValue ? new AgendamentoDAO().BuscarPorId(notif.ReferenciaId.Value) : null;
                return RedirectToAction(ag?.UsuarioId == UsuarioAtualId ? "Meus" : "Recebidos", "Agendamento");
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
