using BD_TRAMPO.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BD_TRAMPO.Controllers
{
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            ServicoDAO dao = new ServicoDAO();

            ViewBag.CountCategorias = dao.ContarServicosPorCategoria();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}