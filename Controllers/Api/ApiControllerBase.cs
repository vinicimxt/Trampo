using BD_TRAMPO.Api;
using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers.Api;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[IgnoreAntiforgeryToken] // Somente Bearer, nunca autenticação por cookie/sessão.
[Produces("application/json")]
[ProducesResponseType(typeof(ErroResponse), 400)]
[ProducesResponseType(typeof(ErroResponse), 401)]
[ProducesResponseType(typeof(ErroResponse), 403)]
[ProducesResponseType(typeof(ErroResponse), 404)]
[ProducesResponseType(typeof(ErroResponse), 409)]
[ProducesResponseType(typeof(ErroResponse), 415)]
[ProducesResponseType(typeof(ErroResponse), 500)]
public abstract class ApiControllerBase : ControllerBase
{
    protected UsuarioContexto Usuario => User.Contexto();
}
