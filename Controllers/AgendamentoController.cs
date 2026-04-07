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

        public IActionResult Finalizar(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();

            var ag = dao.BuscarPorId(id);

            // 1️⃣ EXISTE?
            if (ag == null)
            {
                return Content("Agendamento não encontrado.");
            }

            // 2️⃣ 🔐 SEGURANÇA (ANTES DE TUDO)
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            if (ag.ProfissionalId != profissionalId)
            {
                return Content("Você não tem permissão para isso.");
            }

            // 3️⃣ REGRA DE NEGÓCIO
            if (ag.Status != "Confirmado")
            {
                return Content("Só é possível finalizar agendamentos confirmados.");
            }

            // 4️⃣ REGRA DE TEMPO
            DateTime dataHoraAgendamento = ag.Data.Date + ag.Hora;

            if (dataHoraAgendamento > DateTime.Now)
            {
                return Content("Você só pode finalizar após o horário do atendimento.");
            }

            // 5️⃣ EXECUTA
            dao.Finalizar(id);

            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        public IActionResult Cancelar(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();
            ClienteDAO clienteDAO = new ClienteDAO();
            ProfissionalDAO profDAO = new ProfissionalDAO();

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            var ag = dao.BuscarPorId(id);

            if (ag.Status != "Pendente" && ag.Status != "Confirmado")
            {
                return Content("Esse agendamento não pode ser cancelado.");
            }
            if (ag == null)
                return Content("Agendamento não encontrado.");

            string status = "";

            // 🔐 VERIFICA SE É CLIENTE
            int clienteId = clienteDAO.BuscarClienteIdPorUsuario(usuarioId);
            if (clienteId == ag.ClienteId)
            {
                status = "CanceladoCliente";
            }

            // 🔐 VERIFICA SE É PROFISSIONAL
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);
            if (profissionalId == ag.ProfissionalId)
            {
                status = "CanceladoProfissional";
            }

            if (string.IsNullOrEmpty(status))
            {
                return Content("Você não tem permissão para cancelar.");
            }

            dao.Cancelar(id, status);

            return Redirect(Request.Headers["Referer"].ToString());
        }

    }
}