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
    int a=Usuario("a","cliente",true), b=Usuario("b","cliente"), p=Usuario("p","profissional"), q=Usuario("q","profissional"), admin=Usuario("admin","admin");
    int prof=Profissional(p), outroProf=Profissional(q);
    int s=Servico(prof,"servico"), s2=Servico(prof,"outro"), inativo=Servico(prof,"inativo",false), outro=Servico(outroProf,"terceiro");
    int sub=Id("SELECT TOP 1 Id FROM Subcategorias");
    var start = new ProcessStartInfo("dotnet", "\""+(Environment.GetEnvironmentVariable("TRAMPO_TEST_DLL") ?? Path.Combine(root,"bin/Debug/net10.0/BD-TRAMPO.dll"))+"\" --urls http://127.0.0.1:5177") {
        WorkingDirectory=root, UseShellExecute=false, CreateNoWindow=true, RedirectStandardOutput=true, RedirectStandardError=true };
    start.Environment["ASPNETCORE_ENVIRONMENT"]="Development";
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
    if (args.Contains("--sprint2"))
    {
        Console.WriteLine("SPRINT 1: " + passou + " verificações preservadas.");
        Verificar(Id("SELECT COUNT(*) FROM Disponibilidade WHERE ServicoId=@S",("@S",criadoId))==7,
            "Sprint 2: serviço criado/editado conserva todas as regras");
        var daoServico = new ServicoDAO();
        var edicao = daoServico.BuscarPorId(criadoId);
        var nomeAntes = edicao.Nome;
        edicao.SubcategoriaId = -1;
        bool falhou = false;
        try { daoServico.SalvarComDisponibilidade(edicao, new[]{1,2}, TimeSpan.FromHours(9), TimeSpan.FromHours(12)); }
        catch (SqlException) { falhou = true; }
        Verificar(falhou && daoServico.BuscarPorId(criadoId).Nome == nomeAntes &&
            Id("SELECT COUNT(*) FROM Disponibilidade WHERE ServicoId=@S",("@S",criadoId))==7,
            "Sprint 2: falha SQL preserva serviço e regras anteriores");
        var novoInvalido = daoServico.BuscarPorId(criadoId);
        novoInvalido.Id = 0; novoInvalido.SubcategoriaId = -1; novoInvalido.Nome = tag+"invalido";
        falhou=false;
        try { daoServico.SalvarComDisponibilidade(novoInvalido,new[]{1},TimeSpan.FromHours(9),TimeSpan.FromHours(12)); }
        catch(SqlException) { falhou=true; }
        Verificar(falhou && Id("SELECT COUNT(*) FROM Servicos WHERE Nome=@N",("@N",novoInvalido.Nome))==0,
            "Sprint 2: criação inválida não deixa serviço parcial");
        edicao = daoServico.BuscarPorId(criadoId);
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
        Verificar(!daoServico.BuscarPorId(s).Ativo && TotalReservas()==historicoAntes &&
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