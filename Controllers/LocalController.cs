using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Mvc;
namespace BD_TRAMPO.Controllers;

[Perfil("profissional")]
public class LocalController(LocalService service) : BaseController
{
    private UsuarioContexto Usuario => new(UsuarioAtualId,HttpContext.Session.GetString("UsuarioTipo")??"");
    public IActionResult Lista(bool abrir=false) => ExecutarAplicacao(()=>{
        ViewBag.AbrirDrawer=abrir;return View(service.Listar(Usuario));
    });
    public IActionResult Index()=>Lista();
    [HttpPost]
    public async Task<IActionResult> Salvar(SalvarLocalRequest dados,CancellationToken ct)
    {
        if(!ModelState.IsValid)return BadRequest("Local inválido.");
        try {await service.Salvar(Usuario,dados,ct);TempData["Sucesso"]="Local salvo com sucesso.";}
        catch(FalhaOperacao ex) when(ex.Tipo==TipoFalha.Conflito){TempData["Erro"]=ex.Message;}
        catch(FalhaOperacao ex){return ExecutarAplicacao(()=>throw ex);}
        return RedirectToAction("Lista");
    }
    [HttpPost]
    public IActionResult Excluir(int id)=>ExecutarAplicacao(()=>{
        try{service.Excluir(Usuario,id);TempData["Sucesso"]="Local removido.";}
        catch(FalhaOperacao ex) when(ex.Tipo==TipoFalha.Conflito){TempData["Erro"]=ex.Message;}
        return RedirectToAction("Lista");
    });
}
