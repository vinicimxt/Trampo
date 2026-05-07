using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;


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

        public override void OnActionExecuting(
                   Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (!string.IsNullOrEmpty(usuarioIdStr))
            {
                int usuarioId = int.Parse(usuarioIdStr);

                NotificacaoDAO notifDAO = new NotificacaoDAO();

                ViewBag.NotifCount = notifDAO.ContarNaoLidas(usuarioId);
            }
        }

    }
}