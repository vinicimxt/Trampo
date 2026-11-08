using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers.Api;

[Route("api/v1/profissionais")]
public sealed class ProfissionaisApiController(AvaliacaoConsultaService avaliacoes) : ApiControllerBase
{
    [HttpGet("me/avaliacoes"), Authorize(Roles = "profissional")]
    [ProducesResponseType(typeof(AvaliacoesPublicasResponse), 200)]
    public ActionResult<AvaliacoesPublicasResponse> Recebidas() =>
        Ok(AvaliacoesPublicasResponse.De(avaliacoes.Recebidas(Usuario)));
}
