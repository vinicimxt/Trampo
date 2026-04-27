using BD_TRAMPO.DAO;
using Microsoft.AspNetCore.Mvc;


namespace BD_TRAMPO.Controllers
{
    public class AgendamentoController : Controller
    {

        public IActionResult Novo(int servicoId, DateTime? data)
        {
            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            ServicoDAO servicoDAO = new ServicoDAO();
            var servico = servicoDAO.BuscarPorId(servicoId);

            LocalDAO localDAO = new LocalDAO();
            ViewBag.Locais = localDAO.ListarPorProfissional(servico.ProfissionalId);
            if (servico == null)
            {
                return Content("Serviço não encontrado.");
            }

            ViewBag.ServicoId = servicoId;
            ViewBag.Atendimento = servico.Atendimento ?? "Local";

            DateTime dia = data ?? DateTime.Today;

            AgendamentoDAO dao = new AgendamentoDAO();
            var ocupados = dao.BuscarHorariosOcupados(servicoId, dia);

            List<TimeSpan> todos = new List<TimeSpan>();

            for (int h = 8; h <= 24; h++)
            {
                todos.Add(new TimeSpan(h, 0, 0));
            }

            ViewBag.Data = dia;
            ViewBag.Horarios = todos;
            ViewBag.Ocupados = ocupados;

            return View();
        }


        public IActionResult Salvar(int servicoId, DateTime data, TimeSpan hora, string descricao, string rua, string numero, string bairro, string cidade, int? localId)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
                return RedirectToAction("Login", "Usuario");

            int usuarioId = int.Parse(usuarioIdStr);

            ClienteDAO clienteDAO = new ClienteDAO();
            AgendamentoDAO dao = new AgendamentoDAO();
            ServicoDAO servicoDAO = new ServicoDAO();
            ProfissionalDAO profDAO = new ProfissionalDAO();

            int clienteId = clienteDAO.BuscarClienteIdPorUsuario(usuarioId);
            int profissionalId = servicoDAO.BuscarProfissionalId(servicoId);
            int profissionalLogadoId = profDAO.BuscarPorUsuario(usuarioId);

            if (clienteId == 0)
            {
                clienteDAO.Inserir(usuarioId);
                clienteId = clienteDAO.BuscarClienteIdPorUsuario(usuarioId);
            }
            if (profissionalId == 0)
                return Content("Erro: serviço inválido.");

            //  auto agendamento
            if (profissionalLogadoId == profissionalId)
                return Content("Você não pode agendar seu próprio serviço.");

            var servico = servicoDAO.BuscarPorId(servicoId);

            if (servico == null)
                return Content("Serviço não encontrado.");

            var tipo = servico.Atendimento.ToLower();

            string enderecoCliente = null;

            if (tipo == "domicilio")
            {
                if (string.IsNullOrWhiteSpace(rua) ||
                    string.IsNullOrWhiteSpace(numero) ||
                    string.IsNullOrWhiteSpace(bairro) ||
                    string.IsNullOrWhiteSpace(cidade))
                {
                    return Content("Preencha o endereço completo para atendimento a domicílio.");
                }

                enderecoCliente = $"{rua}, {numero} - {bairro}, {cidade}";
            }

            if (tipo == "local" && localId == null)
                return Content("Selecione um local.");

            DateTime hoje = DateTime.Today;

            if (data < hoje)
                return Content("Não é possível agendar no passado.");

            if (data > hoje.AddDays(366))
                return Content("Você só pode agendar até 1 ano à frente.");

            var bloqueio = clienteDAO.BuscarBloqueio(clienteId);

            if (bloqueio != null && bloqueio > DateTime.Now)
                return Content("Você está bloqueado temporariamente.");

            if (dao.ContarPendentes(clienteId) >= 5)
                return Content("Você já tem muitos agendamentos pendentes.");

            if (dao.HorarioOcupado(servicoId, data, hora))
                return Content("Esse horário já está ocupado.");

            var agendamento = new Agendamento
            {
                ClienteId = clienteId,
                ServicoId = servicoId,
                ProfissionalId = profissionalId,
                Data = data,
                Hora = hora,
                Status = "Pendente",
                Descricao = descricao ?? "",
                EnderecoCliente = enderecoCliente,
                LocalId = localId
            };

            dao.Inserir(agendamento);

            TempData["Sucesso"] = $"Agendamento confirmado para {data:dd/MM/yyyy} às {hora}";

            return RedirectToAction("Meus", "Agendamento");
        }
        private string ValidarAgendamento(Agendamento ag, Servico servico, int usuarioId)
        {
            ProfissionalDAO profDAO = new ProfissionalDAO();
            ServicoDAO servDAO = new ServicoDAO();

            int profissionalLogadoId = profDAO.BuscarPorUsuario(usuarioId);
            int profissionalDoServico = servDAO.BuscarProfissionalId(ag.ServicoId);

            if (profissionalLogadoId == profissionalDoServico)
                return "Você não pode agendar seu próprio serviço.";

            if (servico.Atendimento == "Domicilio" && string.IsNullOrEmpty(ag.EnderecoCliente))
                return "Endereço é obrigatório para atendimento a domicílio.";

            return null;
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
            dao.ConfirmarProfissional(id);
            return RedirectToAction("Recebidos");
        }

        public IActionResult ConfirmarCliente(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();
            dao.ConfirmarCliente(id);

            return RedirectToAction("Meus");
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