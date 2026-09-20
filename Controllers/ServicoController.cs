using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;
namespace BD_TRAMPO.Controllers
{

    [Perfil("profissional")]
    public class ServicoController : BaseController
    {
        private string? ValidarServico(string nome, string atendimento, int? localId, string link,
            string tipoPreco, decimal? preco, string dias, TimeSpan inicio, TimeSpan fim)
        {
            if (string.IsNullOrWhiteSpace(nome) || nome.Length > 100 ||
                !new[] { "Local", "Domicilio", "Online" }.Contains(atendimento) ||
                !new[] { "Fixo", "Combinar" }.Contains(tipoPreco))
                return "Dados do serviço inválidos.";
            if (tipoPreco == "Fixo" && (!preco.HasValue || preco <= 0 || preco > 99999999.99m))
                return "Preço inválido.";
            if (atendimento == "Online" && (!Seguranca.UrlHttpValida(link) || link.Length > 255))
                return "Informe um link HTTP ou HTTPS válido.";
            if (atendimento == "Local" && (!localId.HasValue ||
                new LocalDAO().BuscarPorId(localId.Value)?.ProfissionalId != ProfissionalAtualId))
                return "O local deve pertencer ao profissional.";
            if (inicio < TimeSpan.Zero || inicio >= TimeSpan.FromDays(1) ||
                fim < TimeSpan.Zero || fim >= TimeSpan.FromDays(1) || inicio == fim ||
                string.IsNullOrWhiteSpace(dias) ||
                dias.Split(',').Any(d => !int.TryParse(d, out var dia) || dia < 0 || dia > 6))
                return "Disponibilidade inválida.";
            return null;
        }
        public IActionResult Criar()
        {
            var auth = Proteger();
            if (auth != null) return auth;

            int usuarioId = int.Parse(HttpContext.Session.GetString("UsuarioId"));

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            LocalDAO localDAO = new LocalDAO();
            ViewBag.Locais = localDAO.ListarPorProfissional(profissionalId);

            CategoriaDAO catDAO = new CategoriaDAO();
            ViewBag.Categorias = catDAO.Listar();

            ViewBag.Subcategorias = new List<Subcategoria>(); // começa vazio
            ServicoDAO servicoDAO = new ServicoDAO();
            ViewBag.TotalServicos = servicoDAO.ContarPorProfissional(profissionalId);

            AgendamentoDAO agendamentoDAO = new AgendamentoDAO();
            ViewBag.PedidosPendentes = agendamentoDAO.ContarPendentesProfissional(profissionalId);

            return View();
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public JsonResult GetSubcategorias(int categoriaId)
        {
            SubcategoriaDAO dao = new SubcategoriaDAO();
            var lista = dao.ListarPorCategoria(categoriaId);

            return Json(lista);
        }

        [HttpPost]
        public IActionResult Salvar(string nome, int subcategoriaId, string descricao, string atendimento, int? localId, string linkOnline, string diasSemana, TimeSpan horaInicio, TimeSpan horaFim, string tipoPreco,
        decimal? precoBase)
        {
            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (usuarioIdStr == null)
                return RedirectToAction("Login", "Usuario");

            int usuarioId = int.Parse(usuarioIdStr);

            ProfissionalDAO profDAO = new ProfissionalDAO();
            int profissionalId = profDAO.BuscarPorUsuario(usuarioId);

            if (profissionalId == 0)
            {
                TempData["Erro"] = "Você precisa ser um profissional para criar serviços.";
                return RedirectToAction("Cadastro", "Usuario");
            }

            var erro = ValidarServico(nome, atendimento, localId, linkOnline, tipoPreco, precoBase, diasSemana, horaInicio, horaFim);
            if (erro != null) return BadRequest(erro);
            if (!new SubcategoriaDAO().ListarTodas().Any(x => x.Id == subcategoriaId))
                return BadRequest("Subcategoria inválida.");
            if (atendimento != "Local") localId = null;
            if (atendimento != "Online") linkOnline = null;
            //  VALIDAÇÕES

            if (tipoPreco == "Fixo" && (!precoBase.HasValue || precoBase <= 0))
            {
                TempData["Erro"] = "Informe um preço válido.";
                return RedirectToAction("Criar");
            }

            if (string.IsNullOrWhiteSpace(nome))
            {
                TempData["Erro"] = "Informe o nome do serviço.";
                return RedirectToAction("Criar");
            }

            if (atendimento == "Online")
            {
                if (string.IsNullOrWhiteSpace(linkOnline))
                {
                    TempData["Erro"] = "Informe o link do atendimento online.";
                    return RedirectToAction("Criar");
                }

                if (!Uri.IsWellFormedUriString(linkOnline, UriKind.Absolute))
                {
                    TempData["Erro"] = "Informe um link válido.";
                    return RedirectToAction("Criar");
                }
            }

            if (atendimento == "Local")
            {
                LocalDAO localDAO = new LocalDAO();
                var locais = localDAO.ListarPorProfissional(profissionalId);

                if (locais.Count == 0)
                {
                    TempData["Erro"] = "Cadastre um local antes de criar serviços presenciais.";
                    return RedirectToAction("Criar");
                }

                if (!localId.HasValue)
                {
                    TempData["Erro"] = "Selecione um local.";
                    return RedirectToAction("Criar");
                }
            }

            try
            {
                ServicoDAO dao = new ServicoDAO();

                Servico s = new Servico
                {
                    ProfissionalId = profissionalId,
                    Nome = nome,
                    SubcategoriaId = subcategoriaId,
                    Descricao = descricao,
                    Atendimento = atendimento,
                    LocalId = localId,
                    LinkOnline = linkOnline,
                    TipoPreco = tipoPreco,
                    PrecoBase = precoBase
                };

                dao.SalvarComDisponibilidade(s, diasSemana.Split(',').Select(int.Parse), horaInicio, horaFim);

                TempData["Sucesso"] = "Serviço criado com sucesso ✔";

                return RedirectToAction("MeusServicos", "Profissional");
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao criar serviço. Tente novamente.";
                return RedirectToAction("Criar");
            }
            // catch (Exception ex)  
            // {
            //     TempData["Erro"] = ex.Message;
            //     return RedirectToAction("Criar");
            // }
        }


        public IActionResult Editar(int id)
        {
            var acesso = ProtegerServico(id); if (acesso != null) return acesso;
            ServicoDAO dao = new ServicoDAO();
            var servico = dao.BuscarPorId(id);

            // disponibilidade
            DisponibilidadeDAO dispDAO = new DisponibilidadeDAO();

            var disponibilidade = dispDAO.BuscarPorServico(id);

            ViewBag.Disponibilidade = disponibilidade;

            // horários
            if (disponibilidade.Any())
            {
                servico.HoraInicio = disponibilidade.Min(x => x.HoraInicio);

                servico.HoraFim = disponibilidade.Max(x => x.HoraFim);

                servico.DiasTexto = string.Join(",",
                    disponibilidade
                        .OrderBy(x => x.DiaSemana)
                        .Select(x => x.DiaSemana)
                );
            }

            SubcategoriaDAO subDAO = new SubcategoriaDAO();

            ViewBag.Subcategorias = subDAO.ListarTodas();

            return RedirectToAction("MeusServicos", "Profissional");
        }
        [HttpPost]
        public IActionResult Editar(Servico s, string diasSemana, TimeSpan horaInicio, TimeSpan horaFim)
        {
            var acesso = ProtegerServico(s.Id); if (acesso != null) return acesso;
            var erro = ValidarServico(s.Nome, s.Atendimento, s.LocalId, s.LinkOnline, s.TipoPreco, s.PrecoBase, diasSemana, horaInicio, horaFim);
            if (erro != null) return BadRequest(erro);
            try
            {
                // VALIDAÇÕES
                if (string.IsNullOrWhiteSpace(s.Nome))
                {
                    TempData["Erro"] = "Informe o nome do serviço.";
                    return RedirectToAction("MeusServicos", "Profissional");
                }

                if (s.Atendimento == "Online" && string.IsNullOrWhiteSpace(s.LinkOnline))
                {
                    TempData["Erro"] = "Informe o link do atendimento online.";
                    return RedirectToAction("MeusServicos", "Profissional");
                }

                if (s.Atendimento != "Online")
                {
                    s.LinkOnline = null;
                }

                if (s.Atendimento != "Local")
                {
                    s.LocalId = null;
                }

                if (s.TipoPreco == "Fixo" && (!s.PrecoBase.HasValue || s.PrecoBase <= 0))
                {
                    TempData["Erro"] = "Informe um preço válido.";
                    return RedirectToAction("MeusServicos", "Profissional");
                }

                if (s.TipoPreco == "Combinar")
                {
                    s.PrecoBase = null;
                }

                //  mantém subcategoria (segurança extra)
                ServicoDAO dao = new ServicoDAO();
                var original = dao.BuscarPorId(s.Id);
                s.SubcategoriaId = original.SubcategoriaId;

                s.ProfissionalId = original.ProfissionalId;
                dao.SalvarComDisponibilidade(s, diasSemana.Split(',').Select(int.Parse), horaInicio, horaFim);

                TempData["Sucesso"] = "Serviço atualizado com sucesso ✏️";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao atualizar serviço. Tente novamente.";
            }

            // TRATAMENTO DE ERROS
            // catch (Exception ex)  
            // {
            //     TempData["Erro"] = ex.Message;
            // }


            return RedirectToAction("MeusServicos", "Profissional");
        }

        [HttpPost]
        public IActionResult Excluir(int id)
        {
            var acesso = ProtegerServico(id); if (acesso != null) return acesso;
            try
            {
                ServicoDAO dao = new ServicoDAO();
                dao.Excluir(id);

                TempData["Sucesso"] = "Serviço removido da oferta. O histórico existente foi preservado.";
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("REFERENCE"))
                {
                    TempData["Erro"] = "Este serviço não pode ser excluído pois já possui agendamentos.";
                }
                else
                {
                    TempData["Erro"] = "Erro ao excluir serviço.";
                }
            }

            return RedirectToAction("MeusServicos", "Profissional");
        }

        [HttpPost]
        public IActionResult Desativar(int id)
        {
            var acesso = ProtegerServico(id); if (acesso != null) return acesso;
            try
            {
                ServicoDAO dao = new ServicoDAO();

                dao.Excluir(id);
                TempData["Sucesso"] = "Serviço removido da oferta. O histórico existente foi preservado.";
            }
            catch
            {
                TempData["Erro"] = "Erro ao processar ação.";
            }

            return RedirectToAction("MeusServicos", "Profissional");
        }


        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public JsonResult Subcategorias(int categoriaId)
        {
            SubcategoriaDAO dao = new SubcategoriaDAO();
            var lista = dao.ListarPorCategoria(categoriaId);

            return Json(lista);
        }

        public IActionResult Novo()
        {
            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            CategoriaDAO catDAO = new CategoriaDAO();
            ViewBag.Categorias = catDAO.Listar();

            return View();
        }


    }

}