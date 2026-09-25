using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;
using BD_TRAMPO.Models;

namespace BD_TRAMPO.Controllers
{

    public class SuporteController(BD_TRAMPO.Services.ClienteAtendimentoService service) : BaseController
    {


        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Index()
        {
            return View("ContatoAjuda");
        }
        
        [HttpPost]
        public IActionResult Enviar(string tipo, string assunto, string mensagem)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdStr))
            {
                TempData["Erro"] = "VocÃª precisa estar logado para enviar uma mensagem de suporte.";
                return RedirectToAction("Login", "Usuario");
            }

            int usuarioId = int.Parse(usuarioIdStr);

            try {
                service.Enviar(new BD_TRAMPO.Contracts.UsuarioContexto(usuarioId,
                    HttpContext.Session.GetString("UsuarioTipo") ?? ""), tipo, assunto, mensagem);
            } catch (BD_TRAMPO.Contracts.FalhaOperacao ex) { return BadRequest(ex.Message); }

            TempData["Sucesso"] = "Mensagem enviada com sucesso! Nossa equipe jÃ¡ recebeu seu chamado.";

            return RedirectToAction("Index");
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult ContatoAjuda()
        {
            return View();
        }

    }







}