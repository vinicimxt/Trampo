namespace BD_TRAMPO.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using BD_TRAMPO;
    using Microsoft.Data.SqlClient;

    public class UsuarioController : Controller
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

            // ✅ 1. VALIDAR ANTES
            if (usuarioDAO.EmailExiste(email))
            {
                TempData["Erro"] = "Já existe uma conta com esse email 😬";
                return RedirectToAction("Cadastro");
            }

            string senhaHash = Seguranca.GerarHash(senha);

            int usuarioId;

            try
            {
                // ✅ 2. TENTAR INSERIR
                usuarioId = usuarioDAO.Inserir(nome, email, senhaHash, tipo, telefone);
            }
            catch (SqlException ex)
            {
                //  proteção extra (concorrência)
                if (ex.Number == 2627) // UNIQUE constraint
                {
                    TempData["Erro"] = "Esse email já está cadastrado ";
                    return RedirectToAction("Cadastro");
                }

                throw; // outros erros
            }

            // ✅ 3. AUTENTICAR
            HttpContext.Session.SetString("UsuarioId", usuarioId.ToString());
            HttpContext.Session.SetString("UsuarioNome", nome);
            HttpContext.Session.SetString("UsuarioEmail", email);
            HttpContext.Session.SetString("UsuarioTipo", tipo);

            // ✅ 4. FLUXO NORMAL
            if (tipo == "profissional")
            {
                ProfissionalDAO profDAO = new ProfissionalDAO();
                profDAO.Inserir(usuarioId, tipoDocumento, documento);

                return RedirectToAction("Criar", "Servico");
            }
            else
            {
                ClienteDAO clienteDAO = new ClienteDAO();
                clienteDAO.Inserir(usuarioId);

                return RedirectToAction("Lista", "Profissional");
            }
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
