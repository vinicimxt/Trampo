using BD_TRAMPO.Contracts;
using Microsoft.Data.SqlClient;
namespace BD_TRAMPO.DAO;

public static class EnderecoSql
{
    public static void Parametros(SqlCommand cmd, EnderecoDados? d)
    {
        void P(string n,object? v) => cmd.Parameters.AddWithValue(n,v??DBNull.Value);
        P("@CEP",d?.CEP); P("@Logradouro",d?.Logradouro);P("@Numero",d?.Numero);P("@Complemento",d?.Complemento);
        P("@Bairro",d?.Bairro);P("@Cidade",d?.Cidade);P("@UF",d?.UF);P("@Latitude",d?.Latitude);P("@Longitude",d?.Longitude);
        P("@Estruturado",d?.Estruturado??false);
    }
    public static EnderecoDados Ler(SqlDataReader r,string texto)
    {
        string? S(string nome) {int p=r.GetOrdinal(nome);return r.IsDBNull(p)?null:r.GetString(p);}
        decimal? D(string nome) {int p=r.GetOrdinal(nome);return r.IsDBNull(p)?null:r.GetDecimal(p);}
        return new(S("CEP"),S("Logradouro"),S("Numero"),S("Complemento"),S("Bairro"),S("Cidade"),S("UF"),
            D("Latitude"),D("Longitude"),texto,r.GetBoolean(r.GetOrdinal("EnderecoEstruturado")));
    }
}
