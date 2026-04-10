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

        [HttpPost]
        public IActionResult Salvar(string nome, int subcategoriaId, string descricao, string contato, string atendimento, int? raioAtendimento)
        {
            ServicoDAO dao = new ServicoDAO();

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            Servico s = new Servico
            {
                ProfissionalId = profissionalId,
                Nome = nome,
                SubcategoriaId = subcategoriaId,
                Descricao = descricao,
                Contato = contato,
                Atendimento = atendimento,
                RaioAtendimento = raioAtendimento
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