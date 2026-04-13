using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;
namespace BD_TRAMPO.Controllers
{

    public class LocalController : Controller
    {
        public IActionResult Lista()
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            LocalDAO dao = new LocalDAO();
            var lista = dao.ListarPorProfissional(profissionalId);

            return View(lista);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(string nome, string endereco)
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            LocalDAO dao = new LocalDAO();

            dao.Inserir(new Local
            {
                ProfissionalId = profissionalId,
                Nome = nome,
                Endereco = endereco
            });

            return RedirectToAction("Lista");
        }
    }

}