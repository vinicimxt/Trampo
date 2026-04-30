using Microsoft.Data.SqlClient;

namespace BD_TRAMPO
{

    public class AvaliacaoDAO
    {
        private Conexao conexao = new Conexao();

        public void Inserir(Avaliacao a)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"INSERT INTO Avaliacoes 
                (AgendamentoId, ProfissionalId, UsuarioId, Nota, Comentario, Data)
                VALUES (@AgendamentoId, @ProfissionalId, @UsuarioId, @Nota, @Comentario, GETDATE())";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@AgendamentoId", a.AgendamentoId);
                cmd.Parameters.AddWithValue("@ProfissionalId", a.ProfissionalId);
                cmd.Parameters.AddWithValue("@Nota", a.Nota);
                cmd.Parameters.AddWithValue("@Comentario", a.Comentario ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("UsuarioId", a.UsuarioId);

                cmd.ExecuteNonQuery();
            }
        }

        public List<Avaliacao> ListarPorProfissional(int profissionalId)
        {
            List<Avaliacao> lista = new List<Avaliacao>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                SELECT a.*, u.Nome
                FROM Avaliacoes a
                INNER JOIN Usuarios u ON a.UsuarioId = u.Id
                WHERE a.ProfissionalId = @id
                ORDER BY a.Data DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", profissionalId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Avaliacao
                    {
                        Id = (int)reader["Id"],
                        Nota = (int)reader["Nota"],
                        Comentario = reader["Comentario"]?.ToString(),
                        NomeUsuario = reader["Nome"].ToString(),
                        Data = (DateTime)reader["Data"]
                    });
                }
            }

            return lista;
        }

        public double MediaPorProfissional(int profissionalId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT AVG(CAST(Nota AS FLOAT)) FROM Avaliacoes WHERE ProfissionalId = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", profissionalId);

                object result = cmd.ExecuteScalar();

                return result != DBNull.Value ? Convert.ToDouble(result) : 0;
            }
        }

        public bool JaAvaliou(int agendamentoId, int usuarioId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                SELECT COUNT(*) 
                FROM Avaliacoes 
                WHERE AgendamentoId = @agendamentoId
                AND UsuarioId = @usuarioId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@agendamentoId", agendamentoId);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                return (int)cmd.ExecuteScalar() > 0;
            }
        }
    }
}