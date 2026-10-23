using BD_TRAMPO.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BD_TRAMPO.Controllers.Api;

[Route("api/v1/categorias")]
public sealed class CategoriasApiController(CategoriaDAO categorias, SubcategoriaDAO subcategorias) : ApiControllerBase
{
    [HttpGet, AllowAnonymous]
    [ProducesResponseType(typeof(List<CategoriaResponse>), 200)]
    public ActionResult<List<CategoriaResponse>> Listar() =>
        Ok(CategoriaResponse.Montar(categorias.Listar(), subcategorias.ListarTodas()));
}
