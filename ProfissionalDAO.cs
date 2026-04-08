namespace BD_TRAMPO
{

    using Microsoft.Data.SqlClient;

    public class ProfissionalDAO
    {
        Conexao conexao = new Conexao();

        public void Inserir(int usuarioId, string tipoDocumento, string documento)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
               INSERT INTO Profissionais 
                   (UsuarioId, TipoDocumento, Documento)
                   VALUES 
                   (@UsuarioId, @TipoDocumento, @Documento)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@TipoDocumento", tipoDocumento);
                cmd.Parameters.AddWithValue("@Documento", documento);

                cmd.ExecuteNonQuery();
            }
        }


        public List<Servico> ListarPorUsuario(int usuarioId)
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
            s.Contato
        FROM Servicos s
        INNER JOIN Profissionais p ON s.ProfissionalId = p.Id
        WHERE p.UsuarioId = @UsuarioId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

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
                            : 0,
                        Contato = reader["Contato"].ToString()
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