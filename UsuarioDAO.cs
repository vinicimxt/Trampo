using BD_TRAMPO.Models;

namespace BD_TRAMPO
{
    using Microsoft.Data.SqlClient;

    public class UsuarioDAO
    {
        Conexao conexao = new Conexao();

        public int Inserir(string nome, string email, string senha, string tipo)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                INSERT INTO Usuarios (Nome, Email, Senha, Tipo)
                OUTPUT INSERTED.Id
                VALUES (@Nome, @Email, @Senha, @Tipo)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Nome", nome);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Senha", senha);
                cmd.Parameters.AddWithValue("@Tipo", tipo);

                int id = (int)cmd.ExecuteScalar();

                return id;
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

    }

}