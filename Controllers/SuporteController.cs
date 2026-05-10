using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;
using BD_TRAMPO.Models;

namespace BD_TRAMPO.Controllers
{

    public class SuporteController : Controller
    {


        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult Enviar(string tipo, string assunto, string mensagem)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioIdStr))
            {
                TempData["Erro"] = "Você precisa estar logado para enviar uma mensagem de suporte.";
                return RedirectToAction("Login", "Usuario");
            }

            int usuarioId = int.Parse(usuarioIdStr);

            SuporteDAO dao = new SuporteDAO();

            dao.Inserir(new Suporte
            {
                UsuarioId = usuarioId,
                Tipo = tipo,
                Assunto = assunto,
                Mensagem = mensagem,
                Status = "Aberto"
            });

            TempData["Sucesso"] = "Mensagem enviada com sucesso! Nossa equipe já recebeu seu chamado.";

            return RedirectToAction("Index");
        }

        public IActionResult ContatoAjuda()
        {
            return View();
        }

    }







}