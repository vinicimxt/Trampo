using Microsoft.Data.SqlClient;
namespace BD_TRAMPO
{
    public class Conexao
    {
        private string stringConexao = "Server=(localdb)\\MSSQLLocalDB;Database=Xamou;Trusted_Connection=True;";

        public SqlConnection Conectar()
        {
            SqlConnection conn = new SqlConnection(stringConexao);
            conn.Open();
            return conn;
        }
    }
}