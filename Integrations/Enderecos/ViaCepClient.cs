using System.Text.Json;
using BD_TRAMPO.Contracts;
using BD_TRAMPO.Services;

namespace BD_TRAMPO.Integrations.Enderecos;

public sealed class ViaCepClient(HttpClient http, ILogger<ViaCepClient> logger) : IConsultaEnderecoProvider
{
    private async Task<JsonDocument> Ler(string caminho, CancellationToken ct)
    {
        try {
            using var resposta = await http.GetAsync(caminho, ct);
            if (!resposta.IsSuccessStatusCode) throw new FalhaEndereco(TipoFalhaEndereco.Indisponivel);
            var bytes = await resposta.Content.ReadAsByteArrayAsync(ct);
            if (bytes.Length > 131072) throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida);
            return JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth=16 });
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) {
            logger.LogWarning("Timeout na consulta de endereço.");
            throw new FalhaEndereco(TipoFalhaEndereco.Timeout);
        }
        catch (HttpRequestException) {
            logger.LogWarning("Fornecedor de endereço indisponível.");
            throw new FalhaEndereco(TipoFalhaEndereco.Indisponivel);
        }
        catch (JsonException) {
            logger.LogWarning("Resposta inválida do fornecedor de endereço.");
            throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida);
        }
    }
    private static EnderecoConsulta Converter(JsonElement item)
    {
        try {
            string Campo(string nome) => item.TryGetProperty(nome, out var valor) && valor.ValueKind == JsonValueKind.String
                ? valor.GetString() ?? "" : throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida);
            string cep=EnderecoService.NormalizarCEP(Campo("cep"));
            string uf=EnderecoService.NormalizarUF(Campo("uf"));
            string cidade=Campo("localidade"), logradouro=Campo("logradouro"), bairro=Campo("bairro");
            if(cidade.Length is < 1 or > 80 || logradouro.Length>100 || bairro.Length>80)
                throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida);
            return new(cep,logradouro,bairro,cidade,uf);
        } catch (FalhaOperacao) { throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida); }
        catch (InvalidOperationException) { throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida); }
    }
    public async Task<EnderecoConsulta?> ConsultarCEP(string cep, CancellationToken ct)
    {
        using var doc=await Ler($"{Uri.EscapeDataString(cep)}/json/",ct);
        var raiz=doc.RootElement;
        if(raiz.ValueKind!=JsonValueKind.Object) throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida);
        if(raiz.TryGetProperty("erro",out var erro) && (erro.ValueKind==JsonValueKind.True ||
            (erro.ValueKind==JsonValueKind.String && erro.GetString()=="true"))) return null;
        var resultado=Converter(raiz);
        if(resultado.CEP!=cep) throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida);
        return resultado;
    }
    public async Task<IReadOnlyList<EnderecoConsulta>> Pesquisar(string uf,string cidade,string logradouro,CancellationToken ct)
    {
        using var doc=await Ler($"{Uri.EscapeDataString(uf)}/{Uri.EscapeDataString(cidade)}/{Uri.EscapeDataString(logradouro)}/json/",ct);
        if(doc.RootElement.ValueKind!=JsonValueKind.Array) throw new FalhaEndereco(TipoFalhaEndereco.RespostaInvalida);
        return doc.RootElement.EnumerateArray().Take(20).Select(Converter).ToArray();
    }
}
