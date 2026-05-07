using BD_TRAMPO.DAO;
using Microsoft.AspNetCore.Mvc;


namespace BD_TRAMPO.Controllers
{
    public class AgendamentoController : BaseController
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
                TempData["Erro"] = "Serviço não encontrado.";
                return RedirectToAction("Novo", new { servicoId });
            }

            ViewBag.ServicoId = servicoId;
            ViewBag.Atendimento = servico.Atendimento ?? "Local";

            DateTime dia = data ?? DateTime.Today;

            AgendamentoDAO dao = new AgendamentoDAO();
            var ocupados = dao.BuscarHorariosOcupados(servicoId, dia);

            List<TimeSpan> todos = new List<TimeSpan>();

            for (int h = 1; h <= 24; h++)
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
            {
                TempData["Erro"] = "Serviço inválido.";
                return RedirectToAction("Novo", new { servicoId });
            }

            //  auto agendamento
            if (profissionalLogadoId == profissionalId)
            {
                TempData["Erro"] = "Você não pode agendar seu próprio serviço.";
                return RedirectToAction("Novo", new { servicoId });
            }

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
                    return Content("Preencha o endereço completo.");
                }

                enderecoCliente = $"{rua}, {numero} - {bairro}, {cidade}";
            }

            else if (tipo == "local")
            {
                localId = servico.LocalId; // automático
            }

            DateTime hoje = DateTime.Today;

            if (data < hoje)
            {
                TempData["Erro"] = "Não é possível agendar no passado";
                return RedirectToAction("Novo", new { servicoId });
            }


            if (data > hoje.AddDays(366))
            {
                TempData["Erro"] = "Você só pode agendar até 1 ano a frente.";
                return RedirectToAction("Novo", new { servicoId });
            }


            var bloqueio = clienteDAO.BuscarBloqueio(clienteId);

            if (bloqueio != null && bloqueio > DateTime.Now)
            {
                TempData["Erro"] = "Você está bloqueado temporariamente.";
                return RedirectToAction("Novo", new { servicoId });
            }


            if (dao.ContarPendentes(clienteId) >= 5)
            {
                TempData["Erro"] = "Você tem muitos agendamentos pendentes.";
                return RedirectToAction("Novo", new { servicoId });
            }


            if (dao.HorarioOcupado(servicoId, data, hora))
            {
                TempData["Erro"] = "Esse horário já está ocupado.";
                return RedirectToAction("Novo", new { servicoId, data });
            }

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

            int agendamentoId = dao.Inserir(agendamento);

            TempData["Sucesso"] = $"Agendamento confirmado para {data:dd/MM/yyyy} às {hora}";

            int clienteUsuarioId = usuarioId;

            int profissionalUsuarioId = profDAO.BuscarUsuarioId(profissionalId);

            NotificacaoDAO notif = new NotificacaoDAO();

            notif.Inserir(new Notificacao
            {
                UsuarioId = profissionalUsuarioId,
                Titulo = "Novo agendamento 📅",
                Mensagem = $"Novo agendamento para {data:dd/MM} às {hora}",
                Tipo = "Agendamento",
                ReferenciaId = agendamentoId
            });

            // cliente recebe confirmação do envio
            notif.Inserir(new Notificacao
            {
                UsuarioId = clienteUsuarioId,
                Titulo = "Agendamento enviado 📨",
                Mensagem = "Seu pedido foi enviado para confirmação do profissional.",
                Tipo = "Agendamento",
                ReferenciaId = agendamentoId
            });

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


        public IActionResult Meus(string sucesso)
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ClienteDAO clienteDAO = new ClienteDAO();
            int clienteId = clienteDAO.BuscarClienteIdPorUsuario(usuarioId);

            AgendamentoDAO dao = new AgendamentoDAO();
            var lista = dao.ListarPorCliente(clienteId, usuarioId);

            ViewBag.Sucesso = TempData["Sucesso"];

            return View(lista);
        }

        public IActionResult Recebidos()
        {
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            //  pegar profissional real
            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            AgendamentoDAO dao = new AgendamentoDAO();
            var lista = dao.ListarPorProfissional(profissionalId);

            return View(lista);
        }


        public IActionResult Confirmar(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();

            // atualiza status
            dao.AtualizarStatus(id, "Confirmado");
            dao.ConfirmarProfissional(id);

            //  BUSCA O AGENDAMENTO
            var ag = dao.BuscarPorId(id);

            if (ag != null)
            {
                //  pega o UsuarioId do cliente
                ClienteDAO clienteDAO = new ClienteDAO();
                int clienteUsuarioId = clienteDAO.BuscarUsuarioId(ag.ClienteId);

                //  cria notificação
                NotificacaoDAO notif = new NotificacaoDAO();
                notif.Inserir(new Notificacao
                {
                    UsuarioId = clienteUsuarioId,
                    Titulo = "Agendamento confirmado ✔",
                    Mensagem = $"Seu agendamento para {ag.Data:dd/MM} às {ag.Hora} foi confirmado",
                    Tipo = "Agendamento",
                    ReferenciaId = id
                });

                notif.Inserir(new Notificacao
                {
                    UsuarioId = clienteUsuarioId,
                    Titulo = "Agendamento confirmado ✔",
                    Mensagem = $"Seu agendamento para {ag.Data:dd/MM} às {ag.Hora} foi confirmado.",
                    Tipo = "Agendamento",
                    ReferenciaId = id
                });

            }

            return RedirectToAction("Recebidos");
        }

        public IActionResult ConfirmarCliente(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();
            dao.ConfirmarCliente(id);

            var ag = dao.BuscarPorId(id);

            if (ag != null)
            {
                ProfissionalDAO profDAO = new ProfissionalDAO();

                int profissionalUsuarioId =
                    profDAO.BuscarUsuarioId(ag.ProfissionalId);

                NotificacaoDAO notif = new NotificacaoDAO();

                notif.Inserir(new Notificacao
                {
                    UsuarioId = profissionalUsuarioId,
                    Titulo = "Serviço finalizado ✔",
                    Mensagem = "O cliente confirmou a conclusão do atendimento.",
                    Tipo = "Finalizacao",
                    ReferenciaId = id
                });
            }

            return RedirectToAction("Meus");
        }
        public IActionResult Recusar(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();

            dao.AtualizarStatus(id, "CanceladoProfissional");

            var ag = dao.BuscarPorId(id);

            if (ag != null)
            {
                ClienteDAO clienteDAO = new ClienteDAO();

                int clienteUsuarioId =
                    clienteDAO.BuscarUsuarioId(ag.ClienteId);

                NotificacaoDAO notif = new NotificacaoDAO();

                notif.Inserir(new Notificacao
                {
                    UsuarioId = clienteUsuarioId,
                    Titulo = "Agendamento recusado ❌",
                    Mensagem = "O profissional recusou seu agendamento.",
                    Tipo = "Cancelamento",
                    ReferenciaId = id
                });
            }

            return RedirectToAction("Recebidos");
        }
        public IActionResult Finalizar(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();

            var ag = dao.BuscarPorId(id);

            // 1 EXISTE?
            if (ag == null)
            {
                TempData["Erro"] = "Agendamento não encontrado.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // 2 SEGURANÇA 
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            if (ag.ProfissionalId != profissionalId)
            {
                TempData["Erro"] = "Você não tem permissão para isso.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // 3 REGRA DE NEGÓCIO
            if (ag.Status != "Confirmado")
            {
                TempData["Erro"] = "Só é possível finalizar agendamentos confirmados.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            //  4 REGRA DE TEMPO
            DateTime dataHoraAgendamento = ag.Data.Date + ag.Hora;

            if (dataHoraAgendamento > DateTime.Now)
            {
                TempData["Erro"] = "Você só pode finalizar após o horário do atendimento.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            // 5 EXECUTA
            dao.Finalizar(id);

            ClienteDAO clienteDAO = new ClienteDAO();

            int clienteUsuarioId =
                clienteDAO.BuscarUsuarioId(ag.ClienteId);

            NotificacaoDAO notif = new NotificacaoDAO();

            notif.Inserir(new Notificacao
            {
                UsuarioId = clienteUsuarioId,
                Titulo = "Atendimento finalizado ✔",
                Mensagem = "O profissional marcou o atendimento como concluído.",
                Tipo = "Finalizacao",
                ReferenciaId = id
            });

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult FinalizarProfissional(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();
            dao.FinalizarProfissional(id);
            return RedirectToAction("Recebidos");
        }

        [HttpPost]
        public IActionResult Cancelar(int id)
        {
            AgendamentoDAO dao = new AgendamentoDAO();
            ClienteDAO clienteDAO = new ClienteDAO();
            ProfissionalDAO profDAO = new ProfissionalDAO();

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            var ag = dao.BuscarPorId(id);

            if (ag == null)
            {
                TempData["Erro"] = "Agendamento não encontrado.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            if (ag.Status != "Pendente" && ag.Status != "Confirmado")
            {
                TempData["Erro"] = "Esse agendamento não pode ser cancelado.";
                return Redirect(Request.Headers["Referer"].ToString());
            }


            string status = "";

            //  VERIFICA SE É CLIENTE
            int clienteId = clienteDAO.BuscarClienteIdPorUsuario(usuarioId);
            if (clienteId == ag.ClienteId)
            {
                status = "CanceladoCliente";
            }

            //  VERIFICA SE É PROFISSIONAL
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);
            if (profissionalId == ag.ProfissionalId)
            {
                status = "CanceladoProfissional";
            }

            if (string.IsNullOrEmpty(status))
            {
                TempData["Erro"] = "Você não tem permissão para cancelar.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            dao.Cancelar(id, status);

            NotificacaoDAO notif = new NotificacaoDAO();

            if (status == "CanceladoCliente")
            {
                int profissionalUsuarioId =
                    profDAO.BuscarUsuarioId(ag.ProfissionalId);

                notif.Inserir(new Notificacao
                {
                    UsuarioId = profissionalUsuarioId,
                    Titulo = "Agendamento cancelado ❌",
                    Mensagem = "Um cliente cancelou um agendamento.",
                    Tipo = "Cancelamento",
                    ReferenciaId = id
                });

                notif.Inserir(new Notificacao
                {
                    UsuarioId = usuarioId,
                    Titulo = "Agendamento cancelado ❌",
                    Mensagem = "Você cancelou o agendamento.",
                    Tipo = "Cancelamento",
                    ReferenciaId = id
                });
            }
            else if (status == "CanceladoProfissional")
            {
                int clienteUsuarioId =
                    clienteDAO.BuscarUsuarioId(ag.ClienteId);

                notif.Inserir(new Notificacao
                {
                    UsuarioId = clienteUsuarioId,
                    Titulo = "Agendamento cancelado ❌",
                    Mensagem = "Seu agendamento foi cancelado pelo profissional.",
                    Tipo = "Cancelamento",
                    ReferenciaId = id
                });

                notif.Inserir(new Notificacao
                {
                    UsuarioId = usuarioId,
                    Titulo = "Agendamento cancelado ❌",
                    Mensagem = "Você cancelou um agendamento.",
                    Tipo = "Cancelamento",
                    ReferenciaId = id
                });
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }


        public IActionResult DetalhesModal(int id)
        {
            if (id <= 0)
                return BadRequest("ID inválido");

            AgendamentoDAO dao = new AgendamentoDAO();
            var ag = dao.BuscarPorId(id);

            if (ag == null)
                return NotFound("Agendamento não encontrado");

            return PartialView("_DetalhesAgendamentoModal", ag);
        }


    }
}