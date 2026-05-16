using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{
    public class AdminController : Controller
    {
        private bool UsuarioEhAdmin()
        {
            string email =
                HttpContext.Session.GetString("UsuarioEmail");

            return email == "admin@trampo.com";
        }

        public IActionResult Dashboard()
        {
            if (!UsuarioEhAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            AdminDAO dao = new AdminDAO();

            ViewBag.TotalUsuarios =
                dao.TotalUsuarios();

            ViewBag.TotalProfissionais =
                dao.TotalProfissionais();

            ViewBag.TotalPremium =
                dao.TotalPremium();

            ViewBag.TotalAgendamentos =
                dao.TotalAgendamentos();

            ViewBag.TotalTaxas =
                dao.TotalTaxas();

            ViewBag.TotalPremiumReceita =
                dao.TotalReceitaPremium();

            ViewBag.TotalLiquido =
                dao.TotalLiquidoPlataforma();

            ViewBag.UltimasAssinaturas =
                dao.UltimasAssinaturas();

            ViewBag.UltimosPagamentos =
                dao.UltimosPagamentos();

            return View();
        }
    }
}