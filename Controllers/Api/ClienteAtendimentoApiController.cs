using BD_TRAMPO.Api.Contracts;
using BD_TRAMPO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace BD_TRAMPO.Controllers.Api;
[Route("api/v1/notificacoes"), Authorize(Roles="cliente")]
public sealed class NotificacoesApiController(ClienteAtendimentoService service) : ApiControllerBase
{
    [HttpGet]
    public ActionResult<List<NotificacaoResponse>> Listar(int pagina=1,int tamanho=20) =>
        Ok(service.Notificacoes(Usuario,pagina,tamanho).Select(n=>new NotificacaoResponse(n.Id,n.Titulo,n.Mensagem,n.DataCriacao,n.Lida)).ToList());
    [HttpPost("{id:int}/ler")]
    public IActionResult Ler(int id) { service.Ler(Usuario,id); return NoContent(); }
}
[Route("api/v1/suporte"), Authorize(Roles="cliente")]
public sealed class SuporteApiController(ClienteAtendimentoService service) : ApiControllerBase
{
    [HttpPost, EnableRateLimiting("api-login")]
    public IActionResult Enviar(SuporteRequest dados) {
        service.Enviar(Usuario,dados.Tipo,dados.Assunto,dados.Mensagem); return NoContent();
    }
}
