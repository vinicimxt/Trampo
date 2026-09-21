using BD_TRAMPO.Contracts;
using BD_TRAMPO.Api.Contracts;
namespace BD_TRAMPO.Api;
public static class FalhasEnderecoHttp
{
    public static (int Status,ErroResponse Resposta) Resposta(FalhaEndereco ex)
    {
        var (status,codigo,mensagem)=ex.Tipo switch {
            TipoFalhaEndereco.NaoEncontrado=>(404,"CEP_NAO_ENCONTRADO","CEP não encontrado. Confira ou preencha manualmente."),
            TipoFalhaEndereco.Timeout=>(504,"ENDERECO_TIMEOUT","A consulta demorou demais. Preencha manualmente ou tente novamente."),
            TipoFalhaEndereco.RespostaInvalida=>(502,"ENDERECO_RESPOSTA_INVALIDA","Não foi possível consultar o endereço. Preencha manualmente."),
            _ => (503,"ENDERECO_INDISPONIVEL","Consulta indisponível. Preencha manualmente ou tente novamente.")};
        return(status,new(new(codigo,mensagem)));
    }
}
