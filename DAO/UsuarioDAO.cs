using BD_TRAMPO.Models;

namespace BD_TRAMPO
{
    using Microsoft.Data.SqlClient;

    public class UsuarioDAO
    {
        Conexao conexao = new Conexao();

        public int Inserir(string nome, string email, string senha, string tipo, string telefone)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                INSERT INTO Usuarios (Nome, Email, Senha, Tipo, Telefone)
                OUTPUT INSERTED.Id
                VALUES (@Nome, @Email, @Senha, @Tipo, @Telefone)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Nome", nome);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Senha", senha);
                cmd.Parameters.AddWithValue("@Tipo", tipo);
                cmd.Parameters.AddWithValue("@telefone",
                string.IsNullOrEmpty(telefone) ? (object)DBNull.Value : telefone);

                int id = (int)cmd.ExecuteScalar();

                return id;
            }
        }

        public bool EmailExiste(string email)
        {
            using (SqlConnection conn = conexao.Conectar())
            {

                string sql = "SELECT COUNT(*) FROM Usuarios WHERE Email = @email";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@email", email);

                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }

        public Usuario BuscarLogin(string email, string senhaHash)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT * FROM Usuarios WHERE Email = @Email AND Senha = @Senha";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Senha", senhaHash);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Usuario
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Email = reader["Email"].ToString(),
                        Tipo = reader["Tipo"].ToString()
                    };
                }
            }

            return null;
        }

        public void Remover(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "DELETE FROM Usuarios WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }

    }

}