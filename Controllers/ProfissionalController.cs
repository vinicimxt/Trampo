using Microsoft.AspNetCore.Mvc;


namespace BD_TRAMPO.Controllers
{
    public class ProfissionalController : Controller
    {

        public IActionResult Lista()
        {

            ServicoDAO dao = new ServicoDAO();
            var lista = dao.ListarServicos();

            return View(lista);
        }


        public IActionResult Dashboard()
        {
            string usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            int usuarioId = int.Parse(usuarioIdStr);

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            AgendamentoDAO agDAO = new AgendamentoDAO();

            var dados = agDAO.DashboardProfissional(profissionalId);

            return View(dados);
        }

        public IActionResult MeusServicos()
        {
            string usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            int usuarioId = int.Parse(usuarioIdStr);

            // 🔥 pega o profissional do usuário
            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            // 🔴 valida segurança
            if (profissionalId == 0)
            {
                return Content("Erro: profissional não encontrado.");
            }

            // 🔥 agora sim: só os serviços dele
            ServicoDAO servDAO = new ServicoDAO();
            var lista = servDAO.ListarPorProfissional(profissionalId);

            return View(lista);
        }

        public IActionResult Perfil()
        {
            return View();
        }


    }


}