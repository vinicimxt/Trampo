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
            (ProfissionalId, SubcategoriaId, Nome, Descricao, Contato, Atendimento, LinkOnline, LocalId)
            VALUES 
            (@ProfissionalId,@SubcategoriaId, @Nome, @Descricao, @Contato, @Atendimento,   @LinkOnline,@LocalId)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", s.ProfissionalId);
                cmd.Parameters.AddWithValue("@Nome", s.Nome);
                cmd.Parameters.AddWithValue("@SubcategoriaId", s.SubcategoriaId);
                cmd.Parameters.AddWithValue("@Descricao", s.Descricao ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Contato", s.Contato ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Atendimento", s.Atendimento ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LinkOnline",
                string.IsNullOrEmpty(s.LinkOnline) ? (object)DBNull.Value : s.LinkOnline);
                cmd.Parameters.AddWithValue("@LocalId",
                s.LocalId.HasValue ? (object)s.LocalId : DBNull.Value);

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
            s.ProfissionalId,
            s.Descricao,
            s.Atendimento,
            s.Contato,
            s.LinkOnline,
            u.Nome AS NomeProfissional,
            sc.Nome AS Subcategoria,
            c.Nome AS Categoria,
            l.Endereco
        FROM Servicos s
        INNER JOIN Profissionais p ON s.ProfissionalId = p.Id
        INNER JOIN Usuarios u ON p.UsuarioId = u.Id
        INNER JOIN Subcategorias sc ON s.SubcategoriaId = sc.Id
        INNER JOIN Categorias c ON sc.CategoriaId = c.Id
        LEFT JOIN Locais l ON s.LocalId = l.Id"; // 🔥 AQUI

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Servico
                    {
                        Id = (int)reader["Id"],
                        ProfissionalId = (int)reader["ProfissionalId"],
                        Nome = reader["Nome"].ToString(),
                        Descricao = reader["Descricao"] != DBNull.Value ? reader["Descricao"].ToString() : "",
                        Atendimento = reader["Atendimento"].ToString(),
                        Contato = reader["Contato"] != DBNull.Value ? reader["Contato"].ToString() : "",
                        NomeProfissional = reader["NomeProfissional"].ToString(),
                        Categoria = reader["Categoria"].ToString(),
                        Subcategoria = reader["Subcategoria"].ToString(),
                        LinkOnline = reader["LinkOnline"] != DBNull.Value
                            ? reader["LinkOnline"].ToString()
                            : null,
                        Endereco = reader["Endereco"] != DBNull.Value
                            ? reader["Endereco"].ToString()
                            : ""
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
                string query = @"
                SELECT 
                    s.Id,
                    s.Nome,
                    s.Descricao,
                    s.Contato,
                    s.Atendimento,
                    s.LinkOnline,
                    sc.Nome AS Subcategoria,
                    c.Nome AS Categoria
                FROM Servicos s
                INNER JOIN Profissionais p ON s.ProfissionalId = p.Id
                INNER JOIN Subcategorias sc ON s.SubcategoriaId = sc.Id
                INNER JOIN Categorias c ON sc.CategoriaId = c.Id
                WHERE s.ProfissionalId = @ProfissionalId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Servico
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Descricao = reader["Descricao"].ToString(),
                        Contato = reader["Contato"] != DBNull.Value ? reader["Contato"].ToString() : "",
                        Atendimento = reader["Atendimento"].ToString(),
                        Categoria = reader["Categoria"] != DBNull.Value ? reader["Categoria"].ToString() : "",
                        Subcategoria = reader["Subcategoria"] != DBNull.Value ? reader["Subcategoria"].ToString() : "",
                        LinkOnline = reader["LinkOnline"] != DBNull.Value
                        ? reader["LinkOnline"].ToString()
                        : null
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

                object result = cmd.ExecuteScalar();

                return result != null ? (int)result : 0;
            }
        }
        public int ContarPorProfissional(int profissionalId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {

                string query = "SELECT COUNT(*) FROM Servicos WHERE ProfissionalId = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", profissionalId);

                return (int)cmd.ExecuteScalar();
            }
        }

        public Servico BuscarPorId(int id)
        {
            Servico servico = null;

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
           SELECT 
                Id,
                Nome,
                Descricao,
                Atendimento,
                Contato,
                LocalId
            FROM Servicos
            WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    servico = new Servico
                    {
                        Id = (int)reader["Id"],
                        Nome = reader["Nome"].ToString(),
                        Descricao = reader["Descricao"].ToString(),
                        Atendimento = reader["Atendimento"].ToString(),
                        Contato = reader["Contato"].ToString(),
                        LocalId = reader["LocalId"] != DBNull.Value
                        ? (int)reader["LocalId"]
                        : (int?)null
                    };
                }
            }

            return servico;
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


        public void Atualizar(Servico s)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        UPDATE Servicos SET
            Nome = @Nome,
            Descricao = @Descricao,
            Contato = @Contato,
            Atendimento = @Atendimento,
            SubcategoriaId = @SubcategoriaId,
            LinkOnline = @LinkOnline
        WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Id", s.Id);
                cmd.Parameters.AddWithValue("@Nome", s.Nome);
                cmd.Parameters.AddWithValue("@Descricao", s.Descricao ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Contato", s.Contato ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Atendimento", s.Atendimento);
                cmd.Parameters.AddWithValue("@SubcategoriaId", s.SubcategoriaId);
                cmd.Parameters.AddWithValue("@LinkOnline",
                string.IsNullOrEmpty(s.LinkOnline) ? (object)DBNull.Value : s.LinkOnline);
                cmd.ExecuteNonQuery();

            }
        }

        public void Excluir(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "DELETE FROM Servicos WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }





    }

}