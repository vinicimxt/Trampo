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

        [HttpPost]
        public IActionResult Criar(string servico, string descricao, string atendimento, int? raio, string tipoDocumento, string documento)
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO dao = new ProfissionalDAO();
            dao.Inserir(usuarioId, servico, descricao, atendimento, raio, tipoDocumento, documento);

            return RedirectToAction("Lista");
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

            ProfissionalDAO dao = new ProfissionalDAO();

            var lista = dao.ListarPorUsuario(usuarioId);

            return View(lista);
        }


    }


}