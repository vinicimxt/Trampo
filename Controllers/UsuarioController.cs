namespace BD_TRAMPO.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BD_TRAMPO;
    using Microsoft.Data.SqlClient;
    using BD_TRAMPO.Models.ViewModels;


    public class UsuarioController : BaseController
    {
        public IActionResult Cadastro()
        {
            return View("~/Views/Usuario/Cadastro.cshtml");
        }
        public IActionResult Login()
        {
            return View("~/Views/Usuario/Login.cshtml");
        }

        [HttpPost]
        public IActionResult Cadastrar(string nome, string email, string senha, string tipo, string tipoDocumento, string documento, string telefone, string contato)
        {
            UsuarioDAO usuarioDAO = new UsuarioDAO();

            if (usuarioDAO.EmailExiste(email))
            {
                TempData["Erro"] = "Já existe uma conta com esse email";
                return RedirectToAction("Cadastro");
            }

            string senhaHash = Seguranca.GerarHash(senha);
            int usuarioId;

            try
            {
                usuarioId = usuarioDAO.Inserir(nome, email, senhaHash, tipo, telefone);
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    TempData["Erro"] = "Esse email já está cadastrado";
                    return RedirectToAction("Cadastro");
                }

                TempData["Erro"] = "Erro ao cadastrar.";
                return RedirectToAction("Cadastro");
            }

            if (tipo == "profissional")
            {
                ProfissionalDAO profDAO = new ProfissionalDAO();

                try
                {
                    profDAO.Inserir(usuarioId, tipoDocumento, documento, contato);
                }
                catch (SqlException ex)
                {
                    usuarioDAO.Remover(usuarioId); //rollback manual

                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        TempData["Erro"] = "Já existe um cadastro com esse CPF.";
                        return RedirectToAction("Cadastro");
                    }

                    TempData["Erro"] = "Erro ao cadastrar.";
                    return RedirectToAction("Cadastro");
                }
            }

            ClienteDAO clienteDAO = new ClienteDAO();
            clienteDAO.Inserir(usuarioId);

            HttpContext.Session.SetString("UsuarioId", usuarioId.ToString());
            HttpContext.Session.SetString("UsuarioNome", nome);
            HttpContext.Session.SetString("UsuarioEmail", email);
            HttpContext.Session.SetString("UsuarioTipo", tipo);

            TempData["Sucesso"] = "Conta criada com sucesso!";

            if (tipo == "profissional")
                return RedirectToAction("Criar", "Servico");

            return RedirectToAction("Lista", "Profissional");
        }

        public IActionResult RedirecionarPorTipo()
        {
            var tipo = Sessao.Tipo(HttpContext);

            if (tipo == "profissional")
                return RedirectToAction("Dashboard", "Profissional");

            return RedirectToAction("Lista", "Profissional");
        }



        [HttpPost]
        public IActionResult Logar(string email, string senha)
        {
            string senhaHash = Seguranca.GerarHash(senha);

            UsuarioDAO dao = new UsuarioDAO();

            var usuario = dao.BuscarLogin(email, senhaHash);

            if (usuario != null)
            {
                HttpContext.Session.SetString("UsuarioId", usuario.Id.ToString());
                HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
                HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                HttpContext.Session.SetString("UsuarioTipo", usuario.Tipo);

                // profissional
                if (usuario.Tipo == "profissional")
                {
                    return RedirectToAction("Dashboard", "Profissional");
                }

                // cliente
                return RedirectToAction("Lista", "Profissional");
            }

            TempData["Erro"] = "Email ou senha inválidos.";

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AreaRestrita()
        {
            var usuario = HttpContext.Session.GetString("UsuarioEmail");

            if (usuario == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            return View();
        }

        public IActionResult Perfil()
        {
            var proteger = Proteger();

            if (proteger != null)
                return proteger;

            int usuarioId =
                int.Parse(HttpContext.Session.GetString("UsuarioId"));

            UsuarioDAO usuarioDAO = new UsuarioDAO();
            ProfissionalDAO profDAO = new ProfissionalDAO();

            var usuario = usuarioDAO.BuscarPorId(usuarioId);

            if (usuario == null)
                return RedirectToAction("Login");

            var vm = new PerfilViewModel
            {
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Telefone = usuario.Telefone,
                Tipo = usuario.Tipo
            };

            // se for profissional
            if (usuario.Tipo == "profissional")
            {
                int profissionalId =
                    profDAO.BuscarPorUsuario(usuarioId);

                var profissional =
                    profDAO.BuscarPorId(profissionalId);

                if (profissional != null)
                {
                    vm.ContatoPublico =
                        profissional.Contato;
                }
            }

            return View(vm);
        }

        public IActionResult MinhaConta()
        {
            var proteger = Proteger();

            if (proteger != null)
                return proteger;

            int usuarioId =
                int.Parse(HttpContext.Session.GetString("UsuarioId"));

            UsuarioDAO dao = new UsuarioDAO();

            var usuario = dao.BuscarPorId(usuarioId);

            return View(usuario);
        }

        [HttpPost]
        public IActionResult SalvarConta(string nome, string telefone, string contatoPublico, string senhaAtual, string novaSenha, string confirmarSenha)
        {
            var proteger = Proteger();

            if (proteger != null)
                return proteger;

            int usuarioId =
                int.Parse(HttpContext.Session.GetString("UsuarioId"));

            UsuarioDAO dao = new UsuarioDAO();

            string tipo =
                HttpContext.Session.GetString("UsuarioTipo");

            // PROFISSIONAL
            if (!string.IsNullOrWhiteSpace(tipo) &&
                tipo.ToLower() == "profissional")
            {
                ProfissionalDAO profDAO = new ProfissionalDAO();

                int profissionalId =
                    profDAO.BuscarPorUsuario(usuarioId);

                profDAO.AtualizarContato(
                    profissionalId,
                    contatoPublico
                );
            }

            // CONTA DO USUÁRIO
            dao.AtualizarConta(usuarioId, nome, telefone);

            // ALTERAÇÃO DE SENHA
            if (!string.IsNullOrWhiteSpace(novaSenha))
            {
                if (novaSenha != confirmarSenha)
                {
                    TempData["Erro"] =
                        "A confirmação da senha não confere.";

                    return RedirectToAction("Perfil");
                }

                string senhaAtualHash =
                    Seguranca.GerarHash(senhaAtual);

                bool senhaCorreta =
                    dao.VerificarSenha(
                        usuarioId,
                        senhaAtualHash
                    );

                if (!senhaCorreta)
                {
                    TempData["Erro"] =
                        "Senha atual incorreta.";

                    return RedirectToAction("Perfil");
                }

                string novaSenhaHash =
                    Seguranca.GerarHash(novaSenha);

                dao.AtualizarSenha(
                    usuarioId,
                    novaSenhaHash
                );
            }

            HttpContext.Session.SetString("UsuarioNome", nome);

            TempData["Sucesso"] =
                "Conta atualizada com sucesso.";

            return RedirectToAction("Perfil");
        }



    }
}
