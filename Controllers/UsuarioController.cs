namespace BD_TRAMPO.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BD_TRAMPO;
    using Microsoft.Data.SqlClient;
    using BD_TRAMPO.Models.ViewModels;


    public class UsuarioController(BD_TRAMPO.Services.ContaService contas) : BaseController
    {
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Cadastro()
        {
            return View("~/Views/Usuario/Cadastro.cshtml");
        }
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Login()
        {
            return View("~/Views/Usuario/Login.cshtml");
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Cadastrar(string nome, string email, string senha, string tipo, string tipoDocumento, string documento, string telefone, string contato)
        {
            if (tipo != "profissional")
            {
                try {
                    var cliente = contas.CadastrarCliente(nome, email, senha, telefone);
                    HttpContext.Session.Clear();
                    HttpContext.Session.SetString("UsuarioId", cliente.Id.ToString());
                    HttpContext.Session.SetString("UsuarioNome", cliente.Nome);
                    HttpContext.Session.SetString("UsuarioEmail", cliente.Email);
                    HttpContext.Session.SetString("UsuarioTipo", cliente.Tipo);
                    TempData["Sucesso"] = "Conta criada com sucesso!";
                    return RedirectToAction("Lista", "Profissional");
                } catch (BD_TRAMPO.Contracts.FalhaOperacao ex) {
                    TempData["Erro"] = ex.Message;
                    return RedirectToAction("Cadastro");
                }
            }
            if (string.IsNullOrWhiteSpace(nome) || nome.Length > 100 ||
                string.IsNullOrWhiteSpace(email) || email.Length > 100 || senha?.Length > 1024 ||
                string.IsNullOrWhiteSpace(senha) || senha.Length < 8 ||
                (telefone?.Length ?? 0) > 20 || (documento?.Length ?? 0) > 20 ||
                (contato?.Length ?? 0) > 255)
                return BadRequest("Confira os dados. A senha deve ter ao menos 8 caracteres.");
            UsuarioDAO usuarioDAO = new UsuarioDAO();

            if (usuarioDAO.EmailExiste(email))
            {
                TempData["Erro"] = "Já existe uma conta com esse email";
                return RedirectToAction("Cadastro");
            }


            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) ||
                email.Length > 100 || senha.Length > 1024)
                return BadRequest("Email ou senha inválidos.");
            string senhaHash = Seguranca.GerarHash(senha);

            // segurança
            string tipoSeguro = tipo == "profissional"
                ? "profissional"
                : "cliente";

            int usuarioId;

            try
            {
                usuarioId = usuarioDAO.Inserir(
                    nome,
                    email,
                    senhaHash,
                    tipoSeguro,
                    telefone
                );
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

            // profissional
            if (tipoSeguro == "profissional")
            {
                ProfissionalDAO profDAO = new ProfissionalDAO();

                try
                {
                    profDAO.Inserir(
                        usuarioId,
                        tipoDocumento,
                        documento,
                        contato
                    );
                }
                catch (SqlException ex)
                {
                    usuarioDAO.Remover(usuarioId);

                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        TempData["Erro"] = "Já existe um cadastro com esse documento.";
                        return RedirectToAction("Cadastro");
                    }

                    TempData["Erro"] = "Erro ao cadastrar profissional.";
                    return RedirectToAction("Cadastro");
                }
            }

            // cliente
            ClienteDAO clienteDAO = new ClienteDAO();
            clienteDAO.Inserir(usuarioId);

            // sessão
            HttpContext.Session.SetString("UsuarioId", usuarioId.ToString());
            HttpContext.Session.SetString("UsuarioNome", nome);
            HttpContext.Session.SetString("UsuarioEmail", email);
            HttpContext.Session.SetString("UsuarioTipo", tipoSeguro);

            TempData["Sucesso"] = "Conta criada com sucesso!";

            if (tipoSeguro == "profissional")
                return RedirectToAction("Criar", "Servico");

            return RedirectToAction("Lista", "Profissional");
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Logar(string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha) ||
                email.Length > 100 || senha.Length > 1024)
                return BadRequest("Email ou senha inválidos.");


            UsuarioDAO dao = new UsuarioDAO();

            var usuario = dao.BuscarLogin(email, senha);

            if (usuario != null)
            {
                HttpContext.Session.Clear();
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

        [HttpPost]
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

        public IActionResult PainelCliente()
        {
            var proteger = Proteger();

            if (proteger != null)
                return proteger;

            int usuarioId =
                int.Parse(HttpContext.Session.GetString("UsuarioId"));

            UsuarioDAO usuarioDAO = new UsuarioDAO();

            var usuario = usuarioDAO.BuscarPorId(usuarioId);

            if (usuario == null)
                return RedirectToAction("Login");

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

            try {
                BD_TRAMPO.Services.ContaService.ValidarPerfil(nome, telefone);
                if ((contatoPublico?.Length ?? 0) > 255) return BadRequest("Contato muito longo.");
                var contexto = new BD_TRAMPO.Contracts.UsuarioContexto(usuarioId, tipo);
                if (!string.IsNullOrWhiteSpace(novaSenha))
                    contas.ValidarSenha(usuarioId, senhaAtual, novaSenha, confirmarSenha);
                if (tipo == "profissional") {
                    var profDAO = new ProfissionalDAO();
                    profDAO.AtualizarContato(profDAO.BuscarPorUsuario(usuarioId), contatoPublico);
                }
                contas.Atualizar(contexto, nome, telefone);
                if (!string.IsNullOrWhiteSpace(novaSenha))
                    contas.AlterarSenha(contexto, senhaAtual, novaSenha, confirmarSenha);
            } catch (BD_TRAMPO.Contracts.FalhaOperacao ex) {
                TempData["Erro"] = ex.Message;
                return RedirectToAction("Perfil");
            }

            HttpContext.Session.SetString("UsuarioNome", nome);

            TempData["Sucesso"] =
                "Conta atualizada com sucesso.";

            return RedirectToAction("Perfil");
        }



    }
}
