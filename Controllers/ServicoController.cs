using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;
namespace BD_TRAMPO.Controllers
{

    public class ServicoController : BaseController
    {
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
            ViewBag.TotalServicos = servicoDAO.ContarPorProfissional(usuarioId);

            AgendamentoDAO agendamentoDAO = new AgendamentoDAO();
            ViewBag.PedidosPendentes = agendamentoDAO.ContarPendentes(usuarioId);

            return View();
        }

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

                int servicoIdCriado = dao.Inserir(s);

                // converte "1,2,5" → List<int>
                var dias = diasSemana
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                DisponibilidadeDAO dispDAO = new DisponibilidadeDAO();

                foreach (var dia in dias)
                {
                    dispDAO.Inserir(new Disponibilidade
                    {
                        ProfissionalId = profissionalId,
                        ServicoId = servicoIdCriado,
                        DiaSemana = dia,
                        HoraInicio = horaInicio,
                        HoraFim = horaFim,
                        Ativo = true
                    });
                }


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

            return View(servico);
        }
        [HttpPost]
        public IActionResult Editar(Servico s, string diasSemana, TimeSpan horaInicio, TimeSpan horaFim)
        {
            try
            {
                //  VALIDAÇÕES
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

                dao.Atualizar(s);

                DisponibilidadeDAO dispDAO = new DisponibilidadeDAO();

                // remove regras antigas
                dispDAO.RemoverPorServico(s.Id);

                var dias = (diasSemana ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();


                foreach (var dia in dias)
                {
                    dispDAO.Inserir(new Disponibilidade
                    {
                        ProfissionalId = original.ProfissionalId,
                        ServicoId = s.Id,
                        DiaSemana = dia,
                        HoraInicio = horaInicio,
                        HoraFim = horaFim,
                        Ativo = true
                    });
                }

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

        public IActionResult Excluir(int id)
        {
            try
            {
                ServicoDAO dao = new ServicoDAO();
                dao.Excluir(id);

                TempData["Sucesso"] = "Serviço excluído com sucesso 🗑️";
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

        public IActionResult Desativar(int id)
        {
            try
            {
                ServicoDAO dao = new ServicoDAO();

                if (dao.TemAgendamentos(id))
                {
                    dao.Desativar(id);
                    TempData["Sucesso"] = "Serviço desativado. Ele não aparecerá mais para novos clientes.";
                }
                else
                {
                    DisponibilidadeDAO dispDAO = new DisponibilidadeDAO();

                    dispDAO.RemoverPorServico(id);

                    dao.Excluir(id);

                    TempData["Sucesso"] =
                        "Serviço excluído com sucesso 🗑️";
                }
            }
            catch
            {
                TempData["Erro"] = "Erro ao processar ação.";
            }

            return RedirectToAction("MeusServicos", "Profissional");
        }


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