using Microsoft.AspNetCore.Mvc;
using BD_TRAMPO.DAO;
using Microsoft.AspNetCore.Authorization;


namespace BD_TRAMPO.Controllers
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class PerfilAttribute(params string[] perfis) : Attribute
    {
        public string[] Perfis { get; } = perfis;
    }

    public class BaseController : Controller
    {
        protected int UsuarioAtualId => int.Parse(HttpContext.Session.GetString("UsuarioId")!);
        protected int ProfissionalAtualId => new ProfissionalDAO().BuscarPorUsuario(UsuarioAtualId);

        protected IActionResult? ProtegerServico(int id)
        {
            var s = new ServicoDAO().BuscarPorId(id);
            if (s == null) return NotFound();
            return s.ProfissionalId == ProfissionalAtualId ? null : StatusCode(403);
        }

        protected IActionResult? ProtegerLocal(int id)
        {
            var l = new LocalDAO().BuscarPorId(id);
            if (l == null) return NotFound();
            return l.ProfissionalId == ProfissionalAtualId ? null : StatusCode(403);
        }

        protected IActionResult? ProtegerAgendamento(int id, bool profissional)
        {
            var ag = new AgendamentoDAO().BuscarPorId(id);
            if (ag == null) return NotFound();
            bool permitido = profissional
                ? HttpContext.Session.GetString("UsuarioTipo") == "profissional" && ag.ProfissionalId == ProfissionalAtualId
                : ag.UsuarioId == UsuarioAtualId;
            return permitido ? null : StatusCode(403);
        }

        protected bool UsuarioLogado()
        {
            return Sessao.EstaLogado(HttpContext);
        }

        protected IActionResult Proteger()
        {
            if (HttpContext.Session.GetString("UsuarioId") == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            return null;
        }

        public override void OnActionExecuting(
                   Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            bool publico = context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();
            if (!int.TryParse(HttpContext.Session.GetString("UsuarioId"), out int id))
            {
                if (!publico) context.Result = RedirectToAction("Login", "Usuario");
                return;
            }
            var usuario = new UsuarioDAO().BuscarPorId(id);
            if (usuario == null)
            {
                HttpContext.Session.Clear();
                if (!publico) context.Result = RedirectToAction("Login", "Usuario");
                return;
            }
            // O banco é a fonte do perfil, inclusive após mudanças administrativas.
            HttpContext.Session.SetString("UsuarioTipo", usuario.Tipo.ToLowerInvariant());
            var perfil = context.ActionDescriptor.EndpointMetadata.OfType<PerfilAttribute>().LastOrDefault();
            if (!publico && perfil != null && !perfil.Perfis.Contains(usuario.Tipo, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = StatusCode(403);
                return;
            }
            if (!publico && perfil?.Perfis.SequenceEqual(new[] { "profissional" }) == true && ProfissionalAtualId == 0)
            {
                context.Result = StatusCode(403);
                return;
            }

            var usuarioIdStr = HttpContext.Session.GetString("UsuarioId");

            if (!string.IsNullOrEmpty(usuarioIdStr))
            {
                int usuarioId = int.Parse(usuarioIdStr);

                NotificacaoDAO notifDAO = new NotificacaoDAO();

                ViewBag.NotifCount = notifDAO.ContarNaoLidas(usuarioId);
            }
        }



    }
}