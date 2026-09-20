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
    public IActionResult Salvar(CriarAgendamentoRequest dados) => ExecutarAplicacao(() =>
    {
        if (!ModelState.IsValid) return BadRequest("Dados do agendamento inválidos.");
        int id = service.Criar(Usuario, dados);
        TempData["Sucesso"] = $"Pedido #{id} enviado para confirmação.";
        return RedirectToAction("Meus");
    });

    public IActionResult Meus(string? sucesso) => ExecutarAplicacao(() => {
        ViewBag.Sucesso = TempData["Sucesso"];
        return View(service.Meus(Usuario));
    });

    [Perfil("profissional")]
    public IActionResult Recebidos() => ExecutarAplicacao(() => View(service.Recebidos(Usuario)));

    [HttpPost]
    public IActionResult Confirmar(int id) => ExecutarAplicacao(() => {
        service.Confirmar(Usuario, id); return RedirectToAction("Recebidos");
    });

    [HttpPost]
    public IActionResult Recusar(int id) => ExecutarAplicacao(() => {
        service.Recusar(Usuario, id); return RedirectToAction("Recebidos");
    });

    [HttpPost]
    public IActionResult Finalizar(int id, decimal valorFinal) => ExecutarAplicacao(() => {
        if (!ModelState.IsValid) return BadRequest("Valor inválido.");
        service.Finalizar(Usuario, id, valorFinal); return RedirectToAction("Recebidos");
    });

    [HttpPost]
    public IActionResult FinalizarProfissional(int id) => Finalizar(id, 0);

    [HttpPost]
    public IActionResult ConfirmarCliente(int id) => ExecutarAplicacao(() => {
        service.ConfirmarConclusao(Usuario, id); return RedirectToAction("Meus");
    });

    [HttpPost]
    public IActionResult Cancelar(int id) => ExecutarAplicacao(() => {
        service.Cancelar(Usuario, id);
        return RedirectToAction(Usuario.Tipo == "profissional" ? "Recebidos" : "Meus");
    });

    public IActionResult DetalhesModal(int id) => ExecutarAplicacao(() => {
        var ag = service.BuscarParticipante(Usuario, id);
        return RedirectToAction(ag.UsuarioId == Usuario.UsuarioId ? "Meus" : "Recebidos");
    });
}