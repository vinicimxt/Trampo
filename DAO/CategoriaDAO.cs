using Microsoft.Data.SqlClient;

namespace BD_TRAMPO
{

    public class CategoriaDAO
    {

        public List<Categoria> Listar()
        {
            List<Categoria> lista = new List<Categoria>();
            Conexao conexao = new Conexao();
            
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT * FROM Categorias";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Categoria
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString()
                    });
                }
            }

            return lista;
        }

    }
}