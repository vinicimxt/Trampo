using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers
{

    public class ServicoController : Controller
    {
        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Servico s)
        {
            if (s.Nome == null)
            {
                return Content("Nome está vindo NULL");
            }

            string usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
            {
                return Content("Usuário não logado");
            }

            int usuarioId = int.Parse(usuarioIdStr);

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            if (profissionalId == 0)
            {
                return Content("Profissional não encontrado");
            }

            s.ProfissionalId = profissionalId;

            ServicoDAO dao = new ServicoDAO();
            dao.Inserir(s);

            return RedirectToAction("MeusServicos", "Profissional");

        }




    }

}