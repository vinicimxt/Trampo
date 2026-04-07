namespace BD_TRAMPO
{

    using Microsoft.Data.SqlClient;

    public class ProfissionalDAO
    {
        Conexao conexao = new Conexao();

        public void Inserir(int usuarioId, string atendimento, int? raio, string tipoDocumento, string documento)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
               INSERT INTO Profissionais 
                   (UsuarioId, Atendimento, Raio, TipoDocumento, Documento)
                   VALUES 
                   (@UsuarioId, @Atendimento, @Raio, @TipoDocumento, @Documento)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@Atendimento", atendimento);
                cmd.Parameters.AddWithValue("@Raio", (object?)raio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TipoDocumento", tipoDocumento);
                cmd.Parameters.AddWithValue("@Documento", documento);

                cmd.ExecuteNonQuery();
            }
        }
        public List<Profissional> Listar()
        {
            List<Profissional> lista = new List<Profissional>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT 
            p.Id,
            u.Nome,
            p.TipoServico,
            p.Descricao,
            p.Atendimento,
            p.RaioAtendimento
        FROM Profissionais p
        INNER JOIN Usuarios u ON p.UsuarioId = u.Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    lista.Add(new Profissional
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Servico = reader["TipoServico"].ToString(),
                        Descricao = reader["Descricao"].ToString(),
                        Atendimento = reader["Atendimento"].ToString(),
                        Raio = reader["RaioAtendimento"] != DBNull.Value ? (int)reader["RaioAtendimento"] : 0
                    });

                }
            }

            return lista;
        }

        public List<Profissional> ListarPorUsuario(int usuarioId)
        {
            List<Profissional> lista = new List<Profissional>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT 
            p.Id,
            u.Nome,
            p.TipoServico,
            p.Descricao,
            p.Atendimento,
            p.RaioAtendimento
        FROM Profissionais p
        INNER JOIN Usuarios u ON p.UsuarioId = u.Id
        WHERE p.UsuarioId = @UsuarioId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Profissional
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Servico = reader["TipoServico"].ToString(),
                        Descricao = reader["Descricao"].ToString(),
                        Atendimento = reader["Atendimento"].ToString(),
                        Raio = reader["RaioAtendimento"] != DBNull.Value ? (int)reader["RaioAtendimento"] : 0
                    });
                }
            }

            return lista;
        }

        public int BuscarPorUsuario(int usuarioId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT Id FROM Profissionais WHERE UsuarioId = @UsuarioId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                object result = cmd.ExecuteScalar();

                if (result != null)
                    return (int)result;

                return 0;
            }
        }


    }
}