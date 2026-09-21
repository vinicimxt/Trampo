using System.Net;
using System.Text;
using BD_TRAMPO.Contracts;
using BD_TRAMPO.Integrations.Enderecos;

internal sealed class EnderecoFakeServer : IDisposable
{
    private readonly HttpListener listener=new();
    private readonly CancellationTokenSource cancel=new();
    private readonly Task loop;
    private int chamadas;
    public int Chamadas=>Volatile.Read(ref chamadas);
    public EnderecoFakeServer()
    {
        listener.Prefixes.Add("http://127.0.0.1:5178/");listener.Start();
        loop=Task.Run(async()=>{
            while(!cancel.IsCancellationRequested) {
                try {var c=await listener.GetContextAsync();_ = Responder(c);}
                catch(Exception e) when(e is HttpListenerException or ObjectDisposedException){break;}
            }
        });
    }
    private async Task Responder(HttpListenerContext c)
    {
        Interlocked.Increment(ref chamadas);
        try {
            string path=c.Request.Url?.AbsolutePath??"";
            string json="{\"cep\":\"01001-000\",\"logradouro\":\"Praça da Sé\",\"bairro\":\"Sé\",\"localidade\":\"São Paulo\",\"uf\":\"SP\"}";
            if(path.Contains("99999999"))json="{\"erro\":true}";
            else if(path.Contains("22222222")){c.Response.StatusCode=503;json="{}";}
            else if(path.Contains("33333333")){await Task.Delay(6000,cancel.Token);}
            else if(path.Contains("44444444"))json="json inválido";
            else if(path.Contains("55555555"))json="{\"cep\":123}";
            else if(path.Contains("/SP/"))json="["+json+"]";
            byte[] bytes=Encoding.UTF8.GetBytes(json);c.Response.ContentType="application/json";
            await c.Response.OutputStream.WriteAsync(bytes,cancel.Token);
        }catch(Exception e) when(e is OperationCanceledException or HttpListenerException or IOException or ObjectDisposedException){ }
        finally{c.Response.Close();}
    }
    public void Dispose(){cancel.Cancel();listener.Close();loop.GetAwaiter().GetResult();cancel.Dispose();}
}
internal sealed class GeocoderTeste(Coordenadas? resultado):IGeocodificacaoProvider
{
    public Task<Coordenadas?> Localizar(EnderecoDados endereco,CancellationToken ct)=>Task.FromResult(resultado);
}
