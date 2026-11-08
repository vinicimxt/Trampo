using BD_TRAMPO.Contracts;
using BD_TRAMPO.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddLogging();
services.AddControllersWithViews();
var http = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
var controller = new FeedbackProbe {
    ControllerContext = new ControllerContext { HttpContext = http, RouteData = new RouteData() },
    TempData = new TempDataDictionary(http, new TempDataProvider())
};
controller.RouteData.Values["controller"] = "Agendamento";
controller.RouteData.Values["action"] = "Finalizar";

var regra = controller.Executar(() => throw new FalhaOperacao(TipoFalha.Validacao,
    "O atendimento ainda não começou."));
if (regra is not RedirectToActionResult { ActionName: "Recebidos" } ||
    controller.TempData["Erro"]?.ToString() != "O atendimento ainda não começou.")
    throw new Exception("A falha de negócio deveria voltar à tela de recebidos com feedback.");

var erro = controller.Executar(() => throw new Exception("detalhe interno sensível"));
if (erro is not RedirectToActionResult { ActionName: "Recebidos" } ||
    controller.TempData["Erro"]?.ToString()?.Contains("detalhe interno") == true)
    throw new Exception("A falha técnica não pode ser exibida ao usuário.");

Console.WriteLine("PASSOU: falha de regra retorna ao contexto; erro técnico não expõe detalhes.");

sealed class FeedbackProbe : BaseController
{
    public IActionResult Executar(Func<IActionResult> acao) => ExecutarPost(acao, "Recebidos");
}

sealed class TempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
