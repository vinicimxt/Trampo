using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers.Api;

[Route("api/v1/agendamentos/{id:int}/avaliacao")]
[Authorize(Roles = "cliente,profissional")]
public sealed class AvaliacoesApiController(AgendamentoService service) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(204)]
    public IActionResult Avaliar(int id, AvaliacaoRequest dados)
    {
        service.Avaliar(Usuario, id, dados.Nota, dados.Comentario);
        return NoContent();
    }
}
