using Microsoft.Data.SqlClient;

namespace BD_TRAMPO
{
    public class Conexao
    {
        private string stringConexao =
            "Server=(localdb)\\MSSQLLocalDB;Database=Xamou;Trusted_Connection=True;";

        public SqlConnection Conectar()
        {
            try
            {
                SqlConnection conn =
                    new SqlConnection(stringConexao);

                conn.Open();

                return conn;
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    "Erro ao conectar com o banco de dados.\n" +
                    "Verifique se o SQL Server LocalDB está iniciado.\n\n" +
                    $"Detalhes: {ex.Message}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Erro inesperado ao abrir conexão com o banco.\n\n" +
                    $"Detalhes: {ex.Message}"
                );
            }
        }
    }
}