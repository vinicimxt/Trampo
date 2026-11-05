using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers.Api;

public sealed record AvaliacaoPublicaResponse(int Nota, string? Comentario, string ClienteNome, DateTime Data, string ServicoNome);
public sealed record AvaliacoesPublicasResponse(double? Media, int Total, IReadOnlyList<AvaliacaoPublicaResponse> Avaliacoes);

[Route("api/v1/servicos/{servicoId:int}/avaliacoes")]
public sealed class AvaliacoesPublicasApiController(AvaliacaoService service) : ApiControllerBase
{
    [HttpGet, AllowAnonymous]
    [ProducesResponseType(typeof(AvaliacoesPublicasResponse), 200)]
    public ActionResult<AvaliacoesPublicasResponse> Listar(int servicoId)
    {
        var itens = service.PublicasDoServico(servicoId)
            .Select(a => new AvaliacaoPublicaResponse(a.Nota, a.Comentario, a.NomeUsuario, a.Data, a.ServicoNome))
            .ToList();
        return Ok(new AvaliacoesPublicasResponse(
            itens.Count == 0 ? null : itens.Average(a => (double)a.Nota), itens.Count, itens));
    }
}
