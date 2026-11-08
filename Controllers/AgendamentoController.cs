using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers;

[Perfil("cliente", "profissional")]
public class AgendamentoController(AgendamentoService service) : BaseController
{
    private UsuarioContexto Usuario => new(UsuarioAtualId, HttpContext.Session.GetString("UsuarioTipo") ?? "");

    public IActionResult Novo(int servicoId, DateTime? data) => ExecutarAplicacao(() =>
    {
        var agenda = service.ConsultarAgenda(Usuario, servicoId, data ?? DateTime.Today);
        if (agenda.Regras.Count == 0) {
            TempData["Erro"] = "Profissional ainda não definiu disponibilidade.";
            return RedirectToAction("Lista", "Profissional");
        }
        string[] dias = ["Domingo", "Segunda", "Terça", "Quarta", "Quinta", "Sexta", "Sábado"];
        ViewBag.DiasTexto = string.Join(", ", agenda.Regras.Select(r => r.DiaSemana).Distinct().Order().Select(d => dias[d]));
        ViewBag.DiaInvalido = !RegrasAgenda.Horarios(agenda.Regras, agenda.Dia).Any();
        ViewBag.HoraInicio = agenda.Regras.Min(r => r.HoraInicio).ToString(@"hh\:mm");
        ViewBag.HoraFim = agenda.Regras.Max(r => r.HoraFim).ToString(@"hh\:mm");
        ViewBag.NomeServico = agenda.Servico.Nome;
        ViewBag.NomeProfissional = agenda.Profissional;
        ViewBag.ServicoId = servicoId;
        ViewBag.Data = agenda.Dia;
        ViewBag.Horarios = agenda.Horarios.ToList();
        ViewBag.Ocupados = new List<TimeSpan>();
        ViewBag.Atendimento = agenda.Servico.Atendimento;
        return View();
    });

    [HttpPost]
    public IActionResult Salvar(CriarAgendamentoRequest dados) => ExecutarPost(() =>
    {
        if (!ModelState.IsValid) {
            ModelState.AddModelError("", "Confira os dados do agendamento.");
            return Novo(dados.ServicoId, dados.Data);
        }
        int id = service.Criar(Usuario, dados);
        TempData["Sucesso"] = $"Pedido #{id} enviado para confirmação.";
        return RedirectToAction("Meus");
    }, "Novo", valoresRota: new { servicoId = dados.ServicoId, data = dados.Data });

    public IActionResult Meus(string? sucesso) => ExecutarAplicacao(() => {
        return View(service.Meus(Usuario));
    });

    [Perfil("profissional")]
    public IActionResult Recebidos() => ExecutarAplicacao(() => View(service.Recebidos(Usuario)));

    [HttpPost]
    public IActionResult Confirmar(int id) => ExecutarPost(() => {
        service.Confirmar(Usuario, id);
        TempData["Sucesso"] = "Agendamento confirmado com sucesso.";
        return RedirectToAction("Recebidos");
    }, "Recebidos");

    [HttpPost]
    public IActionResult Recusar(int id) => ExecutarPost(() => {
        service.Recusar(Usuario, id);
        TempData["Sucesso"] = "Agendamento recusado.";
        return RedirectToAction("Recebidos");
    }, "Recebidos");

    [HttpPost]
    public IActionResult Finalizar(int id, decimal valorFinal) => ExecutarPost(() => {
        if (!ModelState.IsValid) {
            TempData["Erro"] = "Informe um valor válido para finalizar o atendimento.";
            return RedirectToAction("Recebidos");
        }
        service.Finalizar(Usuario, id, valorFinal);
        TempData["Sucesso"] = "Atendimento finalizado com sucesso.";
        return RedirectToAction("Recebidos");
    }, "Recebidos");

    [HttpPost]
    public IActionResult FinalizarProfissional(int id) => Finalizar(id, 0);

    [HttpPost]
    public IActionResult ConfirmarCliente(int id) => ExecutarPost(() => {
        service.ConfirmarConclusao(Usuario, id);
        TempData["Sucesso"] = "Conclusão confirmada com sucesso.";
        return RedirectToAction("Meus");
    }, "Meus");

    [HttpPost]
    public IActionResult Cancelar(int id) => ExecutarPost(() => {
        service.Cancelar(Usuario, id);
        TempData["Sucesso"] = "Agendamento cancelado.";
        return RedirectToAction(Usuario.Tipo == "profissional" ? "Recebidos" : "Meus");
    }, Usuario.Tipo == "profissional" ? "Recebidos" : "Meus");

    public IActionResult DetalhesModal(int id) => ExecutarAplicacao(() => {
        var ag = service.BuscarParticipante(Usuario, id);
        return RedirectToAction(ag.UsuarioId == Usuario.UsuarioId ? "Meus" : "Recebidos");
    });
}
