using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BD_TRAMPO.Controllers
{

    public class BaseController : Controller
    {
        protected bool UsuarioLogado()
        {
            return Sessao.EstaLogado(HttpContext);
        }

        protected IActionResult Proteger()
        {
            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            return null;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr != null)
            {
                int usuarioId = int.Parse(usuarioIdStr);

                NotificacaoDAO dao = new NotificacaoDAO();
                ViewBag.NotifCount = dao.ContarNaoLidas(usuarioId);
            }

            base.OnActionExecuting(context);
        }

    }
}