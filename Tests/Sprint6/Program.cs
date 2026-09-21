using BD_TRAMPO;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;

var root = Directory.GetCurrentDirectory();
var android = args.Length > 0 ? Path.GetFullPath(args[0]) :
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AndroidStudioProjects", "TRAMPO");
if (!File.Exists(Path.Combine(android, "gradlew.bat")))
    throw new InvalidOperationException("Projeto Android existente não encontrado.");
var tag = "sprint6-" + Guid.NewGuid().ToString("N");
var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)) + "!a1";
var users = new List<int>();
int professional = 0;
Process? backend = null;
int checks = 0;
object? Sql(string query, params (string, object)[] values) {
    using var connection = new Conexao().Conectar();
    using var command = new SqlCommand(query, connection);
    foreach (var (name, value) in values) command.Parameters.AddWithValue(name, value);
    return command.ExecuteScalar();
}
int Id(string query, params (string, object)[] values) => Convert.ToInt32(Sql(query, values));
void Check(bool ok, string name) {
    if (!ok) throw new Exception("FALHOU: " + name);
    Console.WriteLine("PASSOU: " + name);
    checks++;
}
try {
    foreach (var role in new[] { "cliente", "profissional" }) {
        var user = new UsuarioDAO().Inserir(tag + role, tag + role + "@example.invalid", Seguranca.GerarHash(password), role, "");
        users.Add(user);
        Sql("INSERT INTO Clientes(UsuarioId) VALUES(@U)", ("@U", user));
    }
    professional = Id("INSERT INTO Profissionais(UsuarioId,Plano) OUTPUT INSERTED.Id VALUES(@U,'Premium')", ("@U", users[1]));
    int local = Id("INSERT INTO Locais(ProfissionalId,Nome,Endereco) OUTPUT INSERTED.Id VALUES(@P,@N,'Local temporário Sprint 6')",
        ("@P", professional), ("@N", tag));
    var services = new List<int>();
    foreach (var modality in new[] { "Online", "Local", "Domicilio" }) {
        int service = Id("""
            INSERT INTO Servicos(ProfissionalId,SubcategoriaId,Nome,Atendimento,TipoPreco,PrecoBase,LinkOnline,LocalId,Ativo)
            OUTPUT INSERTED.Id
            VALUES(@P,(SELECT TOP 1 Id FROM Subcategorias ORDER BY Id),@N,@M,'Fixo',80.50,@Link,@Local,1)
            """, ("@P", professional), ("@N", tag + modality), ("@M", modality),
            ("@Link", modality == "Online" ? "https://example.invalid/reuniao" : DBNull.Value),
            ("@Local", modality == "Local" ? local : DBNull.Value));
        services.Add(service);
        for (int day = 0; day < 7; day++)
            Sql("INSERT INTO Disponibilidade(ProfissionalId,ServicoId,DiaSemana,HoraInicio,HoraFim) VALUES(@P,@S,@D,'08:00','18:00')",
                ("@P", professional), ("@S", service), ("@D", day));
    }
    var start = new ProcessStartInfo("dotnet") {
        WorkingDirectory = root, UseShellExecute = false, CreateNoWindow = true,
        RedirectStandardOutput = true, RedirectStandardError = true
    };
    start.ArgumentList.Add(Path.Combine(root, "Tests", "Sprint1", "app", "BD-TRAMPO.dll"));
    start.ArgumentList.Add("--urls");
    start.ArgumentList.Add("http://127.0.0.1:5179");
    start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
    start.Environment["Jwt__SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    // Não registrar corpos, credenciais nem dados privados da execução.
    start.Environment["Logging__LogLevel__Default"] = "None";
    backend = Process.Start(start) ?? throw new Exception("API não iniciou.");
    backend.BeginOutputReadLine();
    backend.BeginErrorReadLine();
    using var http = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5179"), Timeout = TimeSpan.FromSeconds(2) };
    bool ready = false;
    for (int attempt = 0; attempt < 40 && !backend.HasExited; attempt++) {
        try { ready = (await http.GetAsync("/api/v1/servicos")).IsSuccessStatusCode; } catch (HttpRequestException) { }
        if (ready) break;
        await Task.Delay(250);
    }
    Check(ready, "API real disponível para o cliente Kotlin");
    var gradle = new ProcessStartInfo("cmd.exe") {
        WorkingDirectory = android, UseShellExecute = false, CreateNoWindow = true,
        RedirectStandardOutput = true, RedirectStandardError = true
    };
    gradle.Arguments = "/d /c gradlew.bat :app:testDebugUnitTest --tests com.example.trampo.LiveApiTest --rerun-tasks --no-configuration-cache --console=plain";
    gradle.Environment["JAVA_HOME"] = @"C:\Program Files\Android\Android Studio\jbr";
    gradle.Environment["TRAMPO_LIVE_URL"] = "http://127.0.0.1:5179/";
    gradle.Environment["TRAMPO_LIVE_EMAIL"] = tag + "cliente@example.invalid";
    gradle.Environment["TRAMPO_LIVE_PASSWORD"] = password;
    gradle.Environment["TRAMPO_LIVE_SERVICES"] = string.Join(",", services);
    gradle.Environment["TRAMPO_LIVE_DATE"] = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");
    using var client = Process.Start(gradle) ?? throw new Exception("Gradle não iniciou.");
    // Saída do Gradle não contém variáveis de ambiente ou corpos HTTP.
    var output = client.StandardOutput.ReadToEndAsync();
    var errors = client.StandardError.ReadToEndAsync();
    await client.WaitForExitAsync();
    await File.WriteAllTextAsync(Path.Combine(root, "docs", "Sprint6", "integracao-kotlin.log"), await output + await errors);
    Check(client.ExitCode == 0, "Login, agenda, criação, consulta e cancelamento no Kotlin contra API real");
    Check(Id("SELECT COUNT(*) FROM Agendamentos WHERE ProfissionalId=@P", ("@P", professional)) == 3,
        "Três reservas do cliente Kotlin persistidas no mesmo SQL Server do Web");
    Check(Id("SELECT COUNT(*) FROM Agendamentos WHERE ProfissionalId=@P AND Status='Pendente'", ("@P", professional)) == 2,
        "Reservas Local e Domicilio começam pendentes");
    Check(Id("SELECT COUNT(*) FROM Agendamentos WHERE ProfissionalId=@P AND Status='CanceladoCliente'", ("@P", professional)) == 1,
        "Cancelamento solicitado pelo Kotlin persistido no SQL");
    Check(Id("SELECT COUNT(*) FROM AgendamentoEnderecos e JOIN Agendamentos a ON a.Id=e.AgendamentoId WHERE a.ProfissionalId=@P", ("@P", professional)) == 2,
        "Snapshots físicos persistidos; Online sem endereço");
    Console.WriteLine("TOTAL: " + checks + " verificações de integração passaram.");
}
finally {
    if (backend is { HasExited: false }) { backend.Kill(true); await backend.WaitForExitAsync(); }
    backend?.Dispose();
    if (professional > 0) {
        Sql("DELETE FROM Agendamentos WHERE ProfissionalId=@P", ("@P", professional));
        Sql("DELETE FROM Disponibilidade WHERE ProfissionalId=@P", ("@P", professional));
        Sql("DELETE FROM Servicos WHERE ProfissionalId=@P", ("@P", professional));
        Sql("DELETE FROM Locais WHERE ProfissionalId=@P", ("@P", professional));
    }
    foreach (int user in users) {
        Sql("DELETE FROM Notificacoes WHERE UsuarioId=@U", ("@U", user));
        Sql("DELETE FROM Usuarios WHERE Id=@U", ("@U", user));
    }
    Console.WriteLine("Fixtures desta execução removidas; nenhum registro preexistente alterado.");
}