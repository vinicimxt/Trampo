using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{
    [Perfil("profissional")]
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
        public IActionResult Lista(bool abrir = false)
        {
            ViewBag.AbrirDrawer = abrir;
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
            if (id < 0 || string.IsNullOrWhiteSpace(endereco) || endereco.Length > 255 || (nome?.Length ?? 0) > 100)
                return BadRequest("Local inválido.");
            if (id > 0) { var acesso = ProtegerLocal(id); if (acesso != null) return acesso; }
            try
            {
                LocalDAO dao = new LocalDAO();

                if (id > 0)
                {
                    // EDITAR
                    bool atualizado = dao.Atualizar(new Local
                    {
                        Id = id,
                        Nome = nome,
                        Endereco = endereco
                    });

                    if (atualizado) TempData["Sucesso"] = "Local atualizado com sucesso ✏️";
                    else TempData["Erro"] = "Este endereço faz parte do histórico de agendamentos. Cadastre outro local para novos atendimentos.";
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

                    TempData["Sucesso"] = "Local criado com sucesso ✔";
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
        [HttpPost]
        public IActionResult Excluir(int id)
        {
            var acesso = ProtegerLocal(id); if (acesso != null) return acesso;
            try
            {
                if (new LocalDAO().Excluir(id))
                    TempData["Sucesso"] = "Local removido com sucesso ✔";
                else TempData["Erro"] = "Local vinculado a serviço ou agendamento. O endereço foi preservado; cadastre outro local para novos atendimentos.";
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