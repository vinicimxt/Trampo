using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{
    public class ProfissionalController : BaseController
    {

        public IActionResult Lista(string busca, string localizacao)
        {
            ServicoDAO dao = new ServicoDAO();

            var lista = dao.ListarServicos(busca, localizacao);

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

            SubcategoriaDAO subDao = new SubcategoriaDAO();
            ViewBag.Subcategorias = subDao.ListarTodas();
            
            LocalDAO localDao = new LocalDAO();
            ViewBag.Locais = localDao.ListarPorProfissional(usuarioId);

            return View(lista);
        }

        public IActionResult PerfilPublico(int id)
        {
            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            ProfissionalDAO profDAO = new ProfissionalDAO();
            ServicoDAO servicoDAO = new ServicoDAO();
            AvaliacaoDAO avalDAO = new AvaliacaoDAO(); // 🔥 NOVO

            var profissional = profDAO.BuscarPorId(id);
            var servicos = servicoDAO.ListarPorProfissional(id);

            if (profissional == null)
            {
                return Content("Profissional não encontrado.");
            }

            // 🔥 DADOS PRA VIEW
            ViewBag.Servicos = servicos;
            ViewBag.Avaliacoes = avalDAO.ListarPorProfissional(id);
            ViewBag.Media = avalDAO.MediaPorProfissional(id);

            return View(profissional);
        }

        public IActionResult MeuPerfil()
        {
            var auth = Proteger();
            if (auth != null) return auth;

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            ServicoDAO servicoDAO = new ServicoDAO();

            ViewBag.TotalServicos = servicoDAO.ContarPorProfissional(profissionalId);
            ViewBag.PedidosPendentes = 0; // ou seu método real

            return View();
        }



    }


}