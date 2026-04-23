using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers
{
    public class AvaliacaoController : Controller
    {
    public IActionResult Avaliar(int agendamentoId)
{
    //  usuário precisa estar logado
    if (HttpContext.Session.GetString("UsuarioId") == null)
    {
        return RedirectToAction("Login", "Usuario");
    }

    int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

    AgendamentoDAO agDAO = new AgendamentoDAO();
    var ag = agDAO.BuscarPorId(agendamentoId);

    //  valida existência
    if (ag == null)
    {
        return Content("Agendamento não encontrado.");
    }

    //  segurança: só quem contratou pode avaliar
    if (ag.UsuarioId != usuarioId)
    {
        return Content("Você não tem permissão para avaliar este atendimento.");
    }

    //  só pode avaliar se estiver concluído
    if (ag.Status != "Concluido")
    {
        return Content("Você só pode avaliar após o atendimento ser concluído.");
    }

    //  evitar avaliação duplicada
    AvaliacaoDAO avalDAO = new AvaliacaoDAO();
    if (avalDAO.JaAvaliou(agendamentoId))
    {
        return Content("Você já avaliou este atendimento.");
    }

    //  envia dados pra view
    ViewBag.AgendamentoId = agendamentoId;
    ViewBag.ProfissionalId = ag.ProfissionalId;

    return View();
}
        [HttpPost]
        public IActionResult Salvar(Avaliacao a)
        {

            
            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            a.UsuarioId = usuarioId;

            AvaliacaoDAO dao = new AvaliacaoDAO();
            dao.Inserir(a);

            return RedirectToAction("PerfilPublico", "Profissional", new { id = a.ProfissionalId });
        }
    }
}