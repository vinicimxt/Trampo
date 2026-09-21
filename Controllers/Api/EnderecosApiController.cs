using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace BD_TRAMPO.Controllers.Api;

[Route("api/v1/enderecos")]
[EnableRateLimiting("enderecos")]
[ProducesResponseType(typeof(BD_TRAMPO.Api.Contracts.ErroResponse),429)]
[ProducesResponseType(typeof(BD_TRAMPO.Api.Contracts.ErroResponse),502)]
[ProducesResponseType(typeof(BD_TRAMPO.Api.Contracts.ErroResponse),503)]
[ProducesResponseType(typeof(BD_TRAMPO.Api.Contracts.ErroResponse),504)]
public sealed class EnderecosApiController(EnderecoService service):ApiControllerBase
{
    [HttpGet("cep/{cep}")]
    [ProducesResponseType(typeof(EnderecoConsulta),200)]
    public async Task<ActionResult<EnderecoConsulta>> Cep(string cep,CancellationToken ct)=>Ok(await service.ConsultarCEP(cep,ct));
    [HttpGet("pesquisar")]
    [ProducesResponseType(typeof(IReadOnlyList<EnderecoConsulta>),200)]
    public async Task<ActionResult<IReadOnlyList<EnderecoConsulta>>> Pesquisar([FromQuery]string uf,[FromQuery]string cidade,[FromQuery]string logradouro,CancellationToken ct)=>
        Ok(await service.Pesquisar(uf,cidade,logradouro,ct));
}
