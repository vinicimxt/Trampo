using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace BD_TRAMPO.Controllers.Api;
[Route("api/v1/perfil"), Authorize(Roles="cliente")]
public sealed class PerfilApiController(ContaService service) : ApiControllerBase
{
    [HttpGet]
    public ActionResult<PerfilResponse> Buscar() {
        var u = service.Perfil(Usuario);
        return Ok(new PerfilResponse(u.Nome,u.Email,u.Telefone));
    }
    [HttpPut]
    public IActionResult Atualizar(PerfilRequest dados) {
        service.Atualizar(Usuario,dados.Nome,dados.Telefone); return NoContent();
    }
    [HttpPut("senha"), EnableRateLimiting("api-login")]
    public IActionResult Senha(SenhaRequest dados) {
        service.AlterarSenha(Usuario,dados.SenhaAtual,dados.NovaSenha,dados.ConfirmarSenha); return NoContent();
    }
}
