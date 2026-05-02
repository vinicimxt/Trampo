using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers
{
    public class AvaliacaoController : Controller
    {
        public IActionResult Avaliar(int agendamentoId)
        {
            ViewBag.PodeAvaliar = false;

            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                ViewBag.Mensagem = "Você precisa estar logado para avaliar.";
                return View();
            }

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            AgendamentoDAO agDAO = new AgendamentoDAO();
            var ag = agDAO.BuscarPorId(agendamentoId);

            if (ag == null)
            {
                ViewBag.Mensagem = "Esse atendimento não foi encontrado.";
            }
            else if (ag.ClienteId != usuarioId)
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

            TempData["Erro"] = "Obrigado pela avaliação.";

            return RedirectToAction("Meus", "Agendamento");
        }
    }
}