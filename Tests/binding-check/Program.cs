using System.Globalization;
using System.Net.Http.Json;
using BD_TRAMPO.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
var app = builder.Build();
app.Use(async (context, next) =>
{
    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
    await next();
});
app.MapControllers();
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();

var endereco = app.Services.GetRequiredService<IServer>()
    .Features.Get<IServerAddressesFeature>()!.Addresses.Single();
using var cliente = new HttpClient { BaseAddress = new Uri(endereco) };
var campos = new Dictionary<string, string>
{
    ["Id"] = "5", ["Nome"] = "Serviço editado", ["Descricao"] = "Nova descrição",
    ["Atendimento"] = "Domicilio", ["TipoPreco"] = "Fixo", ["PrecoBase"] = "80.00",
    ["DiasSemana"] = "1,2", ["HoraInicio"] = "08:00", ["HoraFim"] = "17:00"
};

var corrigido = await Enviar(campos);
if (!corrigido.Valid || corrigido.Preco != 80m || corrigido.SubcategoriaId != 0)
    throw new Exception($"Formulário corrigido inválido: {System.Text.Json.JsonSerializer.Serialize(corrigido)}");

campos["SubcategoriaId"] = "";
var anterior = await Enviar(campos);
if (anterior.Valid || !anterior.Errors.Contains("SubcategoriaId"))
    throw new Exception("O formulário anterior deveria falhar em SubcategoriaId.");

Console.WriteLine("PASSOU: edição sem SubcategoriaId; falha reproduzida com SubcategoriaId vazio.");
await app.StopAsync();

async Task<Resultado> Enviar(Dictionary<string, string> valores)
{
    using var resposta = await cliente.PostAsync("/probe", new FormUrlEncodedContent(valores));
    resposta.EnsureSuccessStatusCode();
    return (await resposta.Content.ReadFromJsonAsync<Resultado>())!;
}

record Resultado(bool Valid, int SubcategoriaId, decimal? Preco, string[] Errors);

[Route("probe")]
public sealed class ProbeController : Controller
{
    [HttpPost]
    public IActionResult Post([FromForm] SalvarServicoRequest dados) => Ok(new Resultado(
        ModelState.IsValid, dados.SubcategoriaId, dados.PrecoBase,
        ModelState.Where(x => x.Value?.Errors.Count > 0).Select(x => x.Key).ToArray()));
}
