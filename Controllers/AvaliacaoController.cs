using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers;

[Perfil("cliente", "profissional")]
public class AvaliacaoController(AgendamentoService service) : BaseController
{
    private UsuarioContexto Usuario => new(UsuarioAtualId, HttpContext.Session.GetString("UsuarioTipo") ?? "");

    public IActionResult Avaliar(int agendamentoId) => ExecutarAplicacao(() => {
        service.PermitirAvaliacao(Usuario, agendamentoId);
        return RedirectToAction("Meus", "Agendamento");
    });

    [HttpPost]
    public IActionResult Salvar(AvaliarAgendamentoRequest dados) => ExecutarAplicacao(() => {
        if (!ModelState.IsValid) return BadRequest("Avaliação inválida.");
        service.Avaliar(Usuario, dados);
        TempData["Sucesso"] = "Obrigado pela avaliação.⭐";
        return RedirectToAction("Meus", "Agendamento");
    });
}