using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers.Api;

[Route("api/v1/servicos")]
public sealed class ServicosApiController(ServicoService service, AgendamentoService agenda) : ApiControllerBase
{
    [HttpGet, AllowAnonymous]
    [ProducesResponseType(typeof(List<ServicoResponse>), 200)]
    public ActionResult<List<ServicoResponse>> Listar([FromQuery] int pagina = 1, [FromQuery] int tamanho = 20, [FromQuery] string? busca = null, [FromQuery] string? atendimento = null) =>
        Ok(service.ListarPublicos(pagina, tamanho, busca, atendimento).Select(ServicoResponse.De).ToList());

    [HttpGet("{id:int}"), AllowAnonymous]
    [ProducesResponseType(typeof(ServicoResponse), 200)]
    public ActionResult<ServicoResponse> Buscar(int id) => Ok(ServicoResponse.De(service.BuscarPublico(id)));

    [HttpGet("{id:int}/agenda")]
    [Authorize(Roles = "cliente,profissional")]
    [ProducesResponseType(typeof(AgendaResponse), 200)]
    public ActionResult<AgendaResponse> Agenda(int id, [FromQuery] DateOnly data)
    {
        var resultado = agenda.ConsultarAgenda(Usuario, id, data.ToDateTime(TimeOnly.MinValue));
        return Ok(new AgendaResponse(id, data, resultado.Horarios));
    }

    [HttpPost, Authorize(Roles = "profissional")]
    [ProducesResponseType(typeof(RecursoCriadoResponse), 201)]
    public ActionResult<RecursoCriadoResponse> Criar(ServicoRequest dados)
    {
        int id = service.Criar(Usuario, dados.Aplicacao());
        return CreatedAtAction(nameof(Buscar), new { id }, new RecursoCriadoResponse(id));
    }

    [HttpPut("{id:int}"), Authorize(Roles = "profissional")]
    [ProducesResponseType(204)]
    public IActionResult Editar(int id, ServicoRequest dados)
    {
        service.Editar(Usuario, dados.Aplicacao(id));
        return NoContent();
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "profissional")]
    [ProducesResponseType(typeof(RemocaoResponse), 200)]
    public ActionResult<RemocaoResponse> Remover(int id) =>
        Ok(new RemocaoResponse(id, service.Remover(Usuario, id) ? "desativado" : "excluido"));
}
