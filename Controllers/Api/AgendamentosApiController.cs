using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers.Api;

[Route("api/v1/agendamentos")]
[Authorize(Roles = "cliente,profissional")]
public sealed class AgendamentosApiController(AgendamentoService service) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<AgendamentoResponse>), 200)]
    public ActionResult<List<AgendamentoResponse>> Listar([FromQuery] string visao = "meus") =>
        Ok(service.Listar(Usuario, visao).Select(AgendamentoResponse.De).ToList());

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AgendamentoResponse), 200)]
    public ActionResult<AgendamentoResponse> Buscar(int id) =>
        Ok(AgendamentoResponse.De(service.BuscarParticipante(Usuario, id)));

    [HttpPost]
    [ProducesResponseType(typeof(RecursoCriadoResponse), 201)]
    public ActionResult<RecursoCriadoResponse> Criar(CriarAgendamentoRequest dados)
    {
        int id = service.Criar(Usuario, dados);
        return CreatedAtAction(nameof(Buscar), new { id }, new RecursoCriadoResponse(id));
    }

    [HttpPost("{id:int}/cancelar")]
    [ProducesResponseType(204)]
    public IActionResult Cancelar(int id) { service.Cancelar(Usuario, id); return NoContent(); }
    [HttpPost("{id:int}/confirmar"), Authorize(Roles = "profissional")]
    [ProducesResponseType(204)]
    public IActionResult Confirmar(int id) { service.Confirmar(Usuario, id); return NoContent(); }
    [HttpPost("{id:int}/recusar"), Authorize(Roles = "profissional")]
    [ProducesResponseType(204)]
    public IActionResult Recusar(int id) { service.Recusar(Usuario, id); return NoContent(); }
    [HttpPost("{id:int}/finalizar"), Authorize(Roles = "profissional")]
    [ProducesResponseType(204)]
    public IActionResult Finalizar(int id, FinalizarRequest dados) { service.Finalizar(Usuario, id, dados.ValorFinal); return NoContent(); }
    [HttpPost("{id:int}/confirmar-conclusao")]
    [ProducesResponseType(204)]
    public IActionResult Concluir(int id) { service.ConfirmarConclusao(Usuario, id); return NoContent(); }
}
