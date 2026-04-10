using Microsoft.Data.SqlClient;

namespace BD_TRAMPO
{

    public class ServicoDAO
    {
        private Conexao conexao = new Conexao();

        public void Inserir(Servico s)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"INSERT INTO Servicos 
            (ProfissionalId, SubcategoriaId, Nome, Descricao, Contato, Atendimento, RaioAtendimento)
            VALUES 
            (@ProfissionalId,@SubcategoriaId, @Nome, @Descricao, @Contato, @Atendimento, @RaioAtendimento)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", s.ProfissionalId);
                cmd.Parameters.AddWithValue("@Nome", s.Nome);
                cmd.Parameters.AddWithValue("@SubcategoriaId", s.SubcategoriaId);
                cmd.Parameters.AddWithValue("@Descricao", s.Descricao ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Contato", s.Contato ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Atendimento", s.Atendimento ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@RaioAtendimento", s.RaioAtendimento ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public List<Servico> ListarServicos()
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
                    s.Contato,
                    u.Nome AS NomeProfissional,
                    sc.Nome AS Subcategoria,
                    c.Nome AS Categoria
                FROM Servicos s
                INNER JOIN Profissionais p ON s.ProfissionalId = p.Id
                INNER JOIN Usuarios u ON p.UsuarioId = u.Id
                LEFT JOIN Subcategorias sc ON s.SubcategoriaId = sc.Id
                LEFT JOIN Categorias c ON sc.CategoriaId = c.Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Servico
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Categoria = reader["Categoria"] != DBNull.Value ? reader["Categoria"].ToString() : "Não definida",

                        Subcategoria = reader["Subcategoria"] != DBNull.Value ? reader["Subcategoria"].ToString() : "Não definida",
                        Atendimento = reader["Atendimento"].ToString(),
                        RaioAtendimento = reader["RaioAtendimento"] != DBNull.Value
                            ? (int)reader["RaioAtendimento"]
                            : null,
                        Contato = reader["Contato"].ToString(),
                        NomeProfissional = reader["NomeProfissional"].ToString()
                    });
                }
            }

            return lista;
        }

        public List<Servico> ListarPorProfissional(int profissionalId)
        {
            List<Servico> lista = new List<Servico>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"SELECT 
            s.Id,
            s.Nome,
            s.Descricao,
            s.Contato,
            s.Atendimento,
            s.RaioAtendimento,
            sc.Nome AS Subcategoria,
            c.Nome AS Categoria
        FROM Servicos s
        INNER JOIN Subcategorias sc ON s.SubcategoriaId = sc.Id
        INNER JOIN Categorias c ON sc.CategoriaId = c.Id
        WHERE s.ProfissionalId = @ProfissionalId";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId); // 🔥 ESSENCIAL

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Servico
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Descricao = reader["Descricao"].ToString(),
                        Contato = reader["Contato"].ToString(),
                        Atendimento = reader["Atendimento"].ToString(),
                        Categoria = reader["Categoria"].ToString(),
                        Subcategoria = reader["Subcategoria"].ToString()
                    });
                }
            }

            return lista;
        }

        public int BuscarProfissionalId(int servicoId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT ProfissionalId FROM Servicos WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", servicoId);

                return (int)cmd.ExecuteScalar();
            }
        }

        public List<Categoria> Listar()
        {
            List<Categoria> lista = new List<Categoria>();

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


    }

}