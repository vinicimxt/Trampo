using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers
{
    [Perfil("cliente", "profissional")]
    public class AvaliacaoController : BaseController
    {
        public IActionResult Avaliar(int agendamentoId)
        {
            ViewBag.PodeAvaliar = false;

            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                ViewBag.Mensagem = "Você precisa estar logado para avaliar.";
                return RedirectToAction("Meus", "Agendamento");
            }

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            AgendamentoDAO agDAO = new AgendamentoDAO();
            var ag = agDAO.BuscarPorId(agendamentoId);

            if (ag == null)
            {
                ViewBag.Mensagem = "Esse atendimento não foi encontrado.";
            }
            else if (ag.UsuarioId != usuarioId)
            {
                ViewBag.Mensagem = "Você não tem permissão para avaliar este atendimento.";
            }
            else if (!ag.FinalizadoProfissional || !ag.ConfirmadoCliente)
            {
                ViewBag.Mensagem = "Você só pode avaliar após o atendimento ser concluído.";
            }
            else
            {
                AvaliacaoDAO avalDAO = new AvaliacaoDAO();

                if (avalDAO.JaAvaliou(agendamentoId, usuarioId))
                {
                    ViewBag.Mensagem = "Você já avaliou este atendimento.";
                }
                else
                {
                    ViewBag.PodeAvaliar = true;
                    ViewBag.AgendamentoId = agendamentoId;
                    ViewBag.ProfissionalId = ag.ProfissionalId;
                }
            }

            return RedirectToAction("Meus", "Agendamento");
        }


        [HttpPost]
        public IActionResult Salvar(Avaliacao a)
        {


            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            var ag = new AgendamentoDAO().BuscarPorId(a.AgendamentoId);
            if (ag == null) return NotFound();
            if (ag.UsuarioId != usuarioId) return StatusCode(403);
            if (ag.StatusAtual() != "Finalizado" || !ag.PodeAvaliar())
                return BadRequest("O atendimento ainda não foi concluído.");
            if (a.ProfissionalId != ag.ProfissionalId) return BadRequest("Profissional inválido.");
            if (a.Nota < 1 || a.Nota > 5 || (a.Comentario?.Length ?? 0) > 500)
                return BadRequest("Avaliação inválida.");
            if (new AvaliacaoDAO().JaAvaliou(a.AgendamentoId, usuarioId))
                return Conflict("Este atendimento já foi avaliado.");
            a.UsuarioId = usuarioId;
            a.ProfissionalId = ag.ProfissionalId;

            AvaliacaoDAO dao = new AvaliacaoDAO();
            try { dao.Inserir(a); }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            { return Conflict("Este atendimento já foi avaliado."); }

            TempData["Sucesso"] = "Obrigado pela avaliação.⭐";

            return RedirectToAction("Meus", "Agendamento");
        }
    }
}