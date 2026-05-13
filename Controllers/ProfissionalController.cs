using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{
    public class ProfissionalController : BaseController
    {

        private string TraduzirDia(int dia)
        {
            switch (dia)
            {
                case 0: return "DOM";
                case 1: return "SEG";
                case 2: return "TER";
                case 3: return "QUA";
                case 4: return "QUI";
                case 5: return "SEX";
                case 6: return "SAB";
                default: return "";
            }
        }
        public IActionResult Lista(string busca, string localizacao, string categoria)
        {
            ServicoDAO dao = new ServicoDAO();

            var lista = dao.ListarServicos(busca, localizacao, categoria);

            DisponibilidadeDAO dispDAO = new DisponibilidadeDAO();

            foreach (var servico in lista)
            {
                var disp = dispDAO.BuscarPorServico(servico.Id);

                if (disp.Any())
                {
                    servico.HoraInicio = disp.Min(x => x.HoraInicio);
                    servico.HoraFim = disp.Max(x => x.HoraFim);

                    servico.DiasTexto = string.Join(", ",
                        disp
                            .Select(x => x.DiaSemana)
                            .Distinct()
                            .OrderBy(x => x)
                            .Select(x => TraduzirDia(x))
                    );
                }
            }


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

            //  pega o profissional do usuário
            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            //  valida segurança
            if (profissionalId == 0)
            {
                return Content("Erro: profissional não encontrado.");
            }

            // só os serviços dele
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
            AvaliacaoDAO avalDAO = new AvaliacaoDAO();

            var profissional = profDAO.BuscarPorId(id);
            var servicos = servicoDAO.ListarPorProfissional(id);

            if (profissional == null)
            {
                return Content("Profissional não encontrado.");
            }

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
        public IActionResult Contato()
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO dao = new ProfissionalDAO();

            int profissionalId = dao.BuscarPorUsuario(usuarioId);

            var prof = dao.BuscarPorId(profissionalId);

            return View(prof);
        }

        [HttpPost]
        public IActionResult SalvarContato(string contato)
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO dao = new ProfissionalDAO();

            int profissionalId = dao.BuscarPorUsuario(usuarioId);

            dao.AtualizarContato(profissionalId, contato);

            TempData["Sucesso"] = "Contato atualizado com sucesso.";

            return RedirectToAction("Contato");
        }

        public IActionResult Premium()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AssinarPremium()
        {
            string usuarioSession = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioSession))
                return RedirectToAction("Login", "Usuario");

            int usuarioId = int.Parse(usuarioSession);

            ProfissionalDAO dao = new ProfissionalDAO();

            int profissionalId = dao.BuscarPorUsuario(usuarioId);

            dao.AtivarPremium(profissionalId);

            TempData["Sucesso"] = "Plano Premium ativado com sucesso!";

            return RedirectToAction("Dashboard");
        }


    }


}