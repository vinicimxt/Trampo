using Microsoft.Data.SqlClient;
namespace BD_TRAMPO
{

    public class SubcategoriaDAO
    {
        Conexao conexao = new Conexao();
        public List<Subcategoria> ListarPorCategoria(int categoriaId)
        {
            List<Subcategoria> lista = new List<Subcategoria>();
            
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT * FROM Subcategorias WHERE CategoriaId = @CategoriaId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Subcategoria
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString()
                    });
                }
            }

            return lista;
        }

        public List<Subcategoria> ListarTodas()
        {
            List<Subcategoria> lista = new List<Subcategoria>();
            
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT * FROM Subcategorias";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Subcategoria
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