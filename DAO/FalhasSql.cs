using BD_TRAMPO.Contracts;
using Microsoft.Data.SqlClient;

namespace BD_TRAMPO.DAO;

public static class FalhasSql
{
    // Números exclusivos para revalidações conhecidas. Não examina o texto da exceção.
    public static T Executar<T>(Func<T> operacao)
    {
        try { return operacao(); }
        catch (SqlException ex) when (ex.Number is 51001 or 51002 or 51003)
        {
            throw ex.Number switch {
                51001 => new FalhaOperacao(TipoFalha.Conflito, "Agenda em atualização. Tente novamente."),
                51002 => new FalhaOperacao(TipoFalha.NaoEncontrado, "Serviço não encontrado."),
                _ => new FalhaOperacao(TipoFalha.Validacao, "O local deve pertencer ao profissional.")
            };
        }
    }
}
