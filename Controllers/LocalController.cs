using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{
    public class LocalController : BaseController
    {
        /* -----------------------------------------------
           Helper: pega o profissionalId da sessão
        ----------------------------------------------- */
        private int GetProfissionalId()
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));
            return new ProfissionalDAO().BuscarPorUsuario(usuarioId);
        }

        /* -----------------------------------------------
           GET /Local/Lista
           Única view da página — lista + drawer inline
        ----------------------------------------------- */
        public IActionResult Lista()
        {
            var lista = new LocalDAO().ListarPorProfissional(GetProfissionalId());
            return View(lista);
        }

        /* -----------------------------------------------
           POST /Local/Salvar
           Cria ou edita dependendo do campo "id":
             id == 0 ou null → novo local
             id > 0          → editar local existente
        ----------------------------------------------- */
        [HttpPost]
        public IActionResult Salvar(int id, string nome, string endereco)
        {
            try
            {
                LocalDAO dao = new LocalDAO();

                if (id > 0)
                {
                    // EDITAR
                    dao.Atualizar(new Local
                    {
                        Id = id,
                        Nome = nome,
                        Endereco = endereco
                    });

                    TempData["Sucesso"] = "Local atualizado com sucesso ✏️";
                }
                else
                {
                    // CRIAR
                    dao.Inserir(new Local
                    {
                        ProfissionalId = GetProfissionalId(),
                        Nome = nome,
                        Endereco = endereco
                    });

                    TempData["Sucesso"] = "Local criado com sucesso 📍";
                }
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao salvar o local.";
            }

            return RedirectToAction("Lista");
        }

        /* -----------------------------------------------
           GET /Local/Excluir/{id}
        ----------------------------------------------- */
        public IActionResult Excluir(int id)
        {
            try
            {
                new LocalDAO().Excluir(id);
                TempData["Sucesso"] = "Local removido com sucesso 🗑️";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Não foi possível excluir o local.";
            }

            return RedirectToAction("Lista");
        }



        public IActionResult Index()
        {
            LocalDAO dao = new LocalDAO();

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));
            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            var lista = dao.ListarPorProfissional(profissionalId);

            return View("Lista", lista);
        }

    }
}