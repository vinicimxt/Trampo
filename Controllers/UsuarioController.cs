namespace BD_TRAMPO.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BD_TRAMPO;
    using Microsoft.Data.SqlClient;

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
        public IActionResult Cadastrar(
    string nome, string email, string senha, string tipo,
    string tipoDocumento, string documento, string telefone)
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
                    profDAO.Inserir(usuarioId, tipoDocumento, documento);
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

                //  Redirecionamento de page para pro 
                if (usuario.Tipo == "profissional")
                {
                    return RedirectToAction("Dashboard", "Profissional");
                }
                else
                {
                    return RedirectToAction("Lista", "Profissional");
                }
            }
            ViewBag.Mensagem = "Email ou senha inválidos";
            return View("Login");
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


    }
}
