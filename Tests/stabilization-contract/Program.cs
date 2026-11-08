using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

var baseUrl = Environment.GetEnvironmentVariable("TRAMPO_TEST_API_URL") ?? "http://127.0.0.1:5165/";
if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
    !uri.IsLoopback || uri.Scheme != Uri.UriSchemeHttp)
    throw new InvalidOperationException("TRAMPO_TEST_API_URL deve apontar para uma API HTTP local.");

string Required(string name) => Environment.GetEnvironmentVariable(name)
    ?? throw new InvalidOperationException($"Variável {name} não configurada.");

var serviceId = int.Parse(Required("TRAMPO_TEST_PUBLIC_SERVICE_ID"));
if (serviceId <= 0) throw new InvalidOperationException("Informe um serviço público ativo.");

using var http = new HttpClient { BaseAddress = uri, Timeout = TimeSpan.FromSeconds(15) };
var passed = 0;
var skipped = 0;

void Check(bool condition, string description)
{
    if (!condition) throw new Exception("FALHOU: " + description);
    Console.WriteLine("PASSOU: " + description);
    passed++;
}

async Task<(HttpStatusCode Status, JsonDocument? Json)> Send(
    HttpMethod method, string path, string? token = null, object? body = null)
{
    using var request = new HttpRequestMessage(method, path);
    if (token != null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    if (body != null) request.Content = JsonContent.Create(body);
    using var response = await http.SendAsync(request);
    if (response.Content.Headers.ContentLength == 0)
        return (response.StatusCode, null);
    var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
    return (response.StatusCode, json);
}

async Task<string> Login(string email, string password, string role)
{
    var result = await Send(HttpMethod.Post, "api/v1/auth/login", body: new { email, senha = password });
    Check(result.Status == HttpStatusCode.OK && result.Json != null, "login " + role);
    using var login = result.Json!;
    var token = login.RootElement.GetProperty("accessToken").GetString();
    Check(!string.IsNullOrWhiteSpace(token), "JWT recebido para " + role);
    var me = await Send(HttpMethod.Get, "api/v1/auth/me", token);
    Check(me.Status == HttpStatusCode.OK && me.Json != null, "auth/me " + role);
    using var profile = me.Json!;
    Check(profile.RootElement.GetProperty("tipo").GetString() == role, "papel " + role);
    return token!;
}

var client = await Login(Required("TRAMPO_TEST_CLIENT_EMAIL"),
    Required("TRAMPO_TEST_CLIENT_PASSWORD"), "cliente");
var professional = await Login(Required("TRAMPO_TEST_PRO_EMAIL"),
    Required("TRAMPO_TEST_PRO_PASSWORD"), "profissional");

var publicReviews = await Send(HttpMethod.Get, $"api/v1/servicos/{serviceId}/avaliacoes");
Check(publicReviews.Status == HttpStatusCode.OK && publicReviews.Json != null,
    "avaliações públicas de serviço ativo: 200");
using (var json = publicReviews.Json!)
{
    var root = json.RootElement;
    Check(root.GetProperty("total").GetInt32() == root.GetProperty("avaliacoes").GetArrayLength(),
        "resumo público corresponde à lista");
    Check(root.TryGetProperty("media", out _), "média pública presente no contrato");
}
var missing = await Send(HttpMethod.Get, "api/v1/servicos/99999999/avaliacoes");
Check(missing.Status == HttpStatusCode.NotFound, "avaliações de serviço inexistente: 404");
missing.Json?.Dispose();

if (int.TryParse(Environment.GetEnvironmentVariable("TRAMPO_TEST_REVIEWED_BOOKING_ID"), out var reviewedBookingId)
    && reviewedBookingId > 0)
{
    var duplicate = await Send(HttpMethod.Post,
        $"api/v1/agendamentos/{reviewedBookingId}/avaliacao", client,
        new { nota = 5, comentario = "Tentativa de avaliação duplicada em teste" });
    Check(duplicate.Status == HttpStatusCode.Conflict,
        "avaliação duplicada permanece em 409");
    duplicate.Json?.Dispose();
}

var received = await Send(HttpMethod.Get, "api/v1/profissionais/me/avaliacoes", professional);
Check(received.Status == HttpStatusCode.OK && received.Json != null,
    "avaliações recebidas pelo profissional: 200");
using (var json = received.Json!)
{
    var root = json.RootElement;
    Check(root.GetProperty("total").GetInt32() == root.GetProperty("avaliacoes").GetArrayLength(),
        "resumo profissional corresponde à lista");
    foreach (var review in root.GetProperty("avaliacoes").EnumerateArray())
        Check(review.TryGetProperty("servicoNome", out _) &&
              review.TryGetProperty("nota", out _) &&
              review.TryGetProperty("data", out _), "avaliação recebida tem serviço, nota e data");
}
var clientDenied = await Send(HttpMethod.Get, "api/v1/profissionais/me/avaliacoes", client);
Check(clientDenied.Status == HttpStatusCode.Forbidden, "Cliente não acessa avaliações recebidas: 403");
clientDenied.Json?.Dispose();
var anonymousDenied = await Send(HttpMethod.Get, "api/v1/profissionais/me/avaliacoes");
Check(anonymousDenied.Status == HttpStatusCode.Unauthorized,
    "sem JWT não acessa avaliações recebidas: 401");
anonymousDenied.Json?.Dispose();

foreach (var path in new[] { "notificacoes", "suporte", "perfil" })
{
    var clientResult = await Send(HttpMethod.Get, "api/v1/" + path, client);
    var professionalResult = await Send(HttpMethod.Get, "api/v1/" + path, professional);
    var anonymousResult = await Send(HttpMethod.Get, "api/v1/" + path);
    Check(clientResult.Status == HttpStatusCode.OK, "Cliente em " + path + ": 200");
    Check(professionalResult.Status == HttpStatusCode.OK, "Profissional em " + path + ": 200");
    Check(anonymousResult.Status == HttpStatusCode.Unauthorized, "sem JWT em " + path + ": 401");
    anonymousResult.Json?.Dispose();

    using var clientJson = clientResult.Json;
    using var professionalJson = professionalResult.Json;
    if (path == "perfil")
    {
        Check(clientJson?.RootElement.TryGetProperty("nome", out _) == true &&
              professionalJson?.RootElement.TryGetProperty("nome", out _) == true,
            "perfil dos dois papéis contém nome");
        Check(clientJson?.RootElement.TryGetProperty("senha", out _) == false &&
              professionalJson?.RootElement.TryGetProperty("senha", out _) == false,
            "perfil não expõe senha");
        continue;
    }

    var clientIds = clientJson!.RootElement.EnumerateArray()
        .Select(item => item.GetProperty("id").GetInt32()).ToHashSet();
    var professionalIds = professionalJson!.RootElement.EnumerateArray()
        .Select(item => item.GetProperty("id").GetInt32()).ToHashSet();
    if (clientIds.Count > 0 && professionalIds.Count > 0)
    {
        Check(!clientIds.Overlaps(professionalIds), "listas próprias de " + path + " não se misturam");
        if (path == "notificacoes")
        {
            var crossRead = await Send(HttpMethod.Post,
                $"api/v1/notificacoes/{clientIds.First()}/ler", professional);
            Check(crossRead.Status == HttpStatusCode.NotFound,
                "Profissional não marca notificação do Cliente como lida: 404");
            crossRead.Json?.Dispose();
        }
    }
    else
    {
        Console.WriteLine("IGNORADO: isolamento de " + path + " exige dados em ambas as contas.");
        skipped++;
    }
}

Console.WriteLine($"TOTAL: {passed} passaram; {skipped} ignorados.");
