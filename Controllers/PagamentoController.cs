using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO.Controllers
{

    public class PagamentoController : Controller
    {

        public IActionResult CheckoutPremium()
        {
            var usuarioId =
                HttpContext.Session.GetString("UsuarioId");

            var tipo =
                HttpContext.Session.GetString("UsuarioTipo");

            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            if (tipo != "profissional")
            {
                TempData["Erro"] =
                    "Apenas profissionais podem assinar.";

                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        // [HttpPost]
        // public IActionResult AssinarPremium()
        // {
        //     string usuarioSession = HttpContext.Session.GetString("UsuarioId");

        //     if (string.IsNullOrEmpty(usuarioSession))
        //         return RedirectToAction("Login", "Usuario");

        //     int usuarioId = int.Parse(usuarioSession);

        //     ProfissionalDAO dao = new ProfissionalDAO();

        //     int profissionalId = dao.BuscarPorUsuario(usuarioId);

        //     dao.AtivarPremium(profissionalId);

        //     TempData["Sucesso"] = "Plano Premium ativado com sucesso!";

        //     return RedirectToAction("Dashboard");
        // }


        [HttpPost]
        public IActionResult ConfirmarPremium()
        {
            string usuarioSession =
                HttpContext.Session.GetString("UsuarioId");

            string nomeUsuario =
                HttpContext.Session.GetString("UsuarioNome");

            if (string.IsNullOrEmpty(usuarioSession))
                return RedirectToAction("Login", "Usuario");

            int usuarioId = int.Parse(usuarioSession);

            ProfissionalDAO dao = new ProfissionalDAO();

            int profissionalId =
                dao.BuscarPorUsuario(usuarioId);

            // já é premium?
            if (dao.EhPremium(profissionalId))
            {
                TempData["Erro"] =
                    "Você já possui o plano premium.";

                return RedirectToAction(
                    "Dashboard",
                    "Profissional"
                );

            }

            AssinaturaDAO assDAO = new AssinaturaDAO();

            assDAO.Inserir(
                profissionalId,
                "Premium",
                29.90m,
                "Pago",
                "Cartão"
            );

            dao.AtivarPremium(profissionalId);

            NotificacaoDAO notifDAO = new NotificacaoDAO();

            notifDAO.Inserir(new Notificacao
            {
                UsuarioId = 1,
                Titulo = "💎 Novo Premium",
                Mensagem = $"{nomeUsuario} assinou o plano Premium.",
                Tipo = "admin",
                ReferenciaId = profissionalId
            });

            TempData["Sucesso"] =
                "Plano Premium ativado com sucesso!";

            return RedirectToAction(
                "Dashboard",
                "Profissional"
            );
        }

    }

}