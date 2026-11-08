using BD_TRAMPO.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace BD_TRAMPO.Controllers
{
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
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
        public IActionResult Status(int code)
        {
            Response.StatusCode = code is >= 400 and < 600 ? code : 404;
            ViewData["StatusMessage"] = code switch {
                403 => "Você não possui permissão para acessar esta página.",
                404 => "Página não encontrada.",
                _ => "Não foi possível concluir a solicitação. Tente novamente."
            };
            return View("~/Views/Shared/Status.cshtml");
        }
        public IActionResult Termos()
        {
            return View();
        }

        public IActionResult Privacidade()
        {
            return View();
        }

     
    }
}
