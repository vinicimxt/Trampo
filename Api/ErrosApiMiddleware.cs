using BD_TRAMPO.Contracts;
using BD_TRAMPO.Api.Contracts;

namespace BD_TRAMPO.Api;

public sealed class ErrosApiMiddleware(RequestDelegate proximo, ILogger<ErrosApiMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        if (!contexto.Request.Path.StartsWithSegments("/api")) { await proximo(contexto); return; }
        contexto.Response.Headers.CacheControl = "no-store";
        try
        {
            await proximo(contexto);
            if (!contexto.Response.HasStarted && contexto.Response.StatusCode >= 400 &&
                contexto.Response.ContentType == null)
                await Escrever(contexto, contexto.Response.StatusCode);
        }
        catch (FalhaOperacao ex) when (!contexto.Response.HasStarted)
        {
            int status = ex.Tipo switch {
                TipoFalha.Validacao => 400, TipoFalha.NaoAutenticado => 401,
                TipoFalha.SemPermissao => 403, TipoFalha.NaoEncontrado => 404, _ => 409 };
            await Escrever(contexto, status, ex.Message);
        }
        catch (ApiIndisponivelException) when (!contexto.Response.HasStarted)
        { await Escrever(contexto, 503); }
        catch (BadHttpRequestException) when (!contexto.Response.HasStarted)
        { await Escrever(contexto, 400); }
        catch (Exception ex) when (!contexto.Response.HasStarted)
        {
            logger.LogError(ex, "Falha técnica na API. TraceId: {TraceId}", contexto.TraceIdentifier);
            await Escrever(contexto, 500);
        }
    }

    public static ErroResponse Resposta(int status, string? mensagem = null)
    {
        var (codigo, texto) = status switch {
            400 => ("VALIDACAO", "Dados da requisição inválidos."),
            401 => ("NAO_AUTENTICADO", "Autenticação necessária ou inválida."),
            403 => ("SEM_PERMISSAO", "Você não tem permissão para esta operação."),
            404 => ("NAO_ENCONTRADO", "Recurso não encontrado."),
            405 => ("METODO_NAO_PERMITIDO", "Método não permitido."),
            409 => ("CONFLITO", "A operação conflita com o estado atual."),
            415 => ("FORMATO_INVALIDO", "Envie o conteúdo como application/json."),
            429 => ("LIMITE_REQUISICOES", "Muitas tentativas. Tente novamente mais tarde."),
            503 => ("API_INDISPONIVEL", "Autenticação da API indisponível."),
            _ => ("ERRO_INTERNO", "Não foi possível concluir a operação.") };
        return new(new(codigo, mensagem ?? texto));
    }
    public static async Task Escrever(HttpContext contexto, int status, string? mensagem = null)
    {
        contexto.Response.StatusCode = status;
        await contexto.Response.WriteAsJsonAsync(Resposta(status, mensagem));
    }
}
