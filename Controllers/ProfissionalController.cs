using Microsoft.AspNetCore.Mvc;


namespace BD_TRAMPO.Controllers
{
    public class ProfissionalController : Controller
    {

        public IActionResult Lista()
        {

            ProfissionalDAO dao = new ProfissionalDAO();
            var lista = dao.Listar();

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

            // 🔥 agora busca os SERVIÇOS
            ServicoDAO servDAO = new ServicoDAO();
            var lista = servDAO.ListarPorProfissional(profissionalId);

            return View(lista);
        }

  


    }


}