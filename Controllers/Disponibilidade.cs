using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;




namespace BD_TRAMPO.Controllers
{
    public class DisponibilidadeController : Controller
    {
        public IActionResult Index()
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            DisponibilidadeDAO dao = new DisponibilidadeDAO();
            var lista = dao.BuscarPorServico(profissionalId);

            ViewBag.Disponibilidades = lista;

            return View();
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Salvar(int diaSemana, TimeSpan horaInicio, TimeSpan horaFim)
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            DisponibilidadeDAO dao = new DisponibilidadeDAO();

            dao.Inserir(new Disponibilidade
            {
                ProfissionalId = profissionalId,
                DiaSemana = diaSemana,
                HoraInicio = horaInicio,
                HoraFim = horaFim
            });

            TempData["Sucesso"] = "Disponibilidade cadastrada!";
            return RedirectToAction("Index");
        }

        public IActionResult Desativar(int id)
        {
            DisponibilidadeDAO dao = new DisponibilidadeDAO();
            dao.Desativar(id);

            TempData["Sucesso"] = "Horário removido!";
            return RedirectToAction("Index");
        }
    }

}