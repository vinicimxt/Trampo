using BD_TRAMPO.DAO;
using Microsoft.AspNetCore.Mvc;


namespace BD_TRAMPO.Controllers
{
    [Perfil("cliente", "profissional")]
    public class AgendamentoController : BaseController
    {

        public IActionResult Novo(int servicoId, DateTime? data)
        {
            if (HttpContext.Session.GetString("UsuarioId") == null)
                return RedirectToAction("Login", "Usuario");

            ServicoDAO servicoDAO = new ServicoDAO();
            var servico = servicoDAO.BuscarPorId(servicoId);

            if (servico == null || !servico.Ativo)
            {
                TempData["Erro"] = "Serviço não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            DateTime dia = data ?? DateTime.Today;
            if (dia.Date < DateTime.Today || dia.Date > DateTime.Today.AddMonths(3)) return BadRequest("Data inválida.");

            // horários ocupados do dia
            AgendamentoDAO agDAO = new AgendamentoDAO();
            var intervalos = agDAO.IntervalosIndisponiveis(servico.ProfissionalId, dia);

            // disponibilidade do serviço
            DisponibilidadeDAO dispDAO = new DisponibilidadeDAO();
            var disponibilidade = dispDAO.BuscarPorServico(servicoId);

            // verifica se existe disponibilidade cadastrada
            if (disponibilidade == null || !disponibilidade.Any())
            {
                TempData["Erro"] = "Profissional ainda não definiu disponibilidade.";
                return RedirectToAction("Lista", "Profissional");
            }

            // apenas regras ativas
            var regras = disponibilidade
                .Where(d => d.Ativo)
                .OrderBy(d => d.DiaSemana)
                .ToList();

            // tradução dias da semana
            Dictionary<int, string> dias = new Dictionary<int, string>()
    {
        {0, "Domingo"},
        {1, "Segunda"},
        {2, "Terça"},
        {3, "Quarta"},
        {4, "Quinta"},
        {5, "Sexta"},
        {6, "Sábado"}
    };

            // texto geral dos dias disponíveis
            var nomesDias = regras
                .Select(r => r.DiaSemana)
                .Distinct()
                .OrderBy(d => d)
                .Select(d => dias[d])
                .ToList();
            ViewBag.DiasTexto = string.Join(", ", nomesDias);

            var horarios = RegrasAgenda.Horarios(regras, dia)
                .Where(h => h > DateTime.Now && !intervalos.Any(i => RegrasAgenda.Sobrepoe(h, h.AddHours(1), i.Inicio, i.Fim)))
                .Select(h => h.TimeOfDay).Distinct().OrderBy(h => h).ToList();
            ViewBag.DiaInvalido = !RegrasAgenda.Horarios(regras, dia).Any();
            ViewBag.HoraInicio = regras.Min(r => r.HoraInicio).ToString(@"hh\:mm");
            ViewBag.HoraFim = regras.Max(r => r.HoraFim).ToString(@"hh\:mm");
            ViewBag.NomeServico = servico.Nome;
            ViewBag.NomeProfissional = new ProfissionalDAO().BuscarPorId(servico.ProfissionalId)?.Nome;
            // dados da tela
            ViewBag.ServicoId = servicoId;
            ViewBag.Data = dia;
            ViewBag.Horarios = horarios.Distinct().OrderBy(x => x).ToList();
            ViewBag.Ocupados = new List<TimeSpan>();
            ViewBag.Atendimento = servico.Atendimento ?? "Local";

            return View();
        }
        [HttpPost]
        public IActionResult Salvar(int servicoId, DateTime data, TimeSpan? hora, string descricao, string rua, string numero, string bairro, string cidade, int? localId)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
                return RedirectToAction("Login", "Usuario");

            if (!ModelState.IsValid) return BadRequest("Dados do agendamento inválidos.");
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

            if (servico == null || !servico.Ativo)
                return BadRequest("Serviço não encontrado ou inativo.");

            var tipo = servico.Atendimento.ToLower();

            string enderecoCliente = null;

            if (tipo == "domicilio")
            {
                if (string.IsNullOrWhiteSpace(rua) ||
                    string.IsNullOrWhiteSpace(numero) ||
                    string.IsNullOrWhiteSpace(bairro) ||
                    string.IsNullOrWhiteSpace(cidade))
                {
                    return BadRequest("Preencha o endereço completo.");
                }

                enderecoCliente = $"{rua}, {numero} - {bairro}, {cidade}";
            }

            else if (tipo == "local")
            {
                localId = servico.LocalId; // automático
            }

            DateTime hoje = DateTime.Today;

            if (!hora.HasValue || hora < TimeSpan.Zero || hora >= TimeSpan.FromDays(1))
            {
                TempData["Erro"] = "Selecione um horário.";
                return RedirectToAction("Novo", new { servicoId, data });
            }

            if (data < hoje)
            {
                TempData["Erro"] = "Não é possível agendar no passado";
                return RedirectToAction("Novo", new { servicoId });
            }


            if (data > hoje.AddMonths(3))
            {
                TempData["Erro"] = "Você só pode agendar até três meses à frente.";
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

            var agendamento = new Agendamento
            {
                ClienteId = clienteId,
                ServicoId = servicoId,
                ProfissionalId = profissionalId,
                Data = data,
                Hora = hora.Value,
                Status = "Pendente",
                Descricao = descricao ?? "",
                EnderecoCliente = enderecoCliente,
                LocalId = localId
            };

            try
            {
                int agendamentoId = dao.Inserir(agendamento);
                TempData["Sucesso"] = $"Pedido #{agendamentoId} enviado para confirmação.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction("Novo", new { servicoId, data });
            }
            return RedirectToAction("Meus", "Agendamento");
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

        [Perfil("profissional")]
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


        [HttpPost]
        public IActionResult Confirmar(int id)
        {
            var acesso = ProtegerAgendamento(id, true); if (acesso != null) return acesso;
            AgendamentoDAO dao = new AgendamentoDAO();

            // atualiza status
            if (!dao.Confirmar(id, ProfissionalAtualId)) return Conflict("O pedido já foi alterado.");

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

            }

            return RedirectToAction("Recebidos");
        }

        [HttpPost]
        public IActionResult ConfirmarCliente(int id)
        {
            var acesso = ProtegerAgendamento(id, false); if (acesso != null) return acesso;
            AgendamentoDAO dao = new AgendamentoDAO();
            if (!dao.ConfirmarCliente(id, UsuarioAtualId)) return Conflict("A conclusão ainda não pode ser confirmada.");

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
        [HttpPost]
        public IActionResult Recusar(int id)
        {
            var acesso = ProtegerAgendamento(id, true); if (acesso != null) return acesso;
            AgendamentoDAO dao = new AgendamentoDAO();

            if (dao.BuscarPorId(id).StatusAtual() != "Pendente" || !dao.Cancelar(id, "CanceladoProfissional", UsuarioAtualId)) return Conflict("O pedido já foi alterado.");

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

        [HttpPost]
        public IActionResult Finalizar(int id, decimal valorFinal)
        {
            var acesso = ProtegerAgendamento(id, true); if (acesso != null) return acesso;
            AgendamentoDAO dao = new AgendamentoDAO();

            ServicoDAO servicoDAO = new ServicoDAO();


            var ag = dao.BuscarPorId(id);

            if (ag == null) return NotFound();
            var servico = servicoDAO.BuscarPorId(ag.ServicoId);
            if (servico == null) return NotFound();

            // PREÇO FIXO
            if (servico.TipoPreco == "Fixo")
            {
                valorFinal = servico.PrecoBase ?? 0;
            }

            // A COMBINAR
            else
            {
                if (valorFinal <= 0 || valorFinal > 99999999.99m)
                {
                    TempData["Erro"] = "Informe um valor válido.";
                    return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
                }
            }

            // 1 EXISTE?
            if (ag == null)
            {
                TempData["Erro"] = "Agendamento não encontrado.";
                return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
            }

            // 2 SEGURANÇA 
            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            if (ag.ProfissionalId != profissionalId)
            {
                TempData["Erro"] = "Você não tem permissão para isso.";
                return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
            }

            // 3 REGRA DE NEGÓCIO
            if (ag.StatusAtual() != "Confirmado")
            {
                TempData["Erro"] = "Só é possível finalizar agendamentos confirmados.";
                return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
            }


            //  4 REGRA DE TEMPO
            // DateTime dataHoraAgendamento = ag.Data.Date + ag.Hora;

            // if (dataHoraAgendamento > DateTime.Now)
            // {
            //     TempData["Erro"] = "Você só pode finalizar após o horário do atendimento.";
            //     return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
            // }


            if (valorFinal <= 0 || valorFinal > 99999999.99m)
            {
                TempData["Erro"] = "Informe um valor válido.";
                return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
            }

            var profissional = profDAO.BuscarPorId(profissionalId);

            decimal percentual =
                profissional.Plano == "Premium"
                ? 0.04m
                : 0.10m;
            // CALCULAR TAXA 
            decimal taxa = Math.Round(valorFinal * percentual, 2);

            decimal valorLiquido =
                valorFinal - taxa;

            // 5 EXECUTA
            if (ag.Data.Date + ag.Hora > DateTime.Now) return BadRequest("O atendimento ainda não começou.");
            if (!dao.Finalizar(id, profissionalId, valorFinal, taxa, valorLiquido))
                return Conflict("O pedido já foi alterado.");

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

            return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
        }

        [HttpPost]
        public IActionResult FinalizarProfissional(int id)
        {
            var acesso = ProtegerAgendamento(id, true); if (acesso != null) return acesso;
            AgendamentoDAO dao = new AgendamentoDAO();
            return Finalizar(id, 0);
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
                return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
            }

            if (ag.Status != "Pendente" && ag.Status != "Confirmado")
            {
                TempData["Erro"] = "Esse agendamento não pode ser cancelado.";
                return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
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
                return StatusCode(403);
            }

            if (!dao.Cancelar(id, status, usuarioId)) return Conflict("O pedido já foi alterado.");

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

            return RedirectToAction(HttpContext.Session.GetString("UsuarioTipo") == "profissional" ? "Recebidos" : "Meus");
        }


        public IActionResult DetalhesModal(int id)
        {
            if (id <= 0)
                return BadRequest("ID inválido");

            AgendamentoDAO dao = new AgendamentoDAO();
            var ag = dao.BuscarPorId(id);

            if (ag == null)
                return NotFound("Agendamento não encontrado");

            if (ag.UsuarioId != UsuarioAtualId &&
                !(HttpContext.Session.GetString("UsuarioTipo") == "profissional" && ag.ProfissionalId == ProfissionalAtualId))
                return StatusCode(403);
            return RedirectToAction(ag.UsuarioId == UsuarioAtualId ? "Meus" : "Recebidos");
        }





    }
}