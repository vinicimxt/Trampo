using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
namespace BD_TRAMPO.Controllers.Api;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record LocalRequest(string? Nome,EnderecoRequest DadosEndereco);
public sealed record LocalResponse(int Id,string Nome,string Endereco,EnderecoDados? DadosEndereco)
{public static LocalResponse De(Local l)=>new(l.Id,l.Nome,l.Endereco,l.DadosEndereco);}

[Route("api/v1/locais")]
[Authorize(Roles="profissional")]
public sealed class LocaisApiController(LocalService service):ApiControllerBase
{
    [HttpGet,ProducesResponseType(typeof(List<LocalResponse>),200)]
    public ActionResult<List<LocalResponse>> Listar()=>Ok(service.Listar(Usuario).Select(LocalResponse.De).ToList());
    [HttpGet("{id:int}"),ProducesResponseType(typeof(LocalResponse),200)]
    public ActionResult<LocalResponse> Buscar(int id)=>Ok(LocalResponse.De(service.Buscar(Usuario,id)));
    [HttpPost,ProducesResponseType(typeof(BD_TRAMPO.Api.Contracts.RecursoCriadoResponse),201)]
    public async Task<IActionResult> Criar(LocalRequest dados,CancellationToken ct)
    {
        if(dados.DadosEndereco==null)throw new FalhaOperacao(TipoFalha.Validacao,"Informe endereço estruturado.");
        int id=await service.Salvar(Usuario,new(){Nome=dados.Nome,DadosEndereco=dados.DadosEndereco},ct);
        return CreatedAtAction(nameof(Buscar),new{id},new BD_TRAMPO.Api.Contracts.RecursoCriadoResponse(id));
    }
    [HttpPut("{id:int}"),ProducesResponseType(204)]
    public async Task<IActionResult> Editar(int id,LocalRequest dados,CancellationToken ct)
    {
        if(dados.DadosEndereco==null)throw new FalhaOperacao(TipoFalha.Validacao,"Informe endereço estruturado.");
        await service.Salvar(Usuario,new(){Id=id,Nome=dados.Nome,DadosEndereco=dados.DadosEndereco},ct);return NoContent();
    }
    [HttpDelete("{id:int}"),ProducesResponseType(204)]
    public IActionResult Excluir(int id){service.Excluir(Usuario,id);return NoContent();}
}
