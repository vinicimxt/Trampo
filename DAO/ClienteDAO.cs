using Microsoft.Data.SqlClient;

namespace BD_TRAMPO
{
    public class ClienteDAO
    {
        Conexao conexao = new Conexao();

        public int ObterOuCriar(int usuarioId)
        {
            using var conn = conexao.Conectar();
            using var tx = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            using var cmd = new SqlCommand(@"
                DECLARE @Id int;
                SELECT @Id=Id FROM Clientes WITH (UPDLOCK,HOLDLOCK) WHERE UsuarioId=@U;
                IF @Id IS NULL BEGIN
                    INSERT INTO Clientes(UsuarioId) VALUES(@U);
                    SET @Id=CONVERT(int,SCOPE_IDENTITY());
                END;
                SELECT @Id;", conn, tx);
            cmd.Parameters.AddWithValue("@U", usuarioId);
            int id = Convert.ToInt32(cmd.ExecuteScalar());
            tx.Commit();
            return id;
        }
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