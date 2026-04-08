using Microsoft.AspNetCore.Mvc;


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
        if (!UsuarioLogado())
            return RedirectToAction("Login", "Usuario");

        return null;
    }
    
}
}