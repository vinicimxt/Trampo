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
                string query = @"INSERT INTO Profissionais 
                         (UsuarioId, TipoDocumento, Documento)
                         VALUES (@UsuarioId, @TipoDocumento, @Documento)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@TipoDocumento", tipoDocumento);
                cmd.Parameters.AddWithValue("@Documento", documento);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 && ex.Message.Contains("UX_Profissional_CPF"))
                    {
                        throw new Exception("CPF_DUPLICADO");
                    }

                    throw;
                }
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

        public Profissional BuscarPorId(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT p.Id, u.Nome, u.Email
        FROM Profissionais p
        INNER JOIN Usuarios u ON p.UsuarioId = u.Id
        WHERE p.Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Profissional
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Email = reader["Email"].ToString()
                    };
                }
            }

            return null;
        }

        public void AtualizarEndereco(int usuarioId, string endereco)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        UPDATE Profissionais
        SET Endereco = @Endereco
        WHERE UsuarioId = @UsuarioId";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Endereco", endereco);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                cmd.ExecuteNonQuery();
            }
        }



    }
}