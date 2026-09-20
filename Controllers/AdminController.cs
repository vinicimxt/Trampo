using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{
    [Perfil("admin")]
    public class AdminController : BaseController
    {
        public IActionResult Dashboard()
        {
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