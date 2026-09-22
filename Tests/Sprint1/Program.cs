using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;
using BD_TRAMPO.DAO;
using BD_TRAMPO;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

var root = Directory.GetCurrentDirectory();
var tag = "sprint1-" + Guid.NewGuid().ToString("N");
var usuarios = new List<int>();
var profissionais = new List<int>();
var servicos = new List<int>();
var senha = "Teste-Sprint1!2026";
int passou = 0;
Process? app = null;
var logs = new StringBuilder();

object? Sql(string texto, params (string, object?)[] parametros)
{
    using var c = new Conexao().Conectar();
    using var q = new SqlCommand(texto, c);
    foreach (var (nome, valor) in parametros) q.Parameters.AddWithValue(nome, valor ?? DBNull.Value);
    return q.ExecuteScalar();
}
int Id(string texto, params (string, object?)[] p) => Convert.ToInt32(Sql(texto, p));
void Verificar(bool condicao, string nome)
{
    if (!condicao) throw new Exception("FALHOU: " + nome);
    Console.WriteLine("PASSOU: " + nome);
    passou++;
}
int Usuario(string sufixo, string tipo, bool legado = false)
{
    var hash = legado ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(senha))).ToLowerInvariant() : Seguranca.GerarHash(senha);
    int id = new UsuarioDAO().Inserir(tag + sufixo, tag + sufixo + "@example.invalid", hash, tipo, "");
    usuarios.Add(id);
    Sql("INSERT INTO Clientes(UsuarioId) VALUES(@Id)", ("@Id", id));
    return id;
}
int Profissional(int usuario)
{
    int id = Id("INSERT INTO Profissionais(UsuarioId,Plano) OUTPUT INSERTED.Id VALUES(@Id,'Premium')", ("@Id", usuario));
    profissionais.Add(id); return id;
}
int Servico(int profissional, string sufixo, bool ativo = true)
{
    int id = Id(@"INSERT INTO Servicos(ProfissionalId,SubcategoriaId,Nome,Atendimento,TipoPreco,PrecoBase,LinkOnline,Ativo)
        OUTPUT INSERTED.Id VALUES(@P,(SELECT TOP 1 Id FROM Subcategorias),@N,'Online','Fixo',100,'https://example.invalid/reuniao',@A)",
        ("@P", profissional), ("@N", tag + sufixo), ("@A", ativo));
    servicos.Add(id);
    for (int dia = 0; dia < 7; dia++)
        Sql("INSERT INTO Disponibilidade(ProfissionalId,ServicoId,DiaSemana,HoraInicio,HoraFim) VALUES(@P,@S,@D,'08:00','18:00')",
            ("@P", profissional), ("@S", id), ("@D", dia));
    return id;
}
HttpClient Cliente() => new(new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() })
    { BaseAddress = new Uri("http://127.0.0.1:5177"), Timeout = TimeSpan.FromSeconds(30) };
async Task<string> Token(HttpClient c, string pagina = "/Usuario/Login")
{
    var r = await c.GetAsync(pagina);
    var html = await r.Content.ReadAsStringAsync();
    var m = Regex.Match(html, "name=\"csrf-token\" content=\"([^\"]+)\"");
    if (!m.Success) throw new Exception("Token ausente em " + pagina + ": " + r.StatusCode);
    return WebUtility.HtmlDecode(m.Groups[1].Value);
}
async Task<HttpResponseMessage> Post(HttpClient c, string rota, Dictionary<string,string>? campos = null, bool token = true)
{
    campos ??= new();
    if (token) campos["__RequestVerificationToken"] = await Token(c);
    return await c.PostAsync(rota, new FormUrlEncodedContent(campos));
}
async Task Login(HttpClient c, string sufixo)
{
    var r = await Post(c, "/Usuario/Logar", new() { ["email"]=tag+sufixo+"@example.invalid", ["senha"]=senha });
    Verificar(r.StatusCode == HttpStatusCode.Redirect && r.Headers.Location?.ToString() != "/Usuario/Login", "login " + sufixo);
}
Dictionary<string,string> Reserva(int s, DateTime data, string hora) => new() {
    ["servicoId"]=s.ToString(), ["data"]=data.ToString("yyyy-MM-dd"), ["hora"]=hora, ["descricao"]="Teste automatizado" };
int ReservaId(int cliente, int servico) => Id(@"SELECT ISNULL(MAX(a.Id),0) FROM Agendamentos a
    JOIN Clientes c ON c.Id=a.ClienteId WHERE c.UsuarioId=@U AND a.ServicoId=@S", ("@U",cliente),("@S",servico));
int TotalReservas() => Id("SELECT COUNT(*) FROM Agendamentos WHERE ServicoId IN ("+string.Join(",",servicos)+")");
async Task RejeitarReserva(HttpClient c, int s, DateTime data, string hora, string nome)
{
    int antes = TotalReservas();
    var r = await Post(c,"/Agendamento/Salvar",Reserva(s,data,hora));
    Verificar((int)r.StatusCode < 500 && TotalReservas()==antes, nome);
}
try
{
    string moderno = Seguranca.GerarHash(senha);
    Verificar(moderno != Seguranca.GerarHash(senha), "salt diferente para a mesma senha");
    Verificar(Seguranca.Verificar(moderno, senha, out var migrar) && !migrar, "senha moderna válida");
    Verificar(!Seguranca.Verificar(moderno, "incorreta", out _), "senha moderna incorreta rejeitada");
    var regras = new[] { new Disponibilidade { Ativo=true, DiaSemana=1, HoraInicio=TimeSpan.FromHours(22), HoraFim=TimeSpan.FromHours(2) } };
    var segunda = new DateTime(2026,9,21);
    Verificar(RegrasAgenda.Horarios(regras,segunda).Count()==2 &&
        RegrasAgenda.Horarios(regras,segunda.AddDays(1)).Select(x=>x.Hour).SequenceEqual(new[]{0,1}),
        "virada de meia-noite no dia correto");
    if (args.Contains("--unit"))
    {
        Console.WriteLine("TOTAL: " + passou + " verificações sem banco passaram. Integração SQL não executada neste modo.");
        return;
    }
    var configBancoTeste = new ConfigurationBuilder()
        .AddJsonFile(Path.Combine(root, "appsettings.Development.json"), optional: false)
        .AddEnvironmentVariables()
        .Build();
    Conexao.Configurar(configBancoTeste);
    int a=Usuario("a","cliente",true), b=Usuario("b","cliente"), p=Usuario("p","profissional"), q=Usuario("q","profissional"), admin=Usuario("admin","admin");
    int prof=Profissional(p), outroProf=Profissional(q);
    int s=Servico(prof,"servico"), s2=Servico(prof,"outro"), inativo=Servico(prof,"inativo",false), outro=Servico(outroProf,"terceiro");
    int sub=Id("SELECT TOP 1 Id FROM Subcategorias");
    var start = new ProcessStartInfo("dotnet", "\""+(Environment.GetEnvironmentVariable("TRAMPO_TEST_DLL") ?? Path.Combine(root,"bin/Debug/net10.0/BD-TRAMPO.dll"))+"\" --urls http://127.0.0.1:5177") {
        WorkingDirectory=root, UseShellExecute=false, CreateNoWindow=true, RedirectStandardOutput=true, RedirectStandardError=true };
    start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";
    start.Environment["ConnectionStrings__Xamou"] = configBancoTeste.GetConnectionString("Xamou")!;
    start.Environment["Jwt__SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    app=Process.Start(start)!;
    app.OutputDataReceived += (_,e)=> { if(e.Data!=null) lock(logs) logs.AppendLine(e.Data); };
    app.ErrorDataReceived += (_,e)=> { if(e.Data!=null) lock(logs) logs.AppendLine(e.Data); };
    app.BeginOutputReadLine(); app.BeginErrorReadLine();
    using var anon=Cliente();
    bool pronto=false;
    for(int i=0;i<40;i++) {
        if(app.HasExited) throw new Exception("Aplicação não iniciou: "+logs);
        try { if((await anon.GetAsync("/Usuario/Login")).IsSuccessStatusCode) {pronto=true;break;} } catch(HttpRequestException){}
        await Task.Delay(250);
    }
    Verificar(pronto,"aplicação iniciada");
    using var ca=Cliente(); using var cb=Cliente(); using var cp=Cliente(); using var cq=Cliente(); using var cad=Cliente();
    var errado=await Post(anon,"/Usuario/Logar",new(){["email"]=tag+"a@example.invalid",["senha"]="incorreta"});
    Verificar(errado.Headers.Location?.ToString()=="/Usuario/Login" &&
        Convert.ToString(Sql("SELECT Senha FROM Usuarios WHERE Id=@Id",("@Id",a)))!.Length==64,"senha legado incorreta não migra");
    await Login(ca,"a"); await Login(cb,"b"); await Login(cp,"p"); await Login(cq,"q"); await Login(cad,"admin");
    Verificar(Convert.ToString(Sql("SELECT Senha FROM Usuarios WHERE Id=@Id",("@Id",a)))!.Length>64,"login migra SHA-256 no banco");
    Verificar((await cad.GetAsync("/Admin/Dashboard")).IsSuccessStatusCode,"admin por perfil com email diferente");
    Verificar((await ca.GetAsync("/Admin/Dashboard")).StatusCode==HttpStatusCode.Forbidden,"cliente sem acesso administrativo");
    Verificar((await anon.GetAsync("/Admin/Dashboard")).StatusCode==HttpStatusCode.Redirect,"anônimo sem acesso administrativo");
    Verificar((await Post(cp,"/Servico/Editar",new(){["id"]=s.ToString()},false)).StatusCode==HttpStatusCode.BadRequest,"POST sem antiforgery rejeitado");
    Verificar((await anon.GetAsync("/Servico/Excluir/"+s)).StatusCode==HttpStatusCode.MethodNotAllowed,"GET não exclui serviço");
    Verificar((await Post(anon,"/Agendamento/Confirmar/"+s)).StatusCode==HttpStatusCode.Redirect,"anônimo não confirma pedido");
    var campos=new Dictionary<string,string> {["nome"]=tag+"http",["subcategoriaId"]=sub.ToString(),["descricao"]="Teste",
        ["atendimento"]="Online",["linkOnline"]="https://example.invalid",["diasSemana"]="0,1,2,3,4,5,6",
        ["horaInicio"]="08:00",["horaFim"]="18:00",["tipoPreco"]="Fixo",["precoBase"]="100"};
    var criado=await Post(cp,"/Servico/Salvar",campos);
    int criadoId=Id("SELECT ISNULL(MAX(Id),0) FROM Servicos WHERE Nome=@N",("@N",tag+"http"));
    if(criadoId>0)servicos.Add(criadoId);
    Verificar(criado.StatusCode==HttpStatusCode.Redirect&&criadoId>0,"profissional cria serviço");
    campos["id"]=criadoId.ToString(); campos["nome"]=tag+"editado";
    Verificar((await Post(cp,"/Servico/Editar",new(campos))).StatusCode==HttpStatusCode.Redirect &&
        Convert.ToString(Sql("SELECT Nome FROM Servicos WHERE Id=@Id",("@Id",criadoId)))==tag+"editado","edita serviço próprio");
    Verificar((await Post(cq,"/Servico/Editar",new(campos))).StatusCode==HttpStatusCode.Forbidden,"não edita serviço alheio");
    Verificar((await Post(cq,"/Servico/Excluir/"+criadoId)).StatusCode==HttpStatusCode.Forbidden,"não exclui serviço alheio");
    Verificar((await Post(ca,"/Servico/Salvar",new(campos))).StatusCode==HttpStatusCode.Forbidden,"cliente não cria serviço profissional");
    int local=Id("INSERT INTO Locais(ProfissionalId,Nome,Endereco) OUTPUT INSERTED.Id VALUES(@P,'Teste','Rua teste')",("@P",prof));
    Verificar((await Post(cq,"/Local/Salvar",new(){["id"]=local.ToString(),["nome"]="Alterado",["endereco"]="Rua"})).StatusCode==HttpStatusCode.Forbidden,"não edita local alheio");
    Verificar((await Post(cq,"/Local/Excluir/"+local)).StatusCode==HttpStatusCode.Forbidden,"não exclui local alheio");
    var dia=DateTime.Today.AddDays(2);
    await RejeitarReserva(ca,s,DateTime.Today.AddDays(-1),"10:00","data passada rejeitada");
    await RejeitarReserva(ca,s,DateTime.Today,"00:00","horário passado hoje rejeitado");
    await RejeitarReserva(ca,s,dia,"23:00","fora da disponibilidade rejeitado");
    await RejeitarReserva(ca,inativo,dia,"10:00","serviço inativo rejeitado");
    await RejeitarReserva(cp,s,dia,"10:00","autoagendamento rejeitado");
    Sql("INSERT INTO BloqueiosAgenda(ProfissionalId,Data,HoraInicio,HoraFim) VALUES(@P,@D,'12:00','13:00')",("@P",prof),("@D",dia));
    await RejeitarReserva(ca,s,dia,"12:00","bloqueio de agenda respeitado");
    var reserva=await Post(ca,"/Agendamento/Salvar",Reserva(s,dia,"10:00"));
    int ag=ReservaId(a,s);
    Verificar(reserva.StatusCode==HttpStatusCode.Redirect && ag>0,"cliente cria reserva e recebe ID");
    Verificar(Id("SELECT COUNT(*) FROM Notificacoes WHERE ReferenciaId=@Id AND Tipo='Agendamento'",("@Id",ag))==2,"notificações referenciam ID real");
    Verificar((await Post(cb,"/Agendamento/Cancelar/"+ag)).StatusCode==HttpStatusCode.Forbidden &&
        Convert.ToString(Sql("SELECT Status FROM Agendamentos WHERE Id=@Id",("@Id",ag)))=="Pendente","cliente não cancela reserva alheia");
    Verificar((await Post(cq,"/Agendamento/Confirmar/"+ag)).StatusCode==HttpStatusCode.Forbidden,"profissional não confirma pedido alheio");
    Verificar((await Post(cp,"/Agendamento/Confirmar/"+ag)).StatusCode==HttpStatusCode.Redirect,"profissional confirma próprio pedido");
    await RejeitarReserva(cb,s2,dia,"10:00","conflito entre serviços do mesmo profissional");
    await RejeitarReserva(cb,s2,dia,"10:30","sobreposição parcial de uma hora");
    Verificar((await Post(ca,"/Agendamento/Cancelar/"+ag)).StatusCode==HttpStatusCode.Redirect &&
        Convert.ToString(Sql("SELECT Status FROM Agendamentos WHERE Id=@Id",("@Id",ag)))=="CanceladoCliente","cliente cancela próprio agendamento");
    Verificar(Sql("SELECT DataCancelamento FROM Agendamentos WHERE Id=@Id",("@Id",ag)) is DateTime,"data de cancelamento persistida");
    Verificar((await Post(cp,"/Agendamento/Confirmar/"+ag)).StatusCode==HttpStatusCode.Conflict,"cancelado não volta a confirmado");

    var ta=await Token(ca); var tb=await Token(cb);
    var fa=Reserva(s,dia,"10:00");fa["__RequestVerificationToken"]=ta;
    var fb=Reserva(s2,dia,"10:00");fb["__RequestVerificationToken"]=tb;
    int antes=TotalReservas();
    var simultaneas=await Task.WhenAll(ca.PostAsync("/Agendamento/Salvar",new FormUrlEncodedContent(fa)),
        cb.PostAsync("/Agendamento/Salvar",new FormUrlEncodedContent(fb)));
    Verificar(simultaneas.All(x=>(int)x.StatusCode<500)&&TotalReservas()==antes+1,"duas reservas concorrentes: somente uma gravada");
    Verificar(Id("SELECT COUNT(*) FROM Agendamentos WHERE ProfissionalId=@P AND Data=@D AND Hora='10:00' AND Status NOT LIKE 'Cancelado%'",("@P",prof),("@D",dia))==1,"cancelamento libera horário sem duplicar reservas");
    ta=await Token(ca); tb=await Token(cb);
    fa=Reserva(s,dia,"11:00");fa["__RequestVerificationToken"]=ta;
    fb=Reserva(s,dia,"11:00");fb["__RequestVerificationToken"]=tb;
    antes=TotalReservas();
    simultaneas=await Task.WhenAll(ca.PostAsync("/Agendamento/Salvar",new FormUrlEncodedContent(fa)),
        cb.PostAsync("/Agendamento/Salvar",new FormUrlEncodedContent(fb)));
    Verificar(simultaneas.All(x=>(int)x.StatusCode<500)&&TotalReservas()==antes+1,"concorrência no mesmo serviço: uma reserva");
    foreach(var pagina in new[]{"/Servico/Criar","/Profissional/MeusServicos","/Local/Lista","/Agendamento/Recebidos"}) {
        var html=await cp.GetStringAsync(pagina);
        Verificar(html.Contains("__RequestVerificationToken"),"token renderizado em "+pagina);
    }
    var meus=await ca.GetStringAsync("/Agendamento/Meus");
    Verificar(meus.Contains("enviarPost('/Agendamento/ConfirmarCliente/'"),"conclusão no frontend envia POST");
    var telaNovo=await ca.GetStringAsync("/Agendamento/Novo?servicoId="+s+"&data="+dia.ToString("yyyy-MM-dd"));
    Verificar(telaNovo.Contains("__RequestVerificationToken"),"formulário de reserva tem token");
    using(var cadastro=Cliente()) {
        var r=await Post(cadastro,"/Usuario/Cadastrar",new(){["nome"]=tag+"cadastro",["email"]=tag+"cadastro@example.invalid",
            ["senha"]=senha,["tipo"]="admin",["telefone"]=""});
        int novo=Id("SELECT ISNULL(MAX(Id),0) FROM Usuarios WHERE Email=@Email",("@Email",tag+"cadastro@example.invalid"));
        if(novo>0)usuarios.Add(novo);
        Verificar(r.StatusCode==HttpStatusCode.Redirect&&novo>0&&
            Convert.ToString(Sql("SELECT Tipo FROM Usuarios WHERE Id=@Id",("@Id",novo)))=="cliente","cadastro público não concede admin");
    }
    Verificar((await Post(anon,"/Usuario/Logar",new(){["email"]="teste",["senha"]="teste"},false)).StatusCode==HttpStatusCode.BadRequest,"login sem token rejeitado");
    // Fixture passada para testar conclusão sem alterar relógio nem dados reais.
    int clienteId=Id("SELECT Id FROM Clientes WHERE UsuarioId=@U",("@U",a));
    int concluivel=Id(@"INSERT INTO Agendamentos(ClienteId,ProfissionalId,ServicoId,Data,Hora,Status,ConfirmadoProfissional)
        OUTPUT INSERTED.Id VALUES(@C,@P,@S,@D,'08:00','Confirmado',1)",("@C",clienteId),("@P",prof),("@S",s),("@D",DateTime.Today.AddDays(-1)));
    Verificar((await Post(cq,"/Agendamento/Finalizar",new(){["id"]=concluivel.ToString(),["valorFinal"]="100"})).StatusCode==HttpStatusCode.Forbidden,"não finaliza atendimento alheio");
    Verificar((await Post(cp,"/Agendamento/Finalizar",new(){["id"]=concluivel.ToString(),["valorFinal"]="1"})).StatusCode==HttpStatusCode.Redirect &&
        Convert.ToString(Sql("SELECT Status FROM Agendamentos WHERE Id=@Id",("@Id",concluivel)))=="AguardandoCliente","profissional finaliza e aguarda cliente");
    Verificar(Convert.ToDecimal(Sql("SELECT ValorFinal FROM Agendamentos WHERE Id=@Id",("@Id",concluivel)))==100m,"preço fixo vem do banco");
    Verificar((await Post(cb,"/Agendamento/ConfirmarCliente/"+concluivel)).StatusCode==HttpStatusCode.Forbidden,"cliente não conclui atendimento alheio");
    Verificar((await Post(ca,"/Agendamento/ConfirmarCliente/"+concluivel)).StatusCode==HttpStatusCode.Redirect,"cliente confirma conclusão");
    var avaliacao=new Dictionary<string,string>{["agendamentoId"]=concluivel.ToString(),["profissionalId"]=prof.ToString(),["nota"]="5",["comentario"]="Teste"};
    Verificar((await Post(cb,"/Avaliacao/Salvar",new(avaliacao))).StatusCode==HttpStatusCode.Forbidden,"não avalia atendimento alheio");
    var adulterada=new Dictionary<string,string>(avaliacao){["profissionalId"]=outroProf.ToString()};
    Verificar((await Post(ca,"/Avaliacao/Salvar",adulterada)).StatusCode==HttpStatusCode.BadRequest,"não troca profissional avaliado");
    Verificar((await Post(ca,"/Avaliacao/Salvar",new(avaliacao))).StatusCode==HttpStatusCode.Redirect,"avalia atendimento próprio");
    Verificar((await Post(ca,"/Avaliacao/Salvar",new(avaliacao))).StatusCode==HttpStatusCode.Conflict,"avaliação duplicada rejeitada");
    int notif=Id("SELECT TOP 1 Id FROM Notificacoes WHERE UsuarioId=@U",("@U",a));
    Verificar((await Post(cb,"/Notificacao/MarcarComoLida/"+notif)).StatusCode==HttpStatusCode.NotFound,"não marca notificação alheia");
    var req=new HttpRequestMessage(HttpMethod.Post,"/Notificacao/MarcarComoLidaAjax"){
        Content=new StringContent("{\"id\":"+notif+"}",Encoding.UTF8,"application/json")};
    req.Headers.Add("X-CSRF-TOKEN",await Token(ca));
    Verificar((await ca.SendAsync(req)).IsSuccessStatusCode,"AJAX com token no cabeçalho");
    Verificar((await Post(ca,"/Notificacao/MarcarTodasLidas")).IsSuccessStatusCode,"marcar todas restrito ao usuário");
    Verificar((await Post(ca,"/Pagamento/ConfirmarPremium")).StatusCode==HttpStatusCode.Forbidden,"cliente não ativa plano profissional");
    Verificar((await Post(ca,"/Usuario/Logout")).StatusCode==HttpStatusCode.Redirect &&
        (await ca.GetAsync("/Usuario/Perfil")).StatusCode==HttpStatusCode.Redirect,"logout limpa sessão");
    if (args.Contains("--sprint2") || (args.Contains("--sprint3") || (args.Contains("--sprint4") || args.Contains("--sprint5"))))
    {
        Console.WriteLine("SPRINT 1: " + passou + " verificações preservadas.");
        Verificar(Id("SELECT COUNT(*) FROM Disponibilidade WHERE ServicoId=@S",("@S",criadoId))==7,
            "Sprint 2: serviço criado/editado conserva todas as regras");
        var daoServico = new ServicoDAO();
        var edicao = daoServico.BuscarPorId(criadoId) ?? throw new Exception("Serviço de teste não encontrado.");
        var nomeAntes = edicao.Nome;
        edicao.SubcategoriaId = -1;
        bool falhou = false;
        try { daoServico.SalvarComDisponibilidade(edicao, new[]{1,2}, TimeSpan.FromHours(9), TimeSpan.FromHours(12)); }
        catch (SqlException) { falhou = true; }
        Verificar(falhou && daoServico.BuscarPorId(criadoId)?.Nome == nomeAntes &&
            Id("SELECT COUNT(*) FROM Disponibilidade WHERE ServicoId=@S",("@S",criadoId))==7,
            "Sprint 2: falha SQL preserva serviço e regras anteriores");
        var novoInvalido = daoServico.BuscarPorId(criadoId) ?? throw new Exception("Serviço de teste não encontrado.");
        novoInvalido.Id = 0; novoInvalido.SubcategoriaId = -1; novoInvalido.Nome = tag+"invalido";
        falhou=false;
        try { daoServico.SalvarComDisponibilidade(novoInvalido,new[]{1},TimeSpan.FromHours(9),TimeSpan.FromHours(12)); }
        catch(SqlException) { falhou=true; }
        Verificar(falhou && Id("SELECT COUNT(*) FROM Servicos WHERE Nome=@N",("@N",novoInvalido.Nome))==0,
            "Sprint 2: criação inválida não deixa serviço parcial");
        edicao = daoServico.BuscarPorId(criadoId) ?? throw new Exception("Serviço de teste não encontrado.");
        daoServico.SalvarComDisponibilidade(edicao,new[]{1,1,2},TimeSpan.FromHours(9),TimeSpan.FromHours(12));
        Verificar(Id("SELECT COUNT(*) FROM Disponibilidade WHERE ServicoId=@S",("@S",criadoId))==2,
            "Sprint 2: edição substitui regras e elimina dias duplicados");
        Sql("UPDATE Servicos SET LocalId=@L WHERE Id=@S",("@L",local),("@S",criadoId));
        await Post(cp,"/Local/Excluir/"+local);
        Verificar(Id("SELECT COUNT(*) FROM Locais WHERE Id=@L",("@L",local))==1,
            "Sprint 2: local de serviço não pode ser excluído");
        Sql("UPDATE Servicos SET LocalId=NULL WHERE Id=@S",("@S",criadoId));
        Sql("UPDATE Agendamentos SET LocalId=@L WHERE Id=@A",("@L",local),("@A",concluivel));
        await Post(cp,"/Local/Excluir/"+local);
        Verificar(Id("SELECT COUNT(*) FROM Locais WHERE Id=@L",("@L",local))==1,
            "Sprint 2: local somente no histórico não pode ser excluído");
        await Post(cp,"/Local/Salvar",new(){["id"]=local.ToString(),["nome"]="Mudança",["endereco"]="Outro endereço"});
        Verificar(Convert.ToString(Sql("SELECT Endereco FROM Locais WHERE Id=@L",("@L",local)))=="Rua teste",
            "Sprint 2: edição não altera endereço histórico");
        int localLivre=Id("INSERT INTO Locais(ProfissionalId,Endereco) OUTPUT INSERTED.Id VALUES(@P,'Livre')",("@P",prof));
        await Post(cp,"/Local/Excluir/"+localLivre);
        Verificar(Id("SELECT COUNT(*) FROM Locais WHERE Id=@L",("@L",localLivre))==0,
            "Sprint 2: local sem vínculo pode ser excluído");
        var tela = await cb.GetStringAsync("/Agendamento/Novo?servicoId="+s+"&data="+dia.ToString("yyyy-MM-dd"));
        Verificar(!tela.Contains("selecionarHora('12:00:00'") && !tela.Contains("selecionarHora('10:00:00'"),
            "Sprint 2: tela omite bloqueio e reserva ocupada");
        Verificar(tela.Contains("<button type=\"button\" class=\"slot livre\""),
            "Sprint 2: seleção de horário usa botão acessível");
        Sql("INSERT INTO BloqueiosAgenda(ProfissionalId,Data,HoraInicio,HoraFim) VALUES(@P,@D,'13:30','14:30')",("@P",prof),("@D",dia));
        tela=await cb.GetStringAsync("/Agendamento/Novo?servicoId="+s+"&data="+dia.ToString("yyyy-MM-dd"));
        Verificar(!tela.Contains("selecionarHora('13:00:00'") && !tela.Contains("selecionarHora('14:00:00'"),
            "Sprint 2: tela omite sobreposições parciais de bloqueios");
        int historicoAntes=TotalReservas();
        await Post(cp,"/Servico/Excluir/"+s);
        Verificar(daoServico.BuscarPorId(s)?.Ativo == false && TotalReservas()==historicoAntes &&
            Id("SELECT COUNT(*) FROM Avaliacoes WHERE AgendamentoId=@A",("@A",concluivel))==1,
            "Sprint 2: remoção de serviço mantém reservas e avaliação");
        await RejeitarReserva(cb,s,dia,"15:00","Sprint 2: serviço desativado não recebe nova reserva");
        var cadastroHtml=await anon.GetStringAsync("/Usuario/Cadastro");
        Verificar(cadastroHtml.Contains("id=\"formCadastro\""),"Sprint 2: formulário ligado à validação JavaScript");
        var locaisHtml=await cp.GetStringAsync("/Local/Lista?abrir=true");
        Verificar(locaisHtml.Contains("const abrir = 'True'"),"Sprint 2: link de criação de local abre formulário");
        foreach(var pagina in new[]{"/Usuario/Perfil","/Profissional/MeusServicos","/Profissional/MeuPerfil",
            "/Profissional/Dashboard","/Notificacao","/Notificacao/Ultimas","/Pagamento/CheckoutPremium",
            "/Suporte/ContatoAjuda","/Agendamento/Recebidos","/Home/Index"})
            Verificar((await cp.GetAsync(pagina)).IsSuccessStatusCode,"Sprint 2: rota atual "+pagina);
    }
    if ((args.Contains("--sprint3") || (args.Contains("--sprint4") || args.Contains("--sprint5"))))
    {
        Console.WriteLine("BASELINE: " + passou + " verificações anteriores preservadas.");
        var booking = new AgendamentoService(new(),new(),new(),new(),new(),new(),new());
        var catalogo = new ServicoService(new(),new(),new(),new(),new(),new(),new());
        var ctxP = new UsuarioContexto(p,"profissional");
        var ctxQ = new UsuarioContexto(q,"profissional");
        int u3 = Usuario("sprint3","cliente");
        var ctxC = new UsuarioContexto(u3,"cliente");
        var ctxB = new UsuarioContexto(b,"cliente");
        void Falha(Action executar, TipoFalha esperada, string nome) {
            try { executar(); throw new Exception("Operação deveria falhar: "+nome); }
            catch(FalhaOperacao e) { Verificar(e.Tipo==esperada,"Sprint 3: "+nome); }
        }
        SalvarServicoRequest DadosServico() => new() {
            Nome=tag+"app", SubcategoriaId=sub, Atendimento="Online", LinkOnline="https://example.invalid",
            TipoPreco="Fixo", PrecoBase=100, DiasSemana="0,1,2,3,4,5,6",
            HoraInicio=TimeSpan.FromHours(8), HoraFim=TimeSpan.FromHours(18)
        };
        Falha(()=>catalogo.Criar(ctxC,DadosServico()),TipoFalha.SemPermissao,"criação exige profissional");
        Falha(()=>catalogo.Criar(new(p,"cliente"),DadosServico()),TipoFalha.SemPermissao,"contexto adulterado recusado");
        var d3=DadosServico();
        int s3=catalogo.Criar(ctxP,d3); servicos.Add(s3);
        Verificar(s3>0 && Id("SELECT COUNT(*) FROM Disponibilidade WHERE ServicoId=@S",("@S",s3))==7,
            "Sprint 3: Service cria serviço com regras");
        d3.Id=s3; d3.Nome=tag+"app-edit";
        catalogo.Editar(ctxP,d3);
        Verificar(new ServicoDAO().BuscarPorId(s3)?.Nome==d3.Nome,"Sprint 3: Service edita serviço");
        Falha(()=>catalogo.Editar(ctxQ,d3),TipoFalha.SemPermissao,"edição exige propriedade");
        Falha(()=>catalogo.Remover(ctxQ,s3),TipoFalha.SemPermissao,"remoção exige propriedade");
        d3.Atendimento="Local";d3.LocalId=-1;
        Falha(()=>catalogo.Editar(ctxP,d3),TipoFalha.Validacao,"local inválido");
        d3.Atendimento="Online";d3.DiasSemana="9";
        Falha(()=>catalogo.Editar(ctxP,d3),TipoFalha.Validacao,"disponibilidade inválida");
        var dReserva=DateTime.Today.AddDays(9);
        CriarAgendamentoRequest Pedido(int servico, int hora) => new() {
            ServicoId=servico, Data=dReserva, Hora=TimeSpan.FromHours(hora), Descricao="Service Sprint 3"
        };
        Falha(()=>booking.Criar(new(0,"cliente"),Pedido(s3,10)),TipoFalha.NaoAutenticado,"usuário não autenticado");
        Falha(()=>booking.Criar(ctxC,Pedido(-1,10)),TipoFalha.NaoEncontrado,"serviço inexistente");
        Falha(()=>booking.Criar(ctxC,Pedido(inativo,10)),TipoFalha.Validacao,"serviço inativo");
        Falha(()=>booking.Criar(ctxC,Pedido(s3,23)),TipoFalha.Validacao,"fora da disponibilidade");
        Falha(()=>booking.Criar(ctxP,Pedido(s3,10)),TipoFalha.Validacao,"autoagendamento recusado");
        Sql("INSERT INTO BloqueiosAgenda(ProfissionalId,Data,HoraInicio,HoraFim) VALUES(@P,@D,'12:00','13:00')",("@P",prof),("@D",dReserva));
        Falha(()=>booking.Criar(ctxC,Pedido(s3,12)),TipoFalha.Conflito,"bloqueio tipado");
        int ag3=booking.Criar(ctxC,Pedido(s3,10));
        Verificar(ag3>0,"Sprint 3: Service cria reserva válida");
        Falha(()=>booking.Criar(ctxB,Pedido(s3,10)),TipoFalha.Conflito,"conflito tipado");
        Falha(()=>booking.Confirmar(ctxQ,ag3),TipoFalha.SemPermissao,"confirmação exige propriedade");
        Falha(()=>booking.Cancelar(ctxB,ag3),TipoFalha.SemPermissao,"cancelamento exige propriedade");
        var dao3=new AgendamentoDAO();
        string Estado() => Convert.ToString(Sql("SELECT Status FROM Agendamentos WHERE Id=@A",("@A",ag3))) ?? "";
        int Nots() => Id("SELECT COUNT(*) FROM Notificacoes WHERE ReferenciaId=@A AND UsuarioId IN (@C,@P)",
            ("@A",ag3),("@C",u3),("@P",p));
        Notificacao[] FalharNotificacao() => [
            new(){UsuarioId=u3,Titulo="Teste",Mensagem="Rollback",Tipo="Agendamento",ReferenciaId=ag3},
            new(){UsuarioId=-1,Titulo="Teste",Mensagem="FK inválida",Tipo="Agendamento",ReferenciaId=ag3}
        ];
        void Rollback(Func<bool> executar,string estado,string nome) {
            int antesN=Nots(); bool falhou=false;
            try { executar(); } catch(SqlException){falhou=true;}
            Verificar(falhou && Estado()==estado && Nots()==antesN,"Sprint 3: rollback "+nome+" e notificações");
        }
        Rollback(()=>dao3.Confirmar(ag3,prof,FalharNotificacao()),"Pendente","confirmação");
        Rollback(()=>dao3.Cancelar(ag3,"CanceladoProfissional",p,FalharNotificacao(),true),"Pendente","recusa");
        booking.Confirmar(ctxP,ag3);
        Verificar(Estado()=="Confirmado" && Nots()==3,"Sprint 3: confirmação e aviso persistidos juntos");
        Falha(()=>booking.Recusar(ctxP,ag3),TipoFalha.Conflito,"recusa não cancela confirmado");
        Rollback(()=>dao3.Cancelar(ag3,"CanceladoCliente",u3,FalharNotificacao()),"Confirmado","cancelamento");
        Verificar(Sql("SELECT DataCancelamento FROM Agendamentos WHERE Id=@A",("@A",ag3)) is DBNull,
            "Sprint 3: rollback restaura data de cancelamento");
        Sql("UPDATE Agendamentos SET Data=@D WHERE Id=@A",("@D",DateTime.Today.AddDays(-1)),("@A",ag3));
        Rollback(()=>dao3.Finalizar(ag3,prof,100,4,96,FalharNotificacao()),"Confirmado","finalização");
        Verificar(Sql("SELECT ValorFinal FROM Agendamentos WHERE Id=@A",("@A",ag3)) is DBNull,
            "Sprint 3: rollback restaura valores financeiros");
        booking.Finalizar(ctxP,ag3,1);
        Verificar(Estado()=="AguardandoCliente" && Convert.ToDecimal(Sql("SELECT ValorFinal FROM Agendamentos WHERE Id=@A",("@A",ag3)))==100,
            "Sprint 3: finalização usa preço fixo do banco");
        Rollback(()=>dao3.ConfirmarCliente(ag3,u3,FalharNotificacao()),"AguardandoCliente","conclusão");
        Falha(()=>booking.ConfirmarConclusao(ctxB,ag3),TipoFalha.SemPermissao,"conclusão exige contratante");
        booking.ConfirmarConclusao(ctxC,ag3);
        booking.PermitirAvaliacao(ctxC,ag3);
        Verificar(Estado()=="Finalizado","Sprint 3: conclusão permite avaliação");
        booking.Avaliar(ctxC,new(){AgendamentoId=ag3,ProfissionalId=prof,Nota=5});
        Falha(()=>booking.PermitirAvaliacao(ctxC,ag3),TipoFalha.Conflito,"avaliação duplicada tipada");
        int cancelar3=booking.Criar(ctxC,Pedido(s3,11));
        booking.Cancelar(ctxC,cancelar3);
        Verificar(new AgendamentoDAO().BuscarPorId(cancelar3)?.Status=="CanceladoCliente",
            "Sprint 3: Service cancela reserva");
        int recusar3=booking.Criar(ctxC,Pedido(s3,11));
        booking.Recusar(ctxP,recusar3);
        Verificar(new AgendamentoDAO().BuscarPorId(recusar3)?.Status=="CanceladoProfissional",
            "Sprint 3: Service recusa solicitação");
        catalogo.Desativar(ctxP,s3);
        Verificar(new ServicoDAO().BuscarPorId(s3)?.Ativo==false && new AgendamentoDAO().BuscarPorId(ag3)!=null,
            "Sprint 3: Service preserva histórico ao remover oferta");
    }
    if ((args.Contains("--sprint4") || args.Contains("--sprint5")))
    {
        Console.WriteLine("BASELINE SPRINT 4: " + passou + " verificações anteriores preservadas.");
        int inicioSprint4 = passou;
        int apiUsuario = Usuario("api4", "cliente", true);
        using var apiC = Cliente(); using var apiB = Cliente(); using var apiP = Cliente();
        using var apiQ = Cliente(); using var apiAdmin = Cliente();
        async Task<System.Text.Json.JsonElement> Api(HttpClient client, HttpMethod method, string path,
            object? body, int expected, string nome)
        {
            using var request = new HttpRequestMessage(method, path);
            if (body != null) request.Content = System.Net.Http.Json.JsonContent.Create(body);
            using var response = await client.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            Verificar((int)response.StatusCode == expected, "Sprint 4: " + nome + " (HTTP " + (int)response.StatusCode + ")");
            if (expected >= 400) {
                using var error = System.Text.Json.JsonDocument.Parse(json);
                if (!error.RootElement.TryGetProperty("erro", out var detalhe) ||
                    !detalhe.TryGetProperty("codigo", out _) || json.Contains("SqlException") ||
                    json.Contains("StackTrace", StringComparison.OrdinalIgnoreCase) || json.Contains("SQLEXPRESS"))
                    throw new Exception("Contrato de erro inválido: " + nome);
            }
            if (string.IsNullOrEmpty(json)) return default;
            using var document = System.Text.Json.JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
        async Task<string> ApiLogin(HttpClient client, string sufixo)
        {
            var result = await Api(client, HttpMethod.Post, "/api/v1/auth/login",
                new { email=tag+sufixo+"@example.invalid", senha }, 200, "login " + sufixo);
            string token = result.GetProperty("accessToken").GetString() ?? throw new Exception("Token ausente.");
            client.DefaultRequestHeaders.Authorization = new("Bearer", token);
            return token;
        }
        await Api(anon,HttpMethod.Get,"/api/v1/auth/me",null,401,"me exige token");
        await Api(cp,HttpMethod.Get,"/api/v1/auth/me",null,401,"sessão MVC não autentica API");
        await Api(anon,HttpMethod.Post,"/api/v1/auth/login",new {email=tag+"api4@example.invalid",senha="errada"},401,"senha incorreta");
        await Api(anon,HttpMethod.Post,"/api/v1/auth/login",new {email=tag+"ausente@example.invalid",senha},401,"usuário inexistente");
        string accessToken = await ApiLogin(apiC,"api4");
        await ApiLogin(apiB,"b"); await ApiLogin(apiP,"p"); await ApiLogin(apiQ,"q"); await ApiLogin(apiAdmin,"admin");
        Verificar(Convert.ToString(Sql("SELECT Senha FROM Usuarios WHERE Id=@U",("@U",apiUsuario)))?.Length>64,
            "Sprint 4: API reutiliza migração de hash legado");
        var jwtLido = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        Verificar(jwtLido.Claims.All(c => new[]{"sub","role","jti","nbf","exp","iss","aud"}.Contains(c.Type)) &&
            jwtLido.ValidTo>DateTime.UtcNow && jwtLido.ValidTo<DateTime.UtcNow.AddMinutes(16),
            "Sprint 4: claims mínimas e expiração curta");
        var me = await Api(apiC,HttpMethod.Get,"/api/v1/auth/me",null,200,"usuário atual");
        Verificar(me.GetProperty("id").GetInt32()==apiUsuario && me.EnumerateObject().Count()==4 &&
            !me.TryGetProperty("senha",out _) && !me.TryGetProperty("telefone",out _),"Sprint 4: me usa DTO sem dados internos");
        Verificar((await apiC.GetAsync("/Agendamento/Meus")).StatusCode==HttpStatusCode.Redirect,
            "Sprint 4: JWT não cria sessão MVC");
        using var tokenRuim = Cliente(); tokenRuim.DefaultRequestHeaders.Authorization=new("Bearer","invalido");
        await Api(tokenRuim,HttpMethod.Get,"/api/v1/auth/me",null,401,"token inválido");
        string TokenTeste(string issuer, string audience, DateTime expires, byte[] key) {
            var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(issuer,audience,
                new[]{new System.Security.Claims.Claim("sub",apiUsuario.ToString()),new System.Security.Claims.Claim("role","cliente")},
                expires.AddMinutes(-15),expires,new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256));
            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(jwt);
        }
        foreach(var invalid in new[]{
            TokenTeste("Trampo","Trampo.Api",DateTime.UtcNow.AddMinutes(-2),Convert.FromBase64String(start.Environment["Jwt__SigningKey"]!)),
            TokenTeste("Outro","Trampo.Api",DateTime.UtcNow.AddMinutes(5),Convert.FromBase64String(start.Environment["Jwt__SigningKey"]!)),
            TokenTeste("Trampo","Outra",DateTime.UtcNow.AddMinutes(5),Convert.FromBase64String(start.Environment["Jwt__SigningKey"]!)),
            TokenTeste("Trampo","Trampo.Api",DateTime.UtcNow.AddMinutes(5),RandomNumberGenerator.GetBytes(32))}) {
            tokenRuim.DefaultRequestHeaders.Authorization=new("Bearer",invalid);
            await Api(tokenRuim,HttpMethod.Get,"/api/v1/auth/me",null,401,"rejeita expiração/issuer/audience/assinatura inválidos");
        }
        Sql("UPDATE Usuarios SET Tipo='admin' WHERE Id=@U",("@U",apiUsuario));
        try { await Api(apiC,HttpMethod.Get,"/api/v1/auth/me",null,401,"token rejeitado após alteração de perfil"); }
        finally { Sql("UPDATE Usuarios SET Tipo='cliente' WHERE Id=@U",("@U",apiUsuario)); }
        await Api(apiAdmin,HttpMethod.Get,"/api/v1/agendamentos",null,403,"admin não assume papel de participante");
        await Api(anon,HttpMethod.Get,"/api/v1/servicos?tamanho=2",null,200,"catálogo público");
        await Api(anon,HttpMethod.Get,"/api/v1/servicos?tamanho=101",null,400,"limite de paginação");
        await Api(anon,HttpMethod.Get,"/api/v1/servicos/"+inativo,null,404,"catálogo não expõe inativo");
        await Api(anon,HttpMethod.Get,"/api/v1/servicos/-1",null,404,"serviço inexistente");
        var oferta = new BD_TRAMPO.Api.Contracts.ServicoRequest {
            SubcategoriaId=sub, Nome=tag+"api-oferta", Atendimento="Online", LinkOnline="https://example.invalid/reuniao",
            TipoPreco="Fixo",PrecoBase=100,DiasSemana="0,1,2,3,4,5,6",HoraInicio=TimeSpan.FromHours(8),HoraFim=TimeSpan.FromHours(18)
        };
        await Api(anon,HttpMethod.Post,"/api/v1/servicos",oferta,401,"criação exige JWT");
        await Api(apiC,HttpMethod.Post,"/api/v1/servicos",oferta,403,"cliente não cria serviço");
        var criadoApi = await Api(apiP,HttpMethod.Post,"/api/v1/servicos",oferta,201,"profissional cria serviço");
        int apiServico = criadoApi.GetProperty("id").GetInt32(); servicos.Add(apiServico);
        var consulta = await Api(anon,HttpMethod.Get,"/api/v1/servicos/"+apiServico,null,200,"consulta serviço criado");
        Verificar(consulta.GetProperty("profissionalId").GetInt32()==prof && !consulta.TryGetProperty("linkOnline",out _),
            "Sprint 4: proprietário derivado e link privado fora do catálogo");
        oferta.Nome=tag+"api-editado";
        await Api(apiP,HttpMethod.Put,"/api/v1/servicos/"+apiServico,oferta,204,"editar próprio serviço");
        await Api(apiQ,HttpMethod.Put,"/api/v1/servicos/"+apiServico,oferta,403,"não editar serviço alheio");
        await Api(apiQ,HttpMethod.Delete,"/api/v1/servicos/"+apiServico,null,403,"não remover serviço alheio");
        var adulterarServico = System.Text.Json.JsonSerializer.SerializeToNode(oferta)!;
        adulterarServico["profissionalId"]=outroProf;
        await Api(apiP,HttpMethod.Post,"/api/v1/servicos",adulterarServico,400,"ID de proprietário no JSON é recusado");
        var diaApi=DateTime.Today.AddDays(12);
        object PedidoApi(int hora, int? servicoId=null) => new {servicoId=servicoId??apiServico,data=diaApi.ToString("yyyy-MM-dd"),hora=$"{hora:00}:00:00",descricao="Reserva API"};
        await Api(apiC,HttpMethod.Post,"/api/v1/agendamentos",PedidoApi(23),400,"horário fora da disponibilidade");
        await Api(apiC,HttpMethod.Post,"/api/v1/agendamentos",PedidoApi(10,inativo),400,"não reserva inativo");
        var adulterarReserva = System.Text.Json.JsonSerializer.SerializeToNode(PedidoApi(10))!;
        adulterarReserva["clienteId"]=b; adulterarReserva["status"]="Finalizado"; adulterarReserva["preco"]=1;
        await Api(apiC,HttpMethod.Post,"/api/v1/agendamentos",adulterarReserva,400,"IDs status e preço não aceitos no payload");
        var agendaApi=await Api(apiC,HttpMethod.Get,$"/api/v1/servicos/{apiServico}/agenda?data={diaApi:yyyy-MM-dd}",null,200,"consulta agenda");
        Verificar(agendaApi.GetProperty("horarios").EnumerateArray().Any(h=>h.GetString()=="10:00:00"),"Sprint 4: agenda serializa horas disponíveis");
        using var reqA=new HttpRequestMessage(HttpMethod.Post,"/api/v1/agendamentos"){Content=System.Net.Http.Json.JsonContent.Create(PedidoApi(10))};
        using var reqB=new HttpRequestMessage(HttpMethod.Post,"/api/v1/agendamentos"){Content=System.Net.Http.Json.JsonContent.Create(PedidoApi(10))};
        var concorrentes=await Task.WhenAll(apiC.SendAsync(reqA),apiB.SendAsync(reqB));
        Verificar(concorrentes.Count(r=>r.StatusCode==HttpStatusCode.Created)==1 && concorrentes.Count(r=>r.StatusCode==HttpStatusCode.Conflict)==1 &&
            Id("SELECT COUNT(*) FROM Agendamentos WHERE ServicoId=@S AND Data=@D AND Hora='10:00'",("@S",apiServico),("@D",diaApi))==1,
            "Sprint 4: concorrência HTTP cria somente uma reserva");
        int vencedor=Array.FindIndex(concorrentes,r=>r.StatusCode==HttpStatusCode.Created);
        using var corpoConcorrente=System.Text.Json.JsonDocument.Parse(await concorrentes[vencedor].Content.ReadAsStringAsync());
        int agApi=corpoConcorrente.RootElement.GetProperty("id").GetInt32();
        HttpClient dono = vencedor==0?apiC:apiB, estranho=vencedor==0?apiB:apiC;
        Verificar(concorrentes[vencedor].Headers.Location?.AbsolutePath==$"/api/v1/agendamentos/{agApi}","Sprint 4: criação retorna Location");
        foreach(var r in concorrentes)r.Dispose();
        var meusApi=await Api(dono,HttpMethod.Get,"/api/v1/agendamentos",null,200,"lista próprios");
        Verificar(meusApi.EnumerateArray().Any(x=>x.GetProperty("id").GetInt32()==agApi && x.GetProperty("servicoId").GetInt32()==apiServico),
            "Sprint 4: lista contém IDs corretos");
        await Api(apiP,HttpMethod.Get,"/api/v1/agendamentos?visao=recebidos",null,200,"profissional lista recebidos");
        await Api(apiC,HttpMethod.Get,"/api/v1/agendamentos?visao=recebidos",null,403,"recebidos exige profissional");
        await Api(dono,HttpMethod.Get,"/api/v1/agendamentos/"+agApi,null,200,"consulta participante");
        await Api(estranho,HttpMethod.Get,"/api/v1/agendamentos/"+agApi,null,403,"não acessa reserva alheia");
        await Api(apiC,HttpMethod.Get,"/api/v1/agendamentos/-1",null,404,"reserva inexistente");
        await Api(estranho,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/cancelar",null,403,"não cancela reserva alheia");
        await Api(dono,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/avaliacao",new {nota=5},400,"avaliação exige conclusão");
        await Api(apiQ,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/confirmar",null,403,"confirmação exige propriedade");
        await Api(apiC,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/confirmar",null,403,"confirmação exige perfil");
        await Api(apiP,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/confirmar",null,204,"confirmar");
        await Api(apiP,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/recusar",null,409,"não recusa já confirmado");
        await Api(apiP,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/finalizar",new {valorFinal=1},400,"não finaliza no futuro");
        Sql("UPDATE Agendamentos SET Data=@D WHERE Id=@A",("@D",DateTime.Today.AddDays(-1)),("@A",agApi));
        await Api(apiP,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/finalizar",new {valorFinal=1},204,"finalizar");
        await Api(estranho,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/confirmar-conclusao",null,403,"outro cliente não conclui");
        await Api(dono,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/confirmar-conclusao",null,204,"confirmar conclusão");
        var finalizado=await Api(dono,HttpMethod.Get,$"/api/v1/agendamentos/{agApi}",null,200,"consulta valor gravado");
        Verificar(finalizado.GetProperty("valorFinal").GetDecimal()==100 && !finalizado.TryGetProperty("precoBase",out _) &&
            finalizado.GetProperty("origemValorFinal").GetString()=="finalizacao_sem_snapshot_da_oferta",
            "Sprint 4: valor persistido e limitação histórica explícita");
        await Api(estranho,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/avaliacao",new {nota=5},403,"outro cliente não avalia");
        await Api(dono,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/avaliacao",new {nota=5,profissionalId=outroProf},400,"avaliação não aceita profissional arbitrário");
        await Api(dono,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/avaliacao",new {nota=5,comentario="Teste API"},204,"avaliação válida");
        await Api(dono,HttpMethod.Post,$"/api/v1/agendamentos/{agApi}/avaliacao",new {nota=5},409,"avaliação duplicada");
        int cancelarApi=(await Api(apiC,HttpMethod.Post,"/api/v1/agendamentos",PedidoApi(11),201,"criar para cancelar")).GetProperty("id").GetInt32();
        await Api(apiC,HttpMethod.Post,$"/api/v1/agendamentos/{cancelarApi}/cancelar",null,204,"cancelar");
        int recusarApi=(await Api(apiC,HttpMethod.Post,"/api/v1/agendamentos",PedidoApi(11),201,"criar para recusar")).GetProperty("id").GetInt32();
        await Api(apiP,HttpMethod.Post,$"/api/v1/agendamentos/{recusarApi}/recusar",null,204,"recusar");
        var removido=await Api(apiP,HttpMethod.Delete,$"/api/v1/servicos/{apiServico}",null,200,"remover com histórico");
        Verificar(removido.GetProperty("resultado").GetString()=="desativado" && Id("SELECT COUNT(*) FROM Agendamentos WHERE Id=@A",("@A",agApi))==1,
            "Sprint 4: DELETE desativa e preserva histórico");
        int livreApi=(await Api(apiP,HttpMethod.Post,"/api/v1/servicos",oferta,201,"criar sem histórico")).GetProperty("id").GetInt32(); servicos.Add(livreApi);
        var excluido=await Api(apiP,HttpMethod.Delete,$"/api/v1/servicos/{livreApi}",null,200,"remover sem histórico");
        Verificar(excluido.GetProperty("resultado").GetString()=="excluido" && new ServicoDAO().BuscarPorId(livreApi)==null,
            "Sprint 4: DELETE informa exclusão física");
        await Api(anon,HttpMethod.Get,"/api/v1/inexistente",null,404,"erro padronizado em rota inexistente");
        using(var malformed = await apiC.PostAsync("/api/v1/agendamentos",new StringContent("{",Encoding.UTF8,"application/json")))
            Verificar(malformed.StatusCode==HttpStatusCode.BadRequest && (await malformed.Content.ReadAsStringAsync()).Contains("VALIDACAO"),
                "Sprint 4: JSON malformado tem erro seguro");
        foreach(var (codigo,tipo) in new[]{(51001,TipoFalha.Conflito),(51002,TipoFalha.NaoEncontrado),(51003,TipoFalha.Validacao)}) {
            bool mapeado=false;
            try { BD_TRAMPO.DAO.FalhasSql.Executar(()=>Sql($"THROW {codigo}, 'Mensagem independente do mapeamento', 1;")); }
            catch(FalhaOperacao e) {mapeado=e.Tipo==tipo;}
            Verificar(mapeado,"Sprint 4: SQL "+codigo+" mapeado pelo número");
        }
        bool tecnico=false;
        try { BD_TRAMPO.DAO.FalhasSql.Executar(()=>Sql("THROW 51999, 'Falha técnica de teste', 1;")); }
        catch(SqlException) {tecnico=true;}
        Verificar(tecnico,"Sprint 4: SQL desconhecido permanece falha técnica");
        var erroContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        erroContext.Request.Path="/api/v1/teste"; erroContext.Response.Body=new MemoryStream();
        erroContext.RequestServices=new Microsoft.Extensions.DependencyInjection.ServiceCollection().AddLogging().BuildServiceProvider();
        var middleware = new BD_TRAMPO.Api.ErrosApiMiddleware(_=>throw new Exception("SEGREDO-SQL-CONNECTION"),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<BD_TRAMPO.Api.ErrosApiMiddleware>.Instance);
        await middleware.InvokeAsync(erroContext); erroContext.Response.Body.Position=0;
        string erroSeguro=await new StreamReader(erroContext.Response.Body).ReadToEndAsync();
        Verificar(erroContext.Response.StatusCode==500 && !erroSeguro.Contains("SEGREDO") && erroSeguro.Contains("ERRO_INTERNO"),
            "Sprint 4: falha técnica não expõe detalhes internos");
        using var openApi = System.Text.Json.JsonDocument.Parse(await anon.GetStringAsync("/openapi/v1.json"));
        var paths=openApi.RootElement.GetProperty("paths");
        Verificar(paths.EnumerateObject().All(x=>x.Name.StartsWith("/api/v1/")) &&
            paths.GetProperty("/api/v1/agendamentos").GetProperty("post").GetProperty("responses").TryGetProperty("201",out _) &&
            paths.GetProperty("/api/v1/auth/me").GetProperty("get").GetProperty("security").GetArrayLength()>0 &&
            openApi.RootElement.GetProperty("components").GetProperty("securitySchemes").TryGetProperty("Bearer",out _),
            "Sprint 4: OpenAPI documenta rotas schemas respostas e Bearer");
        Verificar((await anon.GetAsync("/swagger/index.html")).IsSuccessStatusCode,"Sprint 4: Swagger UI disponível em Development");
        using var corsRequest=new HttpRequestMessage(HttpMethod.Options,"/api/v1/servicos");
        corsRequest.Headers.Add("Origin","https://outro.example.invalid");
        using var corsResponse=await anon.SendAsync(corsRequest);
        Verificar(!corsResponse.Headers.Contains("Access-Control-Allow-Origin"),"Sprint 4: CORS não liberado indiscriminadamente");
        bool limitado=false;
        for(int tentativa=0;tentativa<11;tentativa++) {
            using var r=await anon.PostAsync("/api/v1/auth/login",System.Net.Http.Json.JsonContent.Create(new {email="inexistente@example.invalid",senha="errada"}));
            if ((int)r.StatusCode==429) {limitado=r.Headers.RetryAfter!=null && (await r.Content.ReadAsStringAsync()).Contains("LIMITE_REQUISICOES");break;}
        }
        Verificar(limitado,"Sprint 4: login limitado com 429 e Retry-After");
        Verificar((await Post(cp,"/Usuario/Logar",new(){["email"]=tag+"p@example.invalid",["senha"]=senha})).StatusCode==HttpStatusCode.Redirect,
            "Sprint 4: rate limit da API não interfere no login MVC");
        void FalhaSqlReal(Action executar, TipoFalha tipo, string nome) {
            bool falhou=false;
            try { executar(); } catch(FalhaOperacao e) {falhou=e.Tipo==tipo;}
            Verificar(falhou,"Sprint 4: "+nome);
        }
        var daoApi = new ServicoDAO();
        FalhaSqlReal(()=>daoApi.SalvarComDisponibilidade(new Servico {Id=-1,ProfissionalId=prof},
            new[]{1},TimeSpan.FromHours(8),TimeSpan.FromHours(18)),TipoFalha.NaoEncontrado,"revalidação SQL de serviço ausente");
        FalhaSqlReal(()=>daoApi.SalvarComDisponibilidade(new Servico {Id=apiServico,ProfissionalId=prof,LocalId=-1},
            new[]{1},TimeSpan.FromHours(8),TimeSpan.FromHours(18)),TipoFalha.Validacao,"revalidação SQL de local inválido");
        using (var bloqueioConn=new Conexao().Conectar())
        using (var bloqueioTx=bloqueioConn.BeginTransaction()) {
            using var bloqueioCmd=new SqlCommand("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@Recurso,@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=1000; SELECT @r",bloqueioConn,bloqueioTx);
            bloqueioCmd.Parameters.AddWithValue("@Recurso","TRAMPO:Profissional:"+prof);
            if(Convert.ToInt32(bloqueioCmd.ExecuteScalar())<0) throw new Exception("Não adquiriu bloqueio de teste.");
            FalhaSqlReal(()=>daoApi.Excluir(apiServico),TipoFalha.Conflito,"timeout SQL real mapeado como conflito");
            bloqueioTx.Rollback();
        }
        using(var textoInvalido=await apiC.PostAsync("/api/v1/agendamentos",new StringContent("texto")))
            Verificar((int)textoInvalido.StatusCode==415 && (await textoInvalido.Content.ReadAsStringAsync()).Contains("FORMATO_INVALIDO"),
                "Sprint 4: formato não JSON recusado consistentemente");
        // Mesmo binário em produção, sem chave: MVC disponível e nenhum emissor com segredo padrão.
        app.Kill(true); app.WaitForExit();
        start.Environment.Remove("Jwt__SigningKey");
        start.Environment["ASPNETCORE_ENVIRONMENT"]="Production";
        app=Process.Start(start) ?? throw new Exception("Não iniciou processo de produção de teste.");
        app.OutputDataReceived += (_,e)=> { if(e.Data!=null) lock(logs) logs.AppendLine(e.Data); };
        app.ErrorDataReceived += (_,e)=> { if(e.Data!=null) lock(logs) logs.AppendLine(e.Data); };
        app.BeginOutputReadLine(); app.BeginErrorReadLine();
        bool producaoPronta=false;
        for(int tentativa=0;tentativa<40;tentativa++) {
            if(app.HasExited) throw new Exception("Processo de produção não iniciou.");
            try { if((await anon.GetAsync("/Usuario/Login")).IsSuccessStatusCode) {producaoPronta=true;break;} }
            catch(HttpRequestException) { }
            await Task.Delay(250);
        }
        Verificar(producaoPronta,"Sprint 4: MVC funciona sem chave JWT");
        await Api(anon,HttpMethod.Post,"/api/v1/auth/login",new {email=tag+"api4@example.invalid",senha},503,"sem chave não emite token");
        await Api(apiC,HttpMethod.Get,"/api/v1/auth/me",null,401,"sem chave não aceita token anterior");
        Verificar((await anon.GetAsync("/openapi/v1.json")).StatusCode==HttpStatusCode.NotFound &&
            (await anon.GetAsync("/swagger/index.html")).StatusCode==HttpStatusCode.NotFound,
            "Sprint 4: documentação desabilitada em Production");
        Console.WriteLine("SPRINT 4: "+(passou-inicioSprint4)+" verificações novas.");
    }

    if(args.Contains("--sprint5"))
    {
        Console.WriteLine("BASELINE SPRINT 5: "+passou+" verificações anteriores preservadas.");
        int inicioSprint5=passou;
        using var fornecedor=new EnderecoFakeServer();
        using var httpEndereco=new HttpClient {BaseAddress=new Uri("http://127.0.0.1:5178/ws/"),Timeout=TimeSpan.FromMilliseconds(150)};
        var provider=new BD_TRAMPO.Integrations.Enderecos.ViaCepClient(httpEndereco,Microsoft.Extensions.Logging.Abstractions.NullLogger<BD_TRAMPO.Integrations.Enderecos.ViaCepClient>.Instance);
        using var cacheEndereco=new BD_TRAMPO.Integrations.Enderecos.CacheEnderecos();
        var enderecoService=new EnderecoService(provider,new BD_TRAMPO.Integrations.Enderecos.GeocodificacaoNaoConfigurada(),cacheEndereco,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<EnderecoService>.Instance);
        EnderecoRequest DadosEndereco()=>new(){CEP="01001-000",Logradouro="Praça da Sé",Numero="10",Complemento="Sala 2",Bairro="Sé",Cidade="São Paulo",UF="sp"};
        void Validacao5(Action executar,string nome) {
            bool falhou=false;try{executar();}catch(FalhaOperacao e){falhou=e.Tipo==TipoFalha.Validacao;}
            Verificar(falhou,"Sprint 5: "+nome);
        }
        Verificar(EnderecoService.NormalizarCEP(" 01001-000 ")=="01001000" && EnderecoService.NormalizarUF(" sp ")=="SP","Sprint 5: normaliza CEP e UF");
        foreach(var invalido in new[]{"","0100100","010010000","0100A000","01001 000",new string('1',200)})
            Validacao5(()=>EnderecoService.NormalizarCEP(invalido),"CEP inválido não chega ao fornecedor");
        Validacao5(()=>EnderecoService.NormalizarUF("XX"),"UF inexistente");
        var consultaCep=await enderecoService.ConsultarCEP("01001-000");int chamadasCep=fornecedor.Chamadas;
        await enderecoService.ConsultarCEP("01001000");
        Verificar(consultaCep.Cidade=="São Paulo" && fornecedor.Chamadas==chamadasCep,"Sprint 5: CEP válido e cache normalizado");
        var pesquisa=await enderecoService.Pesquisar("sp","São Paulo","Praça da Sé");int chamadasPesquisa=fornecedor.Chamadas;
        await enderecoService.Pesquisar("SP","São Paulo","Praça da Sé");
        Verificar(pesquisa.Count==1 && fornecedor.Chamadas==chamadasPesquisa,"Sprint 5: pesquisa por endereço e cache");
        bool curta=false;try{await enderecoService.Pesquisar("SP","SP","a");}catch(FalhaOperacao){curta=true;}
        Verificar(curta && fornecedor.Chamadas==chamadasPesquisa,"Sprint 5: pesquisa ampla recusada antes da integração");
        foreach(var (cep,tipo) in new[]{("99999999",TipoFalhaEndereco.NaoEncontrado),("22222222",TipoFalhaEndereco.Indisponivel),
            ("33333333",TipoFalhaEndereco.Timeout),("44444444",TipoFalhaEndereco.RespostaInvalida),("55555555",TipoFalhaEndereco.RespostaInvalida)}) {
            bool falhou=false;try{await enderecoService.ConsultarCEP(cep);}catch(FalhaEndereco e){falhou=e.Tipo==tipo;}
            Verificar(falhou,"Sprint 5: integração diferencia "+tipo);
        }
        int antesFalhaCache=fornecedor.Chamadas;
        try{await enderecoService.ConsultarCEP("22222222");}catch(FalhaEndereco){ }
        Verificar(fornecedor.Chamadas==antesFalhaCache+1,"Sprint 5: erro externo não fica em cache");
        var manual=await enderecoService.PrepararLocal(DadosEndereco());
        Verificar(manual.Estruturado && manual.Latitude==null && manual.Longitude==null && manual.EnderecoFormatado.Contains("Sala 2"),
            "Sprint 5: endereço manual funciona sem geocoder");
        var geocoderInvalido=new EnderecoService(provider,new GeocoderTeste(new(91,0)),cacheEndereco,Microsoft.Extensions.Logging.Abstractions.NullLogger<EnderecoService>.Instance);
        Verificar((await geocoderInvalido.PrepararLocal(DadosEndereco())).Latitude==null,"Sprint 5: coordenadas inválidas do provider são descartadas");
        var geocoderValido=new EnderecoService(provider,new GeocoderTeste(new(-23.550520m,-46.633308m)),cacheEndereco,Microsoft.Extensions.Logging.Abstractions.NullLogger<EnderecoService>.Instance);
        Verificar((await geocoderValido.PrepararLocal(DadosEndereco())).Longitude==-46.633308m,"Sprint 5: interface aceita coordenadas de provider substituível");
        var dadosInvalidos=DadosEndereco();dadosInvalidos.Numero="";Validacao5(()=>EnderecoService.Validar(dadosInvalidos),"número obrigatório no backend");
        dadosInvalidos=DadosEndereco();dadosInvalidos.Complemento=new string('a',61);Validacao5(()=>EnderecoService.Validar(dadosInvalidos),"limite de complemento");
        // Retorna a Development com fornecedor local controlado; nenhuma consulta externa real.
        app.Kill(true);app.WaitForExit();start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";
        start.Environment["Jwt__SigningKey"]=Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        start.Environment["Enderecos__ViaCepBaseUrl"]="http://127.0.0.1:5178/ws/";
        app=Process.Start(start)??throw new Exception("Servidor de testes não iniciou.");
        app.OutputDataReceived+=(_,e)=>{if(e.Data!=null)lock(logs)logs.AppendLine(e.Data);};
        app.ErrorDataReceived+=(_,e)=>{if(e.Data!=null)lock(logs)logs.AppendLine(e.Data);};app.BeginOutputReadLine();app.BeginErrorReadLine();
        bool pronto5=false;for(int i=0;i<40;i++) {
            if(app.HasExited)throw new Exception("Aplicação Sprint 5 não iniciou.");
            try{if((await anon.GetAsync("/Usuario/Login")).IsSuccessStatusCode){pronto5=true;break;}}catch(HttpRequestException){}await Task.Delay(250);
        }
        Verificar(pronto5,"Sprint 5: aplicação inicia com integração substituível");
        int cliente5=Usuario("api5","cliente");
        using var c5=Cliente();using var p5=Cliente();using var q5=Cliente();
        async Task<System.Text.Json.JsonElement> Req5(HttpClient client,HttpMethod method,string path,object? body,int status,string nome) {
            using var request=new HttpRequestMessage(method,path);
            if(body!=null)request.Content=System.Net.Http.Json.JsonContent.Create(body);
            using var response=await client.SendAsync(request);string texto=await response.Content.ReadAsStringAsync();
            Verificar((int)response.StatusCode==status,"Sprint 5: "+nome+" (HTTP "+(int)response.StatusCode+")");
            if(texto.Length==0)return default;
            using var doc=System.Text.Json.JsonDocument.Parse(texto);
            if(status>=400 && (!doc.RootElement.TryGetProperty("erro",out _) || texto.Contains("Exception") || texto.Contains("SQLEXPRESS")))throw new Exception("Detalhe interno no erro.");
            return doc.RootElement.Clone();
        }
        foreach(var (client,sufixo) in new[]{(c5,"api5"),(p5,"p"),(q5,"q")}) {
            var login=await Req5(client,HttpMethod.Post,"/api/v1/auth/login",new{email=tag+sufixo+"@example.invalid",senha},200,"login para endereços");
            client.DefaultRequestHeaders.Authorization=new("Bearer",login.GetProperty("accessToken").GetString());
        }
        await Req5(anon,HttpMethod.Get,"/api/v1/enderecos/cep/01001000",null,401,"consulta exige autenticação");
        var cep5=await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/cep/01001-000",null,200,"consulta CEP API");
        Verificar(cep5.EnumerateObject().Count()==5 && !cep5.TryGetProperty("ibge",out _) && !cep5.TryGetProperty("latitude",out _),"Sprint 5: resposta própria mínima e sem coordenadas privadas");
        int chamadas5=fornecedor.Chamadas;await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/cep/01001000",null,200,"cache da API");
        Verificar(fornecedor.Chamadas==chamadas5,"Sprint 5: API não repete fornecedor para mesmo CEP");
        await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/cep/abc",null,400,"CEP inválido API");
        await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/cep/99999999",null,404,"CEP inexistente API");
        await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/cep/22222222",null,503,"fornecedor indisponível API");
        await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/cep/33333333",null,504,"timeout API");
        await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/cep/44444444",null,502,"resposta inválida API");
        await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/pesquisar?uf=SP&cidade=Sao%20Paulo&logradouro=Paulista",null,200,"pesquisa API");
        await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/pesquisar?uf=XX&cidade=Sao&logradouro=Rua",null,400,"UF inválida API");
        await Req5(c5,HttpMethod.Get,"/api/v1/enderecos/pesquisar?uf=SP&cidade=Sao&logradouro="+new string('a',101),null,400,"pesquisa com tamanho abusivo");
        var localBody=new {nome=tag+"local5",dadosEndereco=DadosEndereco()};
        await Req5(c5,HttpMethod.Post,"/api/v1/locais",localBody,403,"cliente não cadastra local profissional");
        int local5=(await Req5(p5,HttpMethod.Post,"/api/v1/locais",localBody,201,"criar local estruturado")).GetProperty("id").GetInt32();
        var localConsulta=await Req5(p5,HttpMethod.Get,"/api/v1/locais/"+local5,null,200,"consultar local próprio");
        Verificar(localConsulta.GetProperty("dadosEndereco").GetProperty("cep").GetString()=="01001000" &&
            localConsulta.GetProperty("dadosEndereco").GetProperty("latitude").ValueKind==System.Text.Json.JsonValueKind.Null,"Sprint 5: banco guarda endereço sem coordenadas inventadas");
        await Req5(q5,HttpMethod.Get,"/api/v1/locais/"+local5,null,403,"outro profissional não acessa endereço privado");
        await Req5(q5,HttpMethod.Put,"/api/v1/locais/"+local5,localBody,403,"outro profissional não edita local");
        var edicao5=DadosEndereco();edicao5.Numero="20";
        await Req5(p5,HttpMethod.Put,"/api/v1/locais/"+local5,new{nome=tag+"local5",dadosEndereco=edicao5},204,"editar local estruturado");
        var payloadCoords=System.Text.Json.JsonSerializer.SerializeToNode(localBody)!;payloadCoords["dadosEndereco"]!["latitude"]=30;
        await Req5(p5,HttpMethod.Post,"/api/v1/locais",payloadCoords,400,"coordenada enviada pelo cliente recusada");
        int legado5=Id("INSERT INTO Locais(ProfissionalId,Nome,Endereco) OUTPUT INSERTED.Id VALUES(@P,'Legado','Texto original impossível de decompor')",("@P",prof));
        var legadoLido=new LocalDAO().BuscarPorId(legado5);
        Verificar(legadoLido?.DadosEndereco?.Estruturado==false && legadoLido.DadosEndereco.CEP==null && legadoLido.Endereco=="Texto original impossível de decompor","Sprint 5: local legado preserva texto e NULLs");
        await Req5(p5,HttpMethod.Put,"/api/v1/locais/"+legado5,localBody,204,"confirmação manual converte local legado livre");
        int domicilio5=Servico(prof,"dom5");Sql("UPDATE Servicos SET Atendimento='Domicilio' WHERE Id=@S",("@S",domicilio5));
        int presencial5=Servico(prof,"presencial5");Sql("UPDATE Servicos SET Atendimento='Local',LocalId=@L WHERE Id=@S",("@S",presencial5),("@L",local5));
        int online5=Servico(prof,"online5");var dia5=DateTime.Today.AddDays(16);
        object Reserva5(int servico,int hora,EnderecoRequest? endereco=null)=>new{servicoId=servico,data=dia5.ToString("yyyy-MM-dd"),hora=$"{hora:00}:00:00",endereco};
        await Req5(c5,HttpMethod.Post,"/api/v1/agendamentos",Reserva5(domicilio5,8),400,"domicílio exige endereço");
        int agDom5=(await Req5(c5,HttpMethod.Post,"/api/v1/agendamentos",Reserva5(domicilio5,8,DadosEndereco()),201,"reserva domiciliar estruturada")).GetProperty("id").GetInt32();
        int agLocal5=(await Req5(c5,HttpMethod.Post,"/api/v1/agendamentos",Reserva5(presencial5,9),201,"reserva presencial copia local")).GetProperty("id").GetInt32();
        int agOnline5=(await Req5(c5,HttpMethod.Post,"/api/v1/agendamentos",Reserva5(online5,10),201,"Online dispensa endereço")).GetProperty("id").GetInt32();
        Verificar(Id("SELECT COUNT(*) FROM AgendamentoEnderecos WHERE AgendamentoId IN(@D,@L)",("@D",agDom5),("@L",agLocal5))==2 &&
            Id("SELECT COUNT(*) FROM AgendamentoEnderecos WHERE AgendamentoId=@A",("@A",agOnline5))==0,"Sprint 5: snapshots somente nas modalidades físicas");
        await Req5(p5,HttpMethod.Put,"/api/v1/locais/"+local5,localBody,409,"histórico impede alteração do local");
        await Req5(p5,HttpMethod.Delete,"/api/v1/locais/"+local5,null,409,"histórico impede exclusão do local");
        await Req5(p5,HttpMethod.Delete,"/api/v1/locais/"+legado5,null,204,"remove local sem vínculo");
        var domResposta=await Req5(c5,HttpMethod.Get,"/api/v1/agendamentos/"+agDom5,null,200,"snapshot acessível ao contratante");
        Verificar(domResposta.GetProperty("enderecoAtendimento").GetProperty("numero").GetString()=="10" &&
            !domResposta.GetProperty("enderecoAtendimento").TryGetProperty("latitude",out _),"Sprint 5: DTO de participante omite coordenadas");
        await Req5(q5,HttpMethod.Get,"/api/v1/agendamentos/"+agDom5,null,403,"terceiro não acessa domicílio futuro");
        var enderecoAlterado=DadosEndereco();enderecoAlterado.Numero="999";
        int agOutroDom=(await Req5(c5,HttpMethod.Post,"/api/v1/agendamentos",Reserva5(domicilio5,11,enderecoAlterado),201,"outro domicílio em nova contratação")).GetProperty("id").GetInt32();
        Verificar(Convert.ToString(Sql("SELECT Numero FROM AgendamentoEnderecos WHERE AgendamentoId=@A",("@A",agDom5)))=="10" &&
            Convert.ToString(Sql("SELECT Numero FROM AgendamentoEnderecos WHERE AgendamentoId=@A",("@A",agOutroDom)))=="999","Sprint 5: novo endereço não altera snapshot antigo");
        int antesReserva5=TotalReservas(),antesSnap5=Id("SELECT COUNT(*) FROM AgendamentoEnderecos"),antesNotif5=Id("SELECT COUNT(*) FROM Notificacoes WHERE UsuarioId=@U",("@U",cliente5));
        bool rollback5=false;
        try { new AgendamentoDAO().Inserir(new(){ClienteId=new ClienteDAO().BuscarClienteIdPorUsuario(cliente5),ServicoId=domicilio5,ProfissionalId=prof,
            Data=dia5,Hora=TimeSpan.FromHours(12),EnderecoCliente=manual.EnderecoFormatado,Descricao="Rollback",
            EnderecoAtendimento=manual with{Latitude=91,Longitude=0}}); }
        catch(SqlException){rollback5=true;}
        Verificar(rollback5 && TotalReservas()==antesReserva5 && Id("SELECT COUNT(*) FROM AgendamentoEnderecos")==antesSnap5 &&
            Id("SELECT COUNT(*) FROM Notificacoes WHERE UsuarioId=@U",("@U",cliente5))==antesNotif5,"Sprint 5: falha no snapshot reverte reserva e notificações");
        bool limiteBanco5=false;
        try{Sql("UPDATE Locais SET Latitude=0,Longitude=181 WHERE Id=@L",("@L",local5));}catch(SqlException){limiteBanco5=true;}
        Verificar(limiteBanco5 && new LocalDAO().BuscarPorId(local5)?.DadosEndereco?.Longitude==null,"Sprint 5: constraint SQL protege coordenadas");
        using var web5=Cliente();await Post(web5,"/Usuario/Logar",new(){["email"]=tag+"p@example.invalid",["senha"]=senha});
        Verificar((await web5.GetStringAsync("/Local/Lista")).Contains("data-consultar"),"Sprint 5: formulário de local possui consulta acessível");
        Verificar((await web5.GetAsync("/Endereco/Cep?cep=01001000")).IsSuccessStatusCode,"Sprint 5: MVC consulta pelo mesmo Service com sessão");
        var form5=new Dictionary<string,string>{{"nome",tag+"web5"},{"DadosEndereco.CEP","01001000"},{"DadosEndereco.Logradouro","Praça da Sé"},{"DadosEndereco.Numero","30"},
            {"DadosEndereco.Bairro","Sé"},{"DadosEndereco.Cidade","São Paulo"},{"DadosEndereco.UF","SP"}};
        Verificar((await Post(web5,"/Local/Salvar",form5)).StatusCode==HttpStatusCode.Redirect &&
            Id("SELECT COUNT(*) FROM Locais WHERE Nome=@N AND EnderecoEstruturado=1",("@N",tag+"web5"))==1,"Sprint 5: MVC salva endereço estruturado");
        Verificar((await web5.GetStringAsync($"/Agendamento/Novo?servicoId={domicilio5}")).Contains("Endereco.CEP") &&
            !(await web5.GetStringAsync($"/Agendamento/Novo?servicoId={online5}")).Contains("Endereco.CEP"),"Sprint 5: formulário físico só aparece em Domicilio");
        using var open5=System.Text.Json.JsonDocument.Parse(await anon.GetStringAsync("/openapi/v1.json"));
        Verificar(open5.RootElement.GetProperty("paths").TryGetProperty("/api/v1/enderecos/cep/{cep}",out _) &&
            open5.RootElement.GetProperty("paths").TryGetProperty("/api/v1/locais",out _),"Sprint 5: OpenAPI inclui endereços e locais");
        bool limite5=false;for(int i=0;i<31;i++) {
            using var resp=await c5.GetAsync("/api/v1/enderecos/cep/01001000");
            if((int)resp.StatusCode==429){limite5=resp.Headers.RetryAfter!=null;break;}
        }
        Verificar(limite5,"Sprint 5: rate limiting protege consulta externa mesmo com cache");
        Console.WriteLine("SPRINT 5: "+(passou-inicioSprint5)+" verificações novas.");
    }

    Console.WriteLine("TOTAL: "+passou+" verificações passaram.");
}
finally
{
    if(app is { HasExited:false }) { app.Kill(true); app.WaitForExit(); }
    // Somente IDs criados por esta execução, em ordem de dependência.
    if(usuarios.Count>0)
    {
        var ids=string.Join(",",usuarios);
        var ps=profissionais.Count>0?string.Join(",",profissionais):"0";
        var ss=servicos.Count>0?string.Join(",",servicos):"0";
        Sql("DELETE FROM Avaliacoes WHERE UsuarioId IN ("+ids+")");
        Sql("DELETE FROM Notificacoes WHERE UsuarioId IN ("+ids+")");
        Sql("DELETE FROM Agendamentos WHERE ServicoId IN ("+ss+")");
        Sql("DELETE FROM Disponibilidade WHERE ProfissionalId IN ("+ps+")");
        Sql("DELETE FROM Servicos WHERE ProfissionalId IN ("+ps+")");
        Sql("DELETE FROM BloqueiosAgenda WHERE ProfissionalId IN ("+ps+")");
        Sql("DELETE FROM Locais WHERE ProfissionalId IN ("+ps+")");
        Sql("DELETE FROM Assinaturas WHERE ProfissionalId IN ("+ps+")");
        Sql("DELETE FROM ContatoSuporte WHERE UsuarioId IN ("+ids+")");
        Sql("DELETE FROM Usuarios WHERE Id IN ("+ids+")");
        Console.WriteLine("Registros temporários removidos: "+tag);
    }
}