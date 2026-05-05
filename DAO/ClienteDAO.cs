using Microsoft.Data.SqlClient;

namespace BD_TRAMPO
{
    public class ClienteDAO
    {
        Conexao conexao = new Conexao();

        public void Inserir(int usuarioId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "INSERT INTO Clientes (UsuarioId) VALUES (@UsuarioId)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                cmd.ExecuteNonQuery();
            }
        }
        public int BuscarClienteIdPorUsuario(int usuarioId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT Id FROM Clientes WHERE UsuarioId = @UsuarioId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                object result = cmd.ExecuteScalar();

                if (result != null)
                    return (int)result;

                return 0;
            }
        }

        public DateTime? BuscarBloqueio(int clienteId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT BloqueadoAte FROM Clientes WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", clienteId);

                var result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return null;

                return (DateTime)result;
            }
        }


        public void BloquearCliente(int clienteId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                UPDATE Clientes 
                SET BloqueadoAte = DATEADD(HOUR, 24, GETDATE())
                WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", clienteId);

                cmd.ExecuteNonQuery();
            }
        }


        // SISTEMA DE NOTIFICAÇÕES 

        public int BuscarUsuarioId(int clienteId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT UsuarioId FROM Clientes WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", clienteId);

                object result = cmd.ExecuteScalar();

                return result != null ? (int)result : 0;
            }
        }



    }
}