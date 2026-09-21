using BD_TRAMPO.Contracts;
using BD_TRAMPO.Integrations.Enderecos;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace BD_TRAMPO.Services;

public sealed class EnderecoService(IConsultaEnderecoProvider consulta, IGeocodificacaoProvider geocoder,
    CacheEnderecos cache, ILogger<EnderecoService> logger)
{
    private static readonly HashSet<string> Ufs = new("AC AL AP AM BA CE DF ES GO MA MT MS MG PA PB PR PE PI RJ RN RS RO RR SC SP SE TO".Split(' '));
    public static string NormalizarCEP(string? cep)
    {
        string valor=cep?.Trim() ?? "";
        if(!Regex.IsMatch(valor,@"^[0-9]{5}-?[0-9]{3}$"))
            throw new FalhaOperacao(TipoFalha.Validacao,"Informe um CEP com oito dígitos.");
        return valor.Replace("-","");
    }
    public static string NormalizarUF(string? uf)
    {
        string valor=uf?.Trim().ToUpperInvariant() ?? "";
        if(!Ufs.Contains(valor)) throw new FalhaOperacao(TipoFalha.Validacao,"UF inválida.");
        return valor;
    }
    private static string Campo(string? valor,int max,string nome,int minimo=1)
    {
        string texto=valor?.Trim() ?? "";
        if(texto.Length<minimo || texto.Length>max || texto.Any(char.IsControl))
            throw new FalhaOperacao(TipoFalha.Validacao,$"Confira o campo {nome}.");
        return texto;
    }
    public static EnderecoDados Validar(EnderecoRequest dados)
    {
        string cep=NormalizarCEP(dados.CEP),uf=NormalizarUF(dados.UF);
        string rua=Campo(dados.Logradouro,100,"logradouro"),numero=Campo(dados.Numero,20,"número"),
            bairro=Campo(dados.Bairro,80,"bairro"),cidade=Campo(dados.Cidade,80,"cidade"),
            complemento=Campo(dados.Complemento,60,"complemento",0);
        string texto=$"{rua}, {numero}{(complemento.Length>0 ? " - "+complemento : "")} - {bairro}, {cidade}/{uf} - CEP {cep}";
        if(texto.Length>255) throw new FalhaOperacao(TipoFalha.Validacao,"Endereço completo deve ter até 255 caracteres.");
        return new(cep,rua,numero,complemento.Length==0?null:complemento,bairro,cidade,uf,null,null,texto,true);
    }
    public async Task<EnderecoDados> PrepararLocal(EnderecoRequest dados,CancellationToken ct=default)
    {
        var endereco=Validar(dados);
        try {
            var coordenadas=await geocoder.Localizar(endereco,ct).WaitAsync(TimeSpan.FromSeconds(4),ct);
            if(coordenadas==null) return endereco;
            if(coordenadas.Latitude is < -90 or > 90 || coordenadas.Longitude is < -180 or > 180)
                throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida);
            return endereco with {Latitude=decimal.Round(coordenadas.Latitude,6),Longitude=decimal.Round(coordenadas.Longitude,6)};
        }
        catch(TimeoutException) {logger.LogWarning("Geocodificação expirou; local ficará sem coordenadas.");return endereco;}
        catch(OperationCanceledException) when (!ct.IsCancellationRequested) {
            logger.LogWarning("Geocodificação expirou; local ficará sem coordenadas."); return endereco;
        }
        catch(Exception ex) when (ex is FalhaEndereco or HttpRequestException) {
            logger.LogWarning("Geocodificação indisponível ou inválida; local ficará sem coordenadas."); return endereco;
        }
    }
    private async Task<T> Integrar<T>(Func<Task<T>> executar)
    {
        try{return await executar();}
        catch(FalhaEndereco ex){logger.LogWarning("Falha de consulta de endereço: {Tipo}",ex.Tipo);throw;}
    }
    public async Task<EnderecoConsulta> ConsultarCEP(string cep,CancellationToken ct=default)
    {
        string normal=NormalizarCEP(cep),key="cep:"+normal;
        if(cache.TryGetValue<EnderecoConsulta>(key,out var salvo) && salvo!=null) return salvo;
        var resultado=await Integrar(()=>consulta.ConsultarCEP(normal,ct)) ?? throw new FalhaEndereco(TipoFalhaEndereco.NaoEncontrado);
        cache.Set(key,resultado,new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow=TimeSpan.FromHours(24),Size=1});
        return resultado;
    }
    public async Task<IReadOnlyList<EnderecoConsulta>> Pesquisar(string uf,string cidade,string logradouro,CancellationToken ct=default)
    {
        uf=NormalizarUF(uf);cidade=Campo(cidade,80,"cidade",3);logradouro=Campo(logradouro,100,"logradouro",3);
        string key=$"pesquisa:{uf}:{cidade.ToUpperInvariant()}:{logradouro.ToUpperInvariant()}";
        if(cache.TryGetValue<IReadOnlyList<EnderecoConsulta>>(key,out var salvo) && salvo!=null) return salvo;
        var resultado=await Integrar(()=>consulta.Pesquisar(uf,cidade,logradouro,ct));
        cache.Set(key,resultado,new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow=TimeSpan.FromMinutes(15),Size=1});
        return resultado;
    }
}
