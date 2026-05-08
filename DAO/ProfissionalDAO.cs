namespace BD_TRAMPO
{

    using Microsoft.Data.SqlClient;

    public class ProfissionalDAO
    {
        Conexao conexao = new Conexao();

        public void Inserir(int usuarioId, string tipoDocumento, string documento, string contato)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                    INSERT INTO Profissionais 
                    (
                        UsuarioId,
                        TipoDocumento,
                        Documento,
                        Contato
                    )
                    VALUES
                    (
                        @UsuarioId,
                        @TipoDocumento,
                        @Documento,
                        @Contato
                    )";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@TipoDocumento", tipoDocumento);
                cmd.Parameters.AddWithValue("@Documento", documento);
                cmd.Parameters.AddWithValue("@Contato",
                    string.IsNullOrWhiteSpace(contato)
                    ? DBNull.Value
                    : contato);

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
            s.Atendimento
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
                        //Contato = reader["Contato"].ToString()
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
                    SELECT 
                        p.Id,
                        p.Contato,
                        u.Nome,
                        u.Email,
                        u.Telefone
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
                        Email = reader["Email"].ToString(),
                        Contato = reader["Contato"] != DBNull.Value
                            ? reader["Contato"].ToString()
                            : "",
                        Telefone = reader["Telefone"] != DBNull.Value
                            ? reader["Telefone"].ToString()
                            : ""
                    };
                }
            }

            return null;
        }

        // Sistema de notificação 

        public int BuscarUsuarioId(int profissionalId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT UsuarioId FROM Profissionais WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", profissionalId);

                return (int)cmd.ExecuteScalar();
            }
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

        public void AtualizarContato(int usuarioId, string contato)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            UPDATE Profissionais
            SET Contato = @Contato
            WHERE UsuarioId = @UsuarioId";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Contato",
                    string.IsNullOrWhiteSpace(contato)
                    ? DBNull.Value
                    : contato);

                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                cmd.ExecuteNonQuery();
            }
        }



    }
}