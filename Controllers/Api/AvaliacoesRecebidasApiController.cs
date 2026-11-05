using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers.Api;

[Route("api/v1/profissionais/me/avaliacoes")]
[Authorize(Roles = "profissional")]
public sealed class AvaliacoesRecebidasApiController(AvaliacaoService service) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AvaliacoesPublicasResponse), 200)]
    public ActionResult<AvaliacoesPublicasResponse> Listar()
    {
        var itens = service.Recebidas(Usuario)
            .Select(a => new AvaliacaoPublicaResponse(a.Nota, a.Comentario, a.NomeUsuario, a.Data, a.ServicoNome))
            .ToList();
        return Ok(new AvaliacoesPublicasResponse(
            itens.Count == 0 ? null : itens.Average(a => (double)a.Nota), itens.Count, itens));
    }
}
