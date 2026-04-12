using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers
{

    public class ServicoController : BaseController
    {
        public IActionResult Criar()
        {
            var auth = Proteger();
            if (auth != null) return auth;

            CategoriaDAO catDAO = new CategoriaDAO();
            ViewBag.Categorias = catDAO.Listar();

            ViewBag.Subcategorias = new List<Subcategoria>(); // começa vazio

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
            ViewBag.Subcategorias = subDAO.ListarTodas(); // 🔥 importante

            return View(servico);
        }

        [HttpPost]
        public IActionResult Editar(Servico s)
        {
            ServicoDAO dao = new ServicoDAO();
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
        public IActionResult Salvar(string nome, int subcategoriaId, string descricao, string contato, string atendimento, int? raioAtendimento, string linkOnline)
        {

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            ServicoDAO dao = new ServicoDAO();
            var lista = dao.ListarPorProfissional(profissionalId);
            if (atendimento == "Online" && string.IsNullOrEmpty(linkOnline))
            {
                return Content("Informe o link para atendimento online.");
            }
            Servico s = new Servico
            {
                ProfissionalId = profissionalId,
                Nome = nome,
                SubcategoriaId = subcategoriaId,
                Descricao = descricao,
                Contato = contato,
                Atendimento = atendimento,
                RaioAtendimento = raioAtendimento,
                LinkOnline = linkOnline
            };

            dao.Inserir(s);

            return RedirectToAction("MeusServicos", "Profissional");
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