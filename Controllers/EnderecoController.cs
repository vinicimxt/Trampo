using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;
using BD_TRAMPO.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace BD_TRAMPO.Controllers;

[Perfil("cliente","profissional","admin")]
[EnableRateLimiting("enderecos")]
public class EnderecoController(EnderecoService service):BaseController
{
    private async Task<IActionResult> Consultar(Func<Task<object>> executar)
    {
        try{return Json(await executar());}
        catch(FalhaOperacao ex){return BadRequest(ErrosApiMiddleware.Resposta(400,ex.Message));}
        catch(FalhaEndereco ex){var (status,resposta)=FalhasEnderecoHttp.Resposta(ex);return StatusCode(status,resposta);}
    }
    [HttpGet]
    public Task<IActionResult> Cep(string cep,CancellationToken ct)=>Consultar(async()=>await service.ConsultarCEP(cep,ct));
    [HttpGet]
    public Task<IActionResult> Pesquisar(string uf,string cidade,string logradouro,CancellationToken ct)=>
        Consultar(async()=>await service.Pesquisar(uf,cidade,logradouro,ct));
}
