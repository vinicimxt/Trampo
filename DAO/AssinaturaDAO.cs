using Microsoft.Data.SqlClient;
using BD_TRAMPO.Models;

namespace BD_TRAMPO.DAO
{
    public class AssinaturaDAO
    {
        private Conexao conexao = new Conexao();


        public void Inserir(
    int profissionalId,
    string plano,
    decimal valor,
    string statusPagamento,
    string metodo)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        INSERT INTO Assinaturas
        (
            ProfissionalId,
            Plano,
            Valor,
            StatusPagamento,
            MetodoPagamento
        )
        VALUES
        (
            @ProfissionalId,
            @Plano,
            @Valor,
            @StatusPagamento,
            @MetodoPagamento
        )";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);
                cmd.Parameters.AddWithValue("@Plano", plano);
                cmd.Parameters.AddWithValue("@Valor", valor);
                cmd.Parameters.AddWithValue("@StatusPagamento", statusPagamento);
                cmd.Parameters.AddWithValue("@MetodoPagamento", metodo);

                cmd.ExecuteNonQuery();
            }
        }

    }




}