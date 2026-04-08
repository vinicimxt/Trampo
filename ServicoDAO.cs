using Microsoft.Data.SqlClient;

namespace BD_TRAMPO
{

    public class ServicoDAO
    {
        private Conexao conexao = new Conexao();

        public void Inserir(Servico s)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"INSERT INTO Servicos 
            (ProfissionalId, Nome, Descricao, Contato, Atendimento, RaioAtendimento)
            VALUES 
            (@ProfissionalId, @Nome, @Descricao, @Contato, @Atendimento, @RaioAtendimento)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", s.ProfissionalId);
                cmd.Parameters.AddWithValue("@Nome", s.Nome);
                cmd.Parameters.AddWithValue("@Descricao", s.Descricao ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Contato", s.Contato ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Atendimento", s.Atendimento ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RaioAtendimento", s.RaioAtendimento ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public List<Servico> ListarServicos()
        {
            List<Servico> lista = new List<Servico>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT 
            s.Id,
            s.Nome,
            s.Descricao,
            s.Atendimento,
            s.RaioAtendimento,
            s.Contato,
            u.Nome AS NomeProfissional
        FROM Servicos s
        INNER JOIN Profissionais p ON s.ProfissionalId = p.Id
        INNER JOIN Usuarios u ON p.UsuarioId = u.Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Servico
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Descricao = reader["Descricao"].ToString(),
                        Atendimento = reader["Atendimento"].ToString(),
                        RaioAtendimento = reader["RaioAtendimento"] != DBNull.Value
                            ? (int)reader["RaioAtendimento"]
                            : null,
                        Contato = reader["Contato"].ToString(),
                        NomeProfissional = reader["NomeProfissional"].ToString()
                    });
                }
            }

            return lista;
        }

        public int BuscarProfissionalId(int servicoId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT ProfissionalId FROM Servicos WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", servicoId);

                return (int)cmd.ExecuteScalar();
            }
        }


    }

}