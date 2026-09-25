using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using BD_TRAMPO;
using BD_TRAMPO.Api;
using BD_TRAMPO.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var root = Directory.GetCurrentDirectory();
var tag = "sprint9-" + Guid.NewGuid().ToString("N");
var senha = Convert.ToBase64String(RandomNumberGenerator.GetBytes(20)) + "!9";
var novaSenha = senha + "novo";
var ids = new List<int>();
int professional = 0, service = 0, booking = 0, secondBooking = 0;
int checks = 0;
Process? process = null;
var config = new ConfigurationBuilder().AddJsonFile(Path.Combine(root,"appsettings.Development.json"))
    .AddEnvironmentVariables().Build();
Conexao.Configurar(config);
object? Sql(string query, params (string,object?)[] args) {
    using var conn = new Conexao().Conectar();
    using var cmd = new SqlCommand(query,conn);
    foreach(var (key,value) in args) cmd.Parameters.AddWithValue(key,value ?? DBNull.Value);
    return cmd.ExecuteScalar();
}
int Id(string query, params (string,object?)[] args) => Convert.ToInt32(Sql(query,args));
void Check(bool ok, string label) {
    if(!ok) throw new Exception("FALHOU: " + label);
    checks++; Console.WriteLine("PASSOU: " + label);
}
async Task<HttpResponseMessage> Send(HttpClient http, HttpMethod method, string route, object? body = null, string? token = null) {
    using var req = new HttpRequestMessage(method,route);
    if (body != null) req.Content = JsonContent.Create(body);
    if (token != null) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer",token);
    return await http.SendAsync(req);
}
async Task<JsonElement> Json(HttpResponseMessage response) => JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
void Status(HttpResponseMessage r, HttpStatusCode expected, string label) => Check(r.StatusCode == expected,label + " (HTTP " + (int)r.StatusCode + ")");
try {
    var outsider = new UsuarioDAO().Inserir(tag+"outro",tag+"outro@example.invalid",Seguranca.GerarHash(senha),"cliente","");
    ids.Add(outsider);
    Sql("INSERT INTO Clientes(UsuarioId) VALUES(@U)",("@U",outsider));
    var professionalUser = new UsuarioDAO().Inserir(tag+"profissional",tag+"profissional@example.invalid",Seguranca.GerarHash(senha),"profissional","");
    ids.Add(professionalUser);
    Sql("INSERT INTO Clientes(UsuarioId) VALUES(@U)",("@U",professionalUser));
    professional = Id("INSERT INTO Profissionais(UsuarioId,Plano) OUTPUT INSERTED.Id VALUES(@U,'Premium')",("@U",professionalUser));
    service = Id(@"INSERT INTO Servicos(ProfissionalId,SubcategoriaId,Nome,Descricao,Atendimento,TipoPreco,PrecoBase,LinkOnline,Ativo)
        OUTPUT INSERTED.Id VALUES(@P,(SELECT TOP 1 Id FROM Subcategorias ORDER BY Id),@N,'Teste Sprint 9','Online','Fixo',75,'https://example.invalid/reuniao',1)",
        ("@P",professional),("@N",tag+" serviço"));
    var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    var jwt = new JwtConfiguracao { SigningKey=key, Issuer="Trampo", Audience="Trampo.Api", ExpirationMinutes=15 };
    var tokens = new TokenService(jwt);
    var proToken = tokens.Emitir(new Usuario { Id=professionalUser,Tipo="profissional" }).AccessToken;
    var outsiderToken = tokens.Emitir(new Usuario { Id=outsider,Tipo="cliente" }).AccessToken;
    var start = new ProcessStartInfo("dotnet") { WorkingDirectory=root, UseShellExecute=false,CreateNoWindow=true,
        RedirectStandardOutput=true,RedirectStandardError=true };
    start.ArgumentList.Add(Path.Combine(root,"Tests","Sprint1","app","BD-TRAMPO.dll"));
    start.ArgumentList.Add("--urls"); start.ArgumentList.Add("http://127.0.0.1:5181");
    start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";
    start.Environment["ConnectionStrings__Xamou"] = config.GetConnectionString("Xamou")!;
    start.Environment["Jwt__SigningKey"] = key;
    start.Environment["Logging__LogLevel__Default"] = "None";
    process = Process.Start(start) ?? throw new Exception("API não iniciou");
    process.BeginOutputReadLine(); process.BeginErrorReadLine();
    using var http = new HttpClient { BaseAddress=new Uri("http://127.0.0.1:5181"),Timeout=TimeSpan.FromSeconds(20) };
    bool ready=false;
    for(int i=0;i<40 && !process.HasExited;i++) {
        try { ready=(await http.GetAsync("/health")).IsSuccessStatusCode; } catch(HttpRequestException) { }
        if(ready) break; await Task.Delay(250);
    }
    Check(ready,"API local iniciou");
    var email=tag+"cliente@example.invalid";
    Status(await Send(http,HttpMethod.Post,"/api/v1/auth/register",new { nome="",email,senha }),HttpStatusCode.BadRequest,"cadastro rejeita nome vazio");
    Status(await Send(http,HttpMethod.Post,"/api/v1/auth/register",new { nome="Cliente",email="invalido",senha }),HttpStatusCode.BadRequest,"cadastro rejeita email inválido");
    Status(await Send(http,HttpMethod.Post,"/api/v1/auth/register",new { nome="Cliente",email,senha="curta" }),HttpStatusCode.BadRequest,"cadastro rejeita senha curta");
    Status(await Send(http,HttpMethod.Post,"/api/v1/auth/register",new { nome="Cliente",email,senha,tipo="admin" }),HttpStatusCode.BadRequest,"cadastro não aceita papel informado");
    var reg=await Send(http,HttpMethod.Post,"/api/v1/auth/register",new { nome="Cliente Sprint 9",email,senha,telefone="11999999999" });
    Status(reg,HttpStatusCode.Created,"cadastro público de cliente");
    var clientToken=(await Json(reg)).GetProperty("accessToken").GetString()!;
    var client=Id("SELECT Id FROM Usuarios WHERE Email=@E",("@E",email)); ids.Add(client);
    Check(client>0 && Id("SELECT COUNT(*) FROM Usuarios WHERE Id=@U AND Tipo='cliente'",("@U",client))==1 &&
        Id("SELECT COUNT(*) FROM Clientes WHERE UsuarioId=@U",("@U",client))==1,"cliente e perfil criados juntos");
    Status(await Send(http,HttpMethod.Post,"/api/v1/auth/register",new { nome="Outro",email,senha }),HttpStatusCode.Conflict,"email duplicado rejeitado");
    Status(await Send(http,HttpMethod.Get,"/api/v1/perfil"),HttpStatusCode.Unauthorized,"perfil exige JWT");
    Status(await Send(http,HttpMethod.Get,"/api/v1/perfil",token:proToken),HttpStatusCode.Forbidden,"perfil mobile rejeita profissional");
    var profile=await Send(http,HttpMethod.Get,"/api/v1/perfil",token:clientToken);
    Status(profile,HttpStatusCode.OK,"perfil do próprio cliente");
    var profileJson=await Json(profile);
    Check(profileJson.GetProperty("email").GetString()==email && profileJson.GetProperty("telefone").GetString()=="11999999999","perfil retorna dados reais sem hash");
    Status(await Send(http,HttpMethod.Put,"/api/v1/perfil",new { nome="Hack",telefone="",usuarioId=outsider },clientToken),HttpStatusCode.BadRequest,"perfil rejeita id fornecido no corpo");
    Status(await Send(http,HttpMethod.Put,"/api/v1/perfil",new { nome="",telefone="" },clientToken),HttpStatusCode.BadRequest,"perfil valida nome");
    Status(await Send(http,HttpMethod.Put,"/api/v1/perfil",new { nome="Cliente Editado",telefone="1133334444" },clientToken),HttpStatusCode.NoContent,"edita nome e telefone");
    Check(Id("SELECT COUNT(*) FROM Usuarios WHERE Id=@U AND Nome='Cliente Editado' AND Telefone='1133334444'",("@U",client))==1 &&
        Id("SELECT COUNT(*) FROM Usuarios WHERE Id=@U AND Nome=@N",("@U",outsider),("@N",tag+"outro"))==1,"edição limitada à identidade JWT");
    Status(await Send(http,HttpMethod.Put,"/api/v1/perfil/senha",new { senhaAtual="errada",novaSenha,confirmarSenha=novaSenha },clientToken),HttpStatusCode.BadRequest,"senha atual obrigatória");
    Status(await Send(http,HttpMethod.Put,"/api/v1/perfil/senha",new { senhaAtual=senha,novaSenha,confirmarSenha="diferente" },clientToken),HttpStatusCode.BadRequest,"confirmação de senha obrigatória");
    Status(await Send(http,HttpMethod.Put,"/api/v1/perfil/senha",new { senhaAtual=senha,novaSenha,confirmarSenha=novaSenha },clientToken),HttpStatusCode.NoContent,"altera senha do próprio cliente");
    Check(new UsuarioDAO().BuscarLogin(email,senha)==null && new UsuarioDAO().BuscarLogin(email,novaSenha)!=null,"senha nova utiliza hasher real");
    Status(await Send(http,HttpMethod.Get,"/api/v1/notificacoes",token:clientToken),HttpStatusCode.OK,"notificações exigem cliente");
    Status(await Send(http,HttpMethod.Get,"/api/v1/notificacoes",token:proToken),HttpStatusCode.Forbidden,"profissional não acessa central cliente");
    var ownNotification=Id("INSERT INTO Notificacoes(UsuarioId,Titulo,Mensagem,Lida,DataCriacao) OUTPUT INSERTED.Id VALUES(@U,'Aviso de teste','Mensagem de teste',0,GETDATE())",("@U",client));
    var foreignNotification=Id("INSERT INTO Notificacoes(UsuarioId,Titulo,Mensagem,Lida,DataCriacao) OUTPUT INSERTED.Id VALUES(@U,'Aviso de teste','Outra mensagem',0,GETDATE())",("@U",outsider));
    var notices=await Send(http,HttpMethod.Get,"/api/v1/notificacoes?pagina=1&tamanho=1",token:clientToken);
    Status(notices,HttpStatusCode.OK,"lista paginada de notificações");
    var noticeJson=await Json(notices);
    Check(noticeJson.GetArrayLength()==1 && noticeJson[0].GetProperty("id").GetInt32()==ownNotification,"lista separa usuários");
    Status(await Send(http,HttpMethod.Post,$"/api/v1/notificacoes/{foreignNotification}/ler",token:clientToken),HttpStatusCode.NotFound,"leitura não altera notificação alheia");
    Status(await Send(http,HttpMethod.Post,$"/api/v1/notificacoes/{ownNotification}/ler",token:clientToken),HttpStatusCode.NoContent,"marca notificação própria");
    Check(Id("SELECT COUNT(*) FROM Notificacoes WHERE Id=@N AND Lida=1",("@N",ownNotification))==1 &&
        Id("SELECT COUNT(*) FROM Notificacoes WHERE Id=@N AND Lida=0",("@N",foreignNotification))==1,"ownership de leitura persistido");
    // O limite de 10 chamadas/minuto ? global por IP. Novo processo mant?m a regra intacta.
    if(process is { HasExited:false }) { process.Kill(true); await process.WaitForExitAsync(); }
    process?.Dispose();
    process = Process.Start(start) ?? throw new Exception("API n?o reiniciou");
    process.BeginOutputReadLine(); process.BeginErrorReadLine();
    bool restarted=false;
    for(int i=0;i<40 && !process.HasExited;i++) {
        try { restarted=(await http.GetAsync("/health")).IsSuccessStatusCode; } catch(HttpRequestException) { }
        if(restarted) break; await Task.Delay(250);
    }
    Check(restarted,"API reiniciada para respeitar limite por IP");
    Status(await Send(http,HttpMethod.Post,"/api/v1/suporte",new { tipo="Duvida",assunto="Ajuda",mensagem="Teste" }),HttpStatusCode.Unauthorized,"suporte exige JWT");
    Status(await Send(http,HttpMethod.Get,"/api/v1/notificacoes?pagina=0",token:clientToken),HttpStatusCode.BadRequest,"paginacao de notificacoes validada");
    Status(await Send(http,HttpMethod.Post,"/api/v1/suporte",new { tipo="Dúvida",assunto="",mensagem="Teste" },clientToken),HttpStatusCode.BadRequest,"suporte valida assunto");
    Status(await Send(http,HttpMethod.Post,"/api/v1/suporte",new { tipo="Dúvida",assunto="Ajuda",mensagem="Mensagem temporária" },clientToken),HttpStatusCode.NoContent,"suporte envia chamado");
    Check(Id("SELECT COUNT(*) FROM ContatoSuporte WHERE UsuarioId=@U AND Assunto='Ajuda'",("@U",client))==1,"suporte atribuído ao JWT");
    Status(await Send(http,HttpMethod.Post,"/api/v1/suporte",new { tipo="Dúvida",assunto="Ajuda",mensagem="x",usuarioId=outsider },clientToken),HttpStatusCode.BadRequest,"suporte rejeita identidade no corpo");
    var catalog=await Send(http,HttpMethod.Get,"/api/v1/servicos?pagina=1&tamanho=1&busca="+Uri.EscapeDataString(tag)+"&atendimento=Online");
    Status(catalog,HttpStatusCode.OK,"busca pública paginada");
    var catalogJson=await Json(catalog);
    Check(catalogJson.GetArrayLength()==1 && catalogJson[0].GetProperty("id").GetInt32()==service &&
        catalogJson[0].GetProperty("nomeProfissional").GetString()==tag+"profissional","busca e nome público reais");
    var detail=await Send(http,HttpMethod.Get,$"/api/v1/servicos/{service}");
    Status(detail,HttpStatusCode.OK,"detalhe inclui dados públicos");
    var detailJson=await Json(detail);
    Check(detailJson.GetProperty("categoria").GetString()?.Length>0 && detailJson.GetProperty("subcategoria").GetString()?.Length>0,"categoria e subcategoria reais");
    Status(await Send(http,HttpMethod.Get,"/api/v1/servicos?atendimento=Invalido"),HttpStatusCode.BadRequest,"modalidade inválida rejeitada");
    var clientId=Id("SELECT Id FROM Clientes WHERE UsuarioId=@U",("@U",client));
    booking=Id("INSERT INTO Agendamentos(ClienteId,ProfissionalId,ServicoId,Data,Hora,Status,ConfirmadoProfissional,FinalizadoProfissional,ConfirmadoCliente) OUTPUT INSERTED.Id VALUES(@C,@P,@S,CAST(GETDATE() AS date),'09:00','Finalizado',1,1,1)",("@C",clientId),("@P",professional),("@S",service));
    var outsiderClientId=Id("SELECT Id FROM Clientes WHERE UsuarioId=@U",("@U",outsider));
    secondBooking=Id("INSERT INTO Agendamentos(ClienteId,ProfissionalId,ServicoId,Data,Hora,Status,ConfirmadoProfissional,FinalizadoProfissional,ConfirmadoCliente) OUTPUT INSERTED.Id VALUES(@C,@P,@S,CAST(GETDATE() AS date),'10:00','Finalizado',1,1,1)",("@C",outsiderClientId),("@P",professional),("@S",service));
    Status(await Send(http,HttpMethod.Get,$"/api/v1/agendamentos/{booking}/avaliacao"),HttpStatusCode.Unauthorized,"estado da avaliação exige JWT");
    Status(await Send(http,HttpMethod.Get,$"/api/v1/agendamentos/{booking}/avaliacao",token:outsiderToken),HttpStatusCode.Forbidden,"estado da avaliação exige ownership");
    var state=await Send(http,HttpMethod.Get,$"/api/v1/agendamentos/{booking}/avaliacao",token:clientToken);
    Status(state,HttpStatusCode.OK,"estado da avaliação elegível");
    Check((await Json(state)).GetProperty("podeAvaliar").GetBoolean(),"avaliação disponível após conclusão");
    Status(await Send(http,HttpMethod.Post,$"/api/v1/agendamentos/{booking}/avaliacao",new { nota=0,comentario="Invalida" },clientToken),HttpStatusCode.BadRequest,"nota invalida rejeitada");
    Status(await Send(http,HttpMethod.Post,$"/api/v1/agendamentos/{booking}/avaliacao",new { nota=5,comentario="Ótimo" },clientToken),HttpStatusCode.NoContent,"cliente avalia reserva concluída");
    Status(await Send(http,HttpMethod.Post,$"/api/v1/agendamentos/{booking}/avaliacao",new { nota=5,comentario="Repetida" },clientToken),HttpStatusCode.Conflict,"avaliação repetida rejeitada");
    var after=await Send(http,HttpMethod.Get,$"/api/v1/agendamentos/{booking}/avaliacao",token:clientToken);
    Check((await Json(after)).GetProperty("jaAvaliado").GetBoolean(),"estado informa avaliação existente");
    Status(await Send(http,HttpMethod.Post,$"/api/v1/agendamentos/{secondBooking}/avaliacao",new { nota=5,comentario="Alheia" },clientToken),HttpStatusCode.Forbidden,"POST avaliação respeita ownership");
    Console.WriteLine("TOTAL: " + checks + " verificações Sprint 9 passaram.");
} finally {
    if(process is { HasExited:false }) { process.Kill(true); await process.WaitForExitAsync(); }
    process?.Dispose();
    if(booking>0) Sql("DELETE FROM Avaliacoes WHERE AgendamentoId=@A",("@A",booking));
    if(secondBooking>0) Sql("DELETE FROM Avaliacoes WHERE AgendamentoId=@A",("@A",secondBooking));
    if(booking>0) Sql("DELETE FROM Agendamentos WHERE Id=@A",("@A",booking));
    if(secondBooking>0) Sql("DELETE FROM Agendamentos WHERE Id=@A",("@A",secondBooking));
    if(service>0) { Sql("DELETE FROM Disponibilidade WHERE ServicoId=@S",("@S",service)); Sql("DELETE FROM Servicos WHERE Id=@S",("@S",service)); }
    if(professional>0) Sql("DELETE FROM Profissionais WHERE Id=@P",("@P",professional));
    foreach(var id in ids) {
        Sql("DELETE FROM Notificacoes WHERE UsuarioId=@U",("@U",id));
        Sql("DELETE FROM ContatoSuporte WHERE UsuarioId=@U",("@U",id));
        Sql("DELETE FROM Clientes WHERE UsuarioId=@U",("@U",id));
        Sql("DELETE FROM Usuarios WHERE Id=@U",("@U",id));
    }
    Console.WriteLine("Fixtures Sprint 9 removidas por IDs registrados.");
}
