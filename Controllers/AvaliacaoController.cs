using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers
{
    public class AvaliacaoController : Controller
    {
        public IActionResult Avaliar(int agendamentoId)
        {
            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            AgendamentoDAO agDAO = new AgendamentoDAO();
            var ag = agDAO.BuscarPorId(agendamentoId);

            //  Estado padrão (permite avaliar)
            ViewBag.PodeAvaliar = true;

            if (ag == null)
            {
                ViewBag.PodeAvaliar = false;
                ViewBag.Mensagem = "Esse atendimento não foi encontrado.";
            }
            else if (ag.UsuarioId != usuarioId)
            {
                ViewBag.PodeAvaliar = false;
                ViewBag.Mensagem = "Você não tem permissão para avaliar este atendimento.";
            }
            else if (!ag.FinalizadoProfissional || !ag.ConfirmadoCliente)
            {
                ViewBag.PodeAvaliar = false;
                ViewBag.Mensagem = "Você só pode avaliar após o atendimento ser concluído.";
            }
            else
            {
                AvaliacaoDAO avalDAO = new AvaliacaoDAO();

                if (avalDAO.JaAvaliou(agendamentoId,usuarioId))
                {
                    ViewBag.PodeAvaliar = false;
                    ViewBag.Mensagem = "Você já avaliou este atendimento.";
                }
            }
           
           //Debug
           // Console.WriteLine("AgendamentoId recebido: " + agendamentoId);

            // mesmo se não puder avaliar, ainda mandamos IDs (se existir)
            if (ag != null)
            {
                ViewBag.AgendamentoId = agendamentoId;
                ViewBag.ProfissionalId = ag.ProfissionalId;
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

            return RedirectToAction("PerfilPublico", "Profissional", new { id = a.ProfissionalId });
        }
    }
}