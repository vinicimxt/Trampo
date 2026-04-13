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