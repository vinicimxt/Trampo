using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers
{

    public class ServicoController : BaseController
    {
        public IActionResult Criar()
        {
            var auth = Proteger();
            if (auth != null) return auth;

            return View();
        }

        [HttpPost]
        public IActionResult Criar(Servico s)
        {
            string usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
                return RedirectToAction("Login", "Usuario");

            int usuarioId = int.Parse(usuarioIdStr);

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            s.ProfissionalId = profissionalId;

            ServicoDAO dao = new ServicoDAO();
            dao.Inserir(s);

            return RedirectToAction("MeusServicos");
        }




    }

}