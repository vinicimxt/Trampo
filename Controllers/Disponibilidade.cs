using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;




namespace BD_TRAMPO.Controllers
{
    [Perfil("profissional")]
    public class DisponibilidadeController : BaseController
    {
        public IActionResult Index()
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            DisponibilidadeDAO dao = new DisponibilidadeDAO();
            var lista = dao.BuscarPorServico(profissionalId);

            ViewBag.Disponibilidades = lista;

            return RedirectToAction("MeusServicos", "Profissional");
        }

        public IActionResult Criar()
        {
            return RedirectToAction("MeusServicos", "Profissional");
        }

        [HttpPost]
        public IActionResult Salvar(int servicoId, int diaSemana, TimeSpan horaInicio, TimeSpan horaFim)
        {
            var acesso = ProtegerServico(servicoId); if (acesso != null) return acesso;
            if (diaSemana < 0 || diaSemana > 6 || horaInicio < TimeSpan.Zero ||
                horaFim < TimeSpan.Zero || horaInicio >= TimeSpan.FromDays(1) ||
                horaFim >= TimeSpan.FromDays(1) || horaInicio == horaFim)
                return BadRequest("Disponibilidade inválida.");
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            DisponibilidadeDAO dao = new DisponibilidadeDAO();

            dao.Inserir(new Disponibilidade
            {
                ProfissionalId = profissionalId, ServicoId = servicoId,
                DiaSemana = diaSemana,
                HoraInicio = horaInicio,
                HoraFim = horaFim
            });

            TempData["Sucesso"] = "Disponibilidade cadastrada!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Desativar(int id)
        {
            DisponibilidadeDAO dao = new DisponibilidadeDAO();
            if (!dao.Desativar(id, ProfissionalAtualId)) return NotFound();

            TempData["Sucesso"] = "Horário removido!";
            return RedirectToAction("Index");
        }
    }

}