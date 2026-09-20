using BD_TRAMPO.Api;
using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BD_TRAMPO.Controllers.Api;

[Route("api/v1/auth")]
public sealed class AuthController(AutenticacaoService service, TokenService tokens,
    JwtConfiguracao configuracao) : ApiControllerBase
{
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("api-login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(ErroResponse), 429)]
    [ProducesResponseType(typeof(ErroResponse), 503)]
    public ActionResult<LoginResponse> Login(LoginRequest dados)
    {
        if (configuracao.Chave() == null) throw new ApiIndisponivelException();
        return Ok(tokens.Emitir(service.Entrar(dados.Email, dados.Senha)));
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UsuarioResponse), 200)]
    public ActionResult<UsuarioResponse> Me()
    {
        var usuario = service.Atual(Usuario);
        return Ok(new UsuarioResponse(usuario.Id, usuario.Nome, usuario.Email, usuario.Tipo));
    }
}
