using BD_TRAMPO;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

int checks = 0;
var ports = new Dictionary<int, int>();
void Check(bool ok, string name)
{
    if (!ok) throw new Exception("FALHOU: " + name);
    Console.WriteLine("PASSOU: " + name);
    checks++;
}
IConfiguration Config(string? value) => new ConfigurationBuilder().AddInMemoryCollection(
    new Dictionary<string, string?> { ["ConnectionStrings:Xamou"] = value }).Build();
void Reject(string? value, string name)
{
    try { _ = new Conexao(Config(value)); }
    catch (InvalidOperationException ex)
    {
        Check(!ex.ToString().Contains("CANARY_SECRET"), name);
        return;
    }
    throw new Exception("FALHOU: " + name);
}
Reject(null, "configuracao ausente recusada");
Reject(" ", "configuracao vazia recusada");
Reject("Password=CANARY_SECRET;InvalidOption=yes", "configuracao invalida nao vaza segredo");
Reject("Server=localhost;Password=CANARY_SECRET", "banco obrigatorio");
Reject("Database=Xamou;Password=CANARY_SECRET", "servidor obrigatorio");
_ = new Conexao(Config("Server=127.0.0.1,1;Database=Teste;Integrated Security=true"));
Check(true, "configuracao valida nao abre conexao durante construcao");

var root = Directory.GetCurrentDirectory();
var publish = Path.Combine(root, "artifacts", "backend");
var dll = Path.Combine(publish, "BD-TRAMPO.dll");
Check(File.Exists(dll) && !File.Exists(Path.Combine(publish, "appsettings.Development.json")),
    "publish existe sem configuracao de maquina Development");
Check(!Directory.Exists(Path.Combine(publish, "Tests")) &&
      !Directory.Exists(Path.Combine(publish, "artifacts")), "publish sem testes ou artefatos recursivos");

Process Start(string? connection)
{
    var socket = new TcpListener(IPAddress.Loopback, 0);
    socket.Start();
    int port = ((IPEndPoint)socket.LocalEndpoint).Port;
    socket.Stop();
    var info = new ProcessStartInfo("dotnet")
    {
        WorkingDirectory = publish, UseShellExecute = false, CreateNoWindow = true,
        RedirectStandardOutput = true, RedirectStandardError = true
    };
    info.ArgumentList.Add(dll);
    info.ArgumentList.Add("--urls");
    info.ArgumentList.Add($"http://127.0.0.1:{port}");
    info.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
    info.Environment["DOTNET_ENVIRONMENT"] = "Production";
    foreach (var key in new[] { "ConnectionStrings__Xamou", "SQLCONNSTR_Xamou",
        "SQLAZURECONNSTR_Xamou", "CUSTOMCONNSTR_Xamou", "Jwt__SigningKey",
        "ASPNETCORE_HTTPS_PORT", "HTTPS_PORT", "ASPNETCORE_HTTPS_PORTS" })
        info.Environment.Remove(key);
    if (connection != null) info.Environment["ConnectionStrings__Xamou"] = connection;
    var process = Process.Start(info) ?? throw new Exception("Nao iniciou processo de teste.");
    ports[process.Id] = port;
    return process;
}
async Task Stop(Process process)
{
    if (!process.HasExited) process.Kill(entireProcessTree: true);
    await process.WaitForExitAsync();
}
using (var missing = Start(null))
{
    var stdout = missing.StandardOutput.ReadToEndAsync();
    var stderr = missing.StandardError.ReadToEndAsync();
    try
    {
        await missing.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));
        var logs = await stdout + await stderr;
        Check(missing.ExitCode != 0 && logs.Contains("Configure ConnectionStrings:Xamou"),
            "Production falha sem banco externo, sem fallback SQL Express");
    }
    finally { await Stop(missing); }
}
// Banco deliberadamente inacessivel: health nao deve depender de SQL.
// CANARY_SECRET e somente marcador de teste, nunca credencial de producao.
using (var host = Start("Server=127.0.0.1,1;Database=Teste;User ID=fixture;Password=CANARY_SECRET;Connect Timeout=1;Encrypt=True;TrustServerCertificate=False"))
{
    var stdout = host.StandardOutput.ReadToEndAsync();
    var stderr = host.StandardError.ReadToEndAsync();
    try
    {
        using var http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{ports[host.Id]}"),
            Timeout = TimeSpan.FromSeconds(10) };
        bool ready = false;
        for (int i = 0; i < 60; i++)
        {
            if (host.HasExited) break;
            try { using var response = await http.GetAsync("/health"); ready = response.IsSuccessStatusCode; }
            catch (HttpRequestException) { }
            if (ready) break;
            await Task.Delay(250);
        }
        Check(ready, "Production inicia com connection string externa e banco indisponivel");
        using var health = await http.GetAsync("/health");
        Check(health.StatusCode == HttpStatusCode.OK &&
            await health.Content.ReadAsStringAsync() == "Healthy", "GET health anonimo retorna apenas Healthy");
        Check(health.Headers.CacheControl?.NoStore == true, "health nao deve ser armazenado em cache");
        Check((await http.GetAsync("/openapi/v1.json")).StatusCode == HttpStatusCode.NotFound &&
            (await http.GetAsync("/swagger/index.html")).StatusCode == HttpStatusCode.NotFound,
            "Swagger continua fechado em Production");
        using var failure = await http.GetAsync("/api/v1/servicos");
        var body = await failure.Content.ReadAsStringAsync();
        Check(failure.StatusCode == HttpStatusCode.InternalServerError && body.Contains("ERRO_INTERNO") &&
            !body.Contains("CANARY_SECRET") && !body.Contains("Exception") && !body.Contains("127.0.0.1"),
            "falha SQL retorna erro API generico sem detalhes internos");
        await Stop(host);
        var logs = await stdout + await stderr;
        Check(!logs.Contains("CANARY_SECRET"), "logs nao contem marcador de senha SQL");
    }
    finally { await Stop(host); }
}
Console.WriteLine($"TOTAL: {checks} verificacoes Sprint 8 passaram.");
