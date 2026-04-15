using BD_TRAMPO.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BD_TRAMPO.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            using (var db = new XamouContext())
            {
                var servicos = db.Servicos
                    .OrderByDescending(s => s.Id)
                    .Take(6)
                    .ToList();

                return View(servicos);
            }
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
