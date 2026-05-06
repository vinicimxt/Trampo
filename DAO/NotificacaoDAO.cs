using Microsoft.Data.SqlClient;

namespace BD_TRAMPO.DAO
{

    public class NotificacaoDAO
    {

        Conexao conexao = new Conexao();

        public void Inserir(Notificacao n)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                INSERT INTO Notificacoes
                (UsuarioId, Titulo, Mensagem, Tipo, ReferenciaId, Lida, DataCriacao)
                VALUES
                (@UsuarioId, @Titulo, @Mensagem, @Tipo, @ReferenciaId, 0, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@UsuarioId", n.UsuarioId);
                cmd.Parameters.AddWithValue("@Titulo", n.Titulo);
                cmd.Parameters.AddWithValue("@Mensagem", n.Mensagem);
                cmd.Parameters.AddWithValue("@Tipo", n.Tipo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ReferenciaId", n.ReferenciaId ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public List<Notificacao> ListarPorUsuario(int usuarioId)
        {
            List<Notificacao> lista = new List<Notificacao>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT Id, Titulo, Mensagem, Tipo, ReferenciaId, Lida, DataCriacao
        FROM Notificacoes
        WHERE UsuarioId = @UsuarioId
        ORDER BY DataCriacao DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Notificacao
                    {
                        Id = (int)reader["Id"],
                        Titulo = reader["Titulo"].ToString(),
                        Mensagem = reader["Mensagem"].ToString(),
                        Tipo = reader["Tipo"].ToString(),
                        ReferenciaId = reader["ReferenciaId"] != DBNull.Value ? (int?)reader["ReferenciaId"] : null,
                        Lida = (bool)reader["Lida"],
                        DataCriacao = (DateTime)reader["DataCriacao"]
                    });
                }
            }

            return lista;
        }

        public int ContarNaoLidas(int usuarioId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT COUNT(*) FROM Notificacoes
        WHERE UsuarioId = @UsuarioId AND Lida = 0";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                return (int)cmd.ExecuteScalar();
            }
        }

        public void MarcarComoLida(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "UPDATE Notificacoes SET Lida = 1 WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }


        public List<Notificacao> BuscarUltimas(int usuarioId, int quantidade)
        {
            List<Notificacao> lista = new List<Notificacao>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            SELECT TOP (@Qtd) *
            FROM Notificacoes
            WHERE UsuarioId = @UsuarioId
            ORDER BY DataCriacao DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@Qtd", quantidade);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Notificacao
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Titulo = reader["Titulo"].ToString(),
                        Mensagem = reader["Mensagem"].ToString(),
                        Lida = Convert.ToBoolean(reader["Lida"]),
                        DataCriacao = Convert.ToDateTime(reader["DataCriacao"]),
                        ReferenciaId = reader["ReferenciaId"] != DBNull.Value
                            ? Convert.ToInt32(reader["ReferenciaId"])
                            : (int?)null
                    });
                }
            }

            return lista;
        }




    }

}
