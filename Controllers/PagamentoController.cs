using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{

    public class PagamentoController : Controller
    {

        public IActionResult CheckoutPremium()
        {
            var usuarioId =
                HttpContext.Session.GetString("UsuarioId");

            var tipo =
                HttpContext.Session.GetString("UsuarioTipo");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            if (tipo != "profissional")
            {
                TempData["Erro"] =
                    "Apenas profissionais podem assinar o plano.";

                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public IActionResult AssinarPremium()
        {
            string usuarioSession = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioSession))
                return RedirectToAction("Login", "Usuario");

            int usuarioId = int.Parse(usuarioSession);

            ProfissionalDAO dao = new ProfissionalDAO();

            int profissionalId = dao.BuscarPorUsuario(usuarioId);

            dao.AtivarPremium(profissionalId);

            TempData["Sucesso"] = "Plano Premium ativado com sucesso!";

            return RedirectToAction("Dashboard");
        }


        [HttpPost]
        public IActionResult ConfirmarPremium()
        {
            string usuarioSession = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioSession))
                return RedirectToAction("Login", "Usuario");

            int usuarioId = int.Parse(usuarioSession);

            ProfissionalDAO dao = new ProfissionalDAO();

            int profissionalId = dao.BuscarPorUsuario(usuarioId);

            dao.AtivarPremium(profissionalId);

            TempData["Sucesso"] = "Plano Premium ativado com sucesso!";

            return RedirectToAction("Dashboard", "Profissional");
        }

    }

}