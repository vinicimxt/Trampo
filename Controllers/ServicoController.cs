using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;
namespace BD_TRAMPO.Controllers
{

    public class ServicoController : BaseController
    {
        public IActionResult Criar()
        {
            var auth = Proteger();
            if (auth != null) return auth;

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            LocalDAO localDAO = new LocalDAO();
            ViewBag.Locais = localDAO.ListarPorProfissional(profissionalId);

            CategoriaDAO catDAO = new CategoriaDAO();
            ViewBag.Categorias = catDAO.Listar();

            ViewBag.Subcategorias = new List<Subcategoria>(); // começa vazio
            ServicoDAO servicoDAO = new ServicoDAO();
            ViewBag.TotalServicos = servicoDAO.ContarPorProfissional(usuarioId);

            AgendamentoDAO agendamentoDAO = new AgendamentoDAO();
            ViewBag.PedidosPendentes = agendamentoDAO.ContarPendentes(usuarioId);

            return View();
        }

        public JsonResult GetSubcategorias(int categoriaId)
        {
            SubcategoriaDAO dao = new SubcategoriaDAO();
            var lista = dao.ListarPorCategoria(categoriaId);

            return Json(lista);
        }

        public IActionResult Editar(int id)
        {
            ServicoDAO dao = new ServicoDAO();
            var servico = dao.BuscarPorId(id);

            SubcategoriaDAO subDAO = new SubcategoriaDAO();
            ViewBag.Subcategorias = subDAO.ListarTodas();

            return View(servico);
        }

        [HttpPost]
        public IActionResult Editar(Servico s)
        {
            ServicoDAO dao = new ServicoDAO();

            var original = dao.BuscarPorId(s.Id);

            // mantém subcategoria original
            s.SubcategoriaId = original.SubcategoriaId;

            if (s.Atendimento == "Online" && string.IsNullOrEmpty(s.LinkOnline))
            {
                return Content("Informe o link para atendimento online.");
            }

            if (s.Atendimento != "Online")
            {
                s.LinkOnline = null;
            }

            dao.Atualizar(s);

            return RedirectToAction("MeusServicos", "Profissional");
        }
        public IActionResult Excluir(int id)
        {
            ServicoDAO dao = new ServicoDAO();
            dao.Excluir(id);

            return RedirectToAction("MeusServicos", "Profissional");
        }

        [HttpPost]
        public IActionResult Salvar(string nome, int subcategoriaId, string descricao, string contato, string atendimento, int? localId, string linkOnline)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
                return RedirectToAction("Login", "Usuario");

            int usuarioId = int.Parse(usuarioIdStr);

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            if (profissionalId == 0)
            {
                TempData["Erro"] = "Você precisa ser um profissional para criar serviços.";
                return RedirectToAction("Cadastro", "Usuario");
            }

            // 🔥 VALIDAÇÕES

            if (string.IsNullOrWhiteSpace(nome))
            {
                TempData["Erro"] = "Informe o nome do serviço.";
                return RedirectToAction("Criar");
            }

            if (atendimento == "Online")
            {
                if (string.IsNullOrWhiteSpace(linkOnline))
                {
                    TempData["Erro"] = "Informe o link do atendimento online.";
                    return RedirectToAction("Criar");
                }

                if (!Uri.IsWellFormedUriString(linkOnline, UriKind.Absolute))
                {
                    TempData["Erro"] = "Informe um link válido.";
                    return RedirectToAction("Criar");
                }
            }

            if (atendimento == "Local")
            {
                LocalDAO localDAO = new LocalDAO();
                var locais = localDAO.ListarPorProfissional(profissionalId);

                if (locais.Count == 0)
                {
                    TempData["Erro"] = "Cadastre um local antes de criar serviços presenciais.";
                    return RedirectToAction("Criar");
                }

                if (!localId.HasValue)
                {
                    TempData["Erro"] = "Selecione um local.";
                    return RedirectToAction("Criar");
                }
            }

            try
            {
                ServicoDAO dao = new ServicoDAO();

                Servico s = new Servico
                {
                    ProfissionalId = profissionalId,
                    Nome = nome,
                    SubcategoriaId = subcategoriaId,
                    Descricao = descricao,
                    Contato = contato,
                    Atendimento = atendimento,
                    LocalId = localId,
                    LinkOnline = linkOnline
                };

                dao.Inserir(s);

                TempData["Sucesso"] = "Serviço criado com sucesso 🚀";

                return RedirectToAction("MeusServicos", "Profissional");
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao criar serviço. Tente novamente.";
                return RedirectToAction("Criar");
            }
        }



        public JsonResult Subcategorias(int categoriaId)
        {
            SubcategoriaDAO dao = new SubcategoriaDAO();
            var lista = dao.ListarPorCategoria(categoriaId);

            return Json(lista);
        }

        public IActionResult Novo()
        {
            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            CategoriaDAO catDAO = new CategoriaDAO();
            ViewBag.Categorias = catDAO.Listar();

            return View();
        }


    }

}