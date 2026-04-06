using Microsoft.AspNetCore.Mvc;


namespace BD_TRAMPO.Controllers
{
    public class AgendamentoController : Controller
    {
        public IActionResult Novo(int id)
        {
            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            ViewBag.ProfissionalId = id;
            return View();
        }

        [HttpPost]
        public IActionResult Salvar(int profissionalId, DateTime data, TimeSpan hora, string descricao)
        {

            // 🔥 pegar cliente real
            ClienteDAO clienteDAO = new ClienteDAO();
            AgendamentoDAO dao = new AgendamentoDAO();

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));
            int clienteId = clienteDAO.BuscarClienteIdPorUsuario(usuarioId);

            var bloqueio = clienteDAO.BuscarBloqueio(clienteId);

            if (bloqueio != null && bloqueio > DateTime.Now)
            {
                return Content("Você está bloqueado temporariamente.");
            }

            if (dao.ContarPendentes(clienteId) >= 5) // Bloqueia se tem mais de 5 agendamentos
            {
                return Content("Você já tem muitos agendamentos pendentes.");
            }
            var agendamento = new Agendamento
            {
                ClienteId = clienteId,
                ProfissionalId = profissionalId,
                Data = data,
                Hora = hora,
                Status = "Pendente",
                Descricao = descricao
            };


            dao.Inserir(agendamento);

            return RedirectToAction("Lista", "Profissional");
        }


        public IActionResult Meus()
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ClienteDAO clienteDAO = new ClienteDAO();
            int clienteId = clienteDAO.BuscarClienteIdPorUsuario(usuarioId);

            AgendamentoDAO dao = new AgendamentoDAO();
            var lista = dao.ListarPorCliente(clienteId);

            return View(lista);
        }

        public IActionResult Recebidos()
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            // 🔥 pegar profissional real
            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            AgendamentoDAO dao = new AgendamentoDAO();
            var lista = dao.ListarPorProfissional(profissionalId);

            return View(lista);
        }


        public IActionResult Confirmar(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();
            dao.AtualizarStatus(id, "Confirmado");

            return RedirectToAction("Recebidos");
        }

        public IActionResult Recusar(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();
            dao.AtualizarStatus(id, "Cancelado");

            return RedirectToAction("Recebidos");
        }

        [HttpPost]
        public IActionResult Cancelar(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();
            ClienteDAO clienteDAO = new ClienteDAO();

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));
            int clienteId = clienteDAO.BuscarClienteIdPorUsuario(usuarioId);

            dao.Cancelar(id);

            // 🚨 verifica excesso de cancelamentos
            if (dao.ContarCancelamentosHoje(clienteId) >= 3)
            {
                clienteDAO.BloquearCliente(clienteId);
                return Content("Você foi bloqueado por excesso de cancelamentos.");
            }

            

            return Redirect(Request.Headers["Referer"].ToString());
        }

    }
}