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

        public List<Avaliacao> ListarPorProfissional(int profissionalId, int? servicoId = null)
        {
            List<Avaliacao> lista = new List<Avaliacao>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                SELECT a.*, u.Nome, ag.ServicoId, s.Nome AS ServicoNome
                FROM Avaliacoes a
                INNER JOIN Usuarios u ON a.UsuarioId = u.Id
                LEFT JOIN Agendamentos ag ON ag.Id = a.AgendamentoId
                LEFT JOIN Servicos s ON s.Id = ag.ServicoId
                WHERE a.ProfissionalId = @id AND (@servicoId IS NULL OR ag.ServicoId = @servicoId)
                ORDER BY a.Data DESC, a.Id DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", profissionalId);
                cmd.Parameters.AddWithValue("@servicoId", servicoId.HasValue ? servicoId.Value : DBNull.Value);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Avaliacao
                    {
                        Id = (int)reader["Id"],
                        AgendamentoId = (int)reader["AgendamentoId"],
                        ProfissionalId = (int)reader["ProfissionalId"],
                        UsuarioId = (int)reader["UsuarioId"],
                        ServicoId = reader["ServicoId"] == DBNull.Value ? null : (int)reader["ServicoId"],
                        ServicoNome = reader["ServicoNome"] == DBNull.Value ? null : reader["ServicoNome"].ToString(),
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
                string query = @"SELECT COUNT(*) 
                         FROM Avaliacoes 
                         WHERE AgendamentoId = @id 
                         AND UsuarioId = @usuarioId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", agendamentoId);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                return (int)cmd.ExecuteScalar() > 0;
            }
        }


        public decimal BuscarMedia(int profissionalId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            SELECT ISNULL(AVG(CAST(Nota AS DECIMAL(10,2))), 0)
            FROM Avaliacoes
            WHERE ProfissionalId = @ProfissionalId
        ";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

    }
}
