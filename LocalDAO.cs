using Microsoft.Data.SqlClient;

namespace BD_TRAMPO.DAO
{
    public class LocalDAO
    {
        Conexao conexao = new Conexao();

        public void Inserir(Local l)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                INSERT INTO Locais (ProfissionalId, Nome, Endereco)
                VALUES (@ProfissionalId, @Nome, @Endereco)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", l.ProfissionalId);
                cmd.Parameters.AddWithValue("@Nome", l.Nome);
                cmd.Parameters.AddWithValue("@Endereco", l.Endereco);

                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(Local l)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        UPDATE Locais SET
            Nome = @Nome,
            Endereco = @Endereco
        WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Id", l.Id);
                cmd.Parameters.AddWithValue("@Nome", l.Nome);
                cmd.Parameters.AddWithValue("@Endereco", l.Endereco);

                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "DELETE FROM Locais WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }


        public Local BuscarPorId(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT * FROM Locais WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Local
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Endereco = reader["Endereco"].ToString()
                    };
                }
            }

            return null;
        }



        public List<Local> ListarPorProfissional(int profissionalId)
        {
            List<Local> lista = new List<Local>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT * FROM Locais WHERE ProfissionalId = @ProfissionalId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Local
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Endereco = reader["Endereco"].ToString()
                    });
                }
            }

            return lista;
        }
    }
}