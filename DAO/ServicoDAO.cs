using BD_TRAMPO.Contracts;
using Microsoft.Data.SqlClient;
using BD_TRAMPO.DAO;

namespace BD_TRAMPO
{

    public class ServicoDAO
    {
        private Conexao conexao = new Conexao();

        public int Inserir(Servico s)
        {
            using var conn = conexao.Conectar();
            return Inserir(s, conn, null);
        }

        private int Inserir(Servico s, SqlConnection conn, SqlTransaction? tx)
        {

                string query = @"
        INSERT INTO Servicos
        (ProfissionalId, SubcategoriaId, Nome, Descricao,
         Atendimento, LinkOnline, LocalId, TipoPreco, PrecoBase)

        VALUES
        (@ProfissionalId, @SubcategoriaId, @Nome, @Descricao,
         @Atendimento, @LinkOnline, @LocalId, @TipoPreco, @PrecoBase);

        SELECT SCOPE_IDENTITY();
        ";

                SqlCommand cmd = new SqlCommand(query, conn, tx);

                cmd.Parameters.AddWithValue("@ProfissionalId", s.ProfissionalId);
                cmd.Parameters.AddWithValue("@SubcategoriaId", s.SubcategoriaId);
                cmd.Parameters.AddWithValue("@Nome", s.Nome);

                cmd.Parameters.AddWithValue("@Descricao",
                    string.IsNullOrEmpty(s.Descricao)
                    ? (object)DBNull.Value
                    : s.Descricao);

                cmd.Parameters.AddWithValue("@Atendimento", s.Atendimento);

                cmd.Parameters.AddWithValue("@LinkOnline",
                    string.IsNullOrEmpty(s.LinkOnline)
                    ? (object)DBNull.Value
                    : s.LinkOnline);

                cmd.Parameters.AddWithValue("@LocalId",
                    s.LocalId.HasValue
                    ? (object)s.LocalId
                    : DBNull.Value);

                cmd.Parameters.AddWithValue("@TipoPreco", s.TipoPreco);

                cmd.Parameters.AddWithValue("@PrecoBase",
                    s.PrecoBase.HasValue
                    ? s.PrecoBase.Value
                    : DBNull.Value);

                int idGerado = Convert.ToInt32(cmd.ExecuteScalar());

                return idGerado;

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
            s.LinkOnline,
            s.Ativo,
            u.Nome AS NomeProfissional,
            sc.Nome AS Subcategoria,
            c.Nome AS Categoria,
            l.Endereco
        FROM Servicos s
        INNER JOIN Profissionais p ON s.ProfissionalId = p.Id
        INNER JOIN Usuarios u ON p.UsuarioId = u.Id
        INNER JOIN Subcategorias sc ON s.SubcategoriaId = sc.Id
        INNER JOIN Categorias c ON sc.CategoriaId = c.Id
        LEFT JOIN Locais l ON s.LocalId = l.Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Servico
                    {
                        Id = (int)reader["Id"],
                        ProfissionalId = (int)reader["ProfissionalId"],
                        Nome = reader.GetString(reader.GetOrdinal("Nome")),
                        Descricao = reader["Descricao"] != DBNull.Value ? (reader.IsDBNull(reader.GetOrdinal("Descricao")) ? "" : reader.GetString(reader.GetOrdinal("Descricao"))) : "",
                        Atendimento = reader.GetString(reader.GetOrdinal("Atendimento")),
                        NomeProfissional = (reader.IsDBNull(reader.GetOrdinal("NomeProfissional")) ? "" : reader.GetString(reader.GetOrdinal("NomeProfissional"))),
                        Categoria = (reader.IsDBNull(reader.GetOrdinal("Categoria")) ? "" : reader.GetString(reader.GetOrdinal("Categoria"))),
                        Subcategoria = (reader.IsDBNull(reader.GetOrdinal("Subcategoria")) ? "" : reader.GetString(reader.GetOrdinal("Subcategoria"))),
                        LinkOnline = Seguranca.UrlHttpValida((reader.IsDBNull(reader.GetOrdinal("LinkOnline")) ? "" : reader.GetString(reader.GetOrdinal("LinkOnline"))))
                            ? (reader.IsDBNull(reader.GetOrdinal("LinkOnline")) ? "" : reader.GetString(reader.GetOrdinal("LinkOnline")))
                            : null,
                        Endereco = reader["Endereco"] != DBNull.Value
                            ? (reader.IsDBNull(reader.GetOrdinal("Endereco")) ? "" : reader.GetString(reader.GetOrdinal("Endereco")))
                            : "",
                        Ativo = (bool)reader["Ativo"]
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
                    s.ProfissionalId,
                    s.Descricao,
                    s.Atendimento,
                    s.LinkOnline,
                    s.Ativo, s.LocalId, s.SubcategoriaId, s.TipoPreco, s.PrecoBase,
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
                    Servico s = new Servico
                    {
                        Id = (int)reader["Id"],
                        Nome = reader.GetString(reader.GetOrdinal("Nome")),
                        ProfissionalId = (int)reader["ProfissionalId"],
                        Descricao = (reader.IsDBNull(reader.GetOrdinal("Descricao")) ? "" : reader.GetString(reader.GetOrdinal("Descricao"))),
                        Atendimento = reader.GetString(reader.GetOrdinal("Atendimento")),

                        Categoria = reader["Categoria"] != DBNull.Value
                            ? (reader.IsDBNull(reader.GetOrdinal("Categoria")) ? "" : reader.GetString(reader.GetOrdinal("Categoria")))
                            : "",

                        Subcategoria = reader["Subcategoria"] != DBNull.Value
                            ? (reader.IsDBNull(reader.GetOrdinal("Subcategoria")) ? "" : reader.GetString(reader.GetOrdinal("Subcategoria")))
                            : "",

                        LinkOnline = Seguranca.UrlHttpValida((reader.IsDBNull(reader.GetOrdinal("LinkOnline")) ? "" : reader.GetString(reader.GetOrdinal("LinkOnline"))))
                            ? (reader.IsDBNull(reader.GetOrdinal("LinkOnline")) ? "" : reader.GetString(reader.GetOrdinal("LinkOnline")))
                            : null,

                        Ativo = (bool)reader["Ativo"],
                        LocalId = reader["LocalId"] == DBNull.Value ? null : (int)reader["LocalId"],
                        SubcategoriaId = (int)reader["SubcategoriaId"],
                        TipoPreco = reader.GetString(reader.GetOrdinal("TipoPreco")),
                        PrecoBase = reader["PrecoBase"] == DBNull.Value ? null : (decimal)reader["PrecoBase"]
                    };

                    // =========================
                    // DISPONIBILIDADE
                    // =========================

                    DisponibilidadeDAO dispDAO = new DisponibilidadeDAO();

                    var disponibilidade = dispDAO.BuscarPorServico(s.Id);

                    if (disponibilidade.Any())
                    {
                        s.HoraInicio = disponibilidade.Min(x => x.HoraInicio);

                        s.HoraFim = disponibilidade.Max(x => x.HoraFim);

                        s.DiasTexto = string.Join(",",
                            disponibilidade
                                .OrderBy(x => x.DiaSemana)
                                .Select(x => x.DiaSemana)
                        );
                    }

                    lista.Add(s);
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

        public Servico? BuscarPorId(int id)
        {
            Servico? servico = null;

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                    SELECT 
                        Id,
                        ProfissionalId,
                        Nome,
                        Descricao,
                        Atendimento,
                        LocalId,
                        SubcategoriaId,
                        TipoPreco,
                        PrecoBase, Ativo, LinkOnline
                    FROM Servicos
                    WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    servico = new Servico
                    {
                        Ativo = (bool)reader["Ativo"], LinkOnline = Seguranca.UrlHttpValida((reader.IsDBNull(reader.GetOrdinal("LinkOnline")) ? "" : reader.GetString(reader.GetOrdinal("LinkOnline")))) ? (reader.IsDBNull(reader.GetOrdinal("LinkOnline")) ? "" : reader.GetString(reader.GetOrdinal("LinkOnline"))) : null,
                        Id = (int)reader["Id"],
                        ProfissionalId = (int)reader["ProfissionalId"],
                        Nome = reader.GetString(reader.GetOrdinal("Nome")),
                        Descricao = (reader.IsDBNull(reader.GetOrdinal("Descricao")) ? "" : reader.GetString(reader.GetOrdinal("Descricao"))),
                        Atendimento = reader.GetString(reader.GetOrdinal("Atendimento")),
                        LocalId = reader["LocalId"] != DBNull.Value
                            ? (int)reader["LocalId"]
                            : (int?)null,
                        SubcategoriaId = (int)reader["SubcategoriaId"],
                        TipoPreco = reader.GetString(reader.GetOrdinal("TipoPreco")),

                        PrecoBase = reader["PrecoBase"] != DBNull.Value
                            ? Convert.ToDecimal(reader["PrecoBase"])
                            : null
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
                        Nome = reader.GetString(reader.GetOrdinal("Nome"))
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
                        Nome = reader.GetString(reader.GetOrdinal("Nome"))
                    });
                }
            }

            return lista;
        }

        public List<Servico> ListarServicos(string busca, string localizacao, string categoria)
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
                        s.LinkOnline,
                        s.TipoPreco,
                        s.PrecoBase,

                        u.Nome AS NomeProfissional,
                        sc.Nome AS Subcategoria,
                        c.Nome AS Categoria,
                        l.Endereco

                    FROM Servicos s

                    INNER JOIN Profissionais p 
                        ON s.ProfissionalId = p.Id

                    INNER JOIN Usuarios u 
                        ON p.UsuarioId = u.Id

                    INNER JOIN Subcategorias sc 
                        ON s.SubcategoriaId = sc.Id

                    INNER JOIN Categorias c 
                        ON sc.CategoriaId = c.Id

                    LEFT JOIN Locais l 
                        ON s.LocalId = l.Id

                    WHERE s.Ativo = 1

                    AND
                        (@busca IS NULL OR 
                            s.Nome COLLATE Latin1_General_CI_AI LIKE '%' + @busca + '%' OR
                            u.Nome COLLATE Latin1_General_CI_AI LIKE '%' + @busca + '%')

                    AND
                        (@localizacao IS NULL OR 
                            l.Endereco COLLATE Latin1_General_CI_AI LIKE '%' + @localizacao + '%')

                    AND
                        (@categoria IS NULL OR 
                            c.Nome COLLATE Latin1_General_CI_AI LIKE '%' + @categoria + '%')
                    ";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@busca",
                    string.IsNullOrWhiteSpace(busca) ? (object)DBNull.Value : busca);

                cmd.Parameters.AddWithValue("@localizacao",
                    string.IsNullOrWhiteSpace(localizacao) ? (object)DBNull.Value : localizacao);

                cmd.Parameters.AddWithValue("@categoria",
                    string.IsNullOrWhiteSpace(categoria) ? (object)DBNull.Value : categoria);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Servico
                    {
                        Id = (int)reader["Id"],
                        ProfissionalId = (int)reader["ProfissionalId"],
                        Nome = reader.GetString(reader.GetOrdinal("Nome")),

                        Descricao = reader["Descricao"] != DBNull.Value
                            ? (reader.IsDBNull(reader.GetOrdinal("Descricao")) ? "" : reader.GetString(reader.GetOrdinal("Descricao")))
                            : "",

                        Atendimento = reader.GetString(reader.GetOrdinal("Atendimento")),

                        TipoPreco = reader.GetString(reader.GetOrdinal("TipoPreco")),

                        PrecoBase = reader["PrecoBase"] != DBNull.Value
                            ? Convert.ToDecimal(reader["PrecoBase"])
                            : null,

                        NomeProfissional = (reader.IsDBNull(reader.GetOrdinal("NomeProfissional")) ? "" : reader.GetString(reader.GetOrdinal("NomeProfissional"))),

                        Categoria = (reader.IsDBNull(reader.GetOrdinal("Categoria")) ? "" : reader.GetString(reader.GetOrdinal("Categoria"))),

                        Subcategoria = (reader.IsDBNull(reader.GetOrdinal("Subcategoria")) ? "" : reader.GetString(reader.GetOrdinal("Subcategoria"))),

                        LinkOnline = Seguranca.UrlHttpValida((reader.IsDBNull(reader.GetOrdinal("LinkOnline")) ? "" : reader.GetString(reader.GetOrdinal("LinkOnline"))))
                            ? (reader.IsDBNull(reader.GetOrdinal("LinkOnline")) ? "" : reader.GetString(reader.GetOrdinal("LinkOnline")))
                            : null,

                        Endereco = reader["Endereco"] != DBNull.Value
                            ? (reader.IsDBNull(reader.GetOrdinal("Endereco")) ? "" : reader.GetString(reader.GetOrdinal("Endereco")))
                            : ""

                    });
                }
            }

            return lista;
        }



        public void Atualizar(Servico s)
        {
            using var conn = conexao.Conectar();
            Atualizar(s, conn, null);
        }

        private void Atualizar(Servico s, SqlConnection conn, SqlTransaction? tx)
        {

                string query = @"
                UPDATE Servicos SET
                    Nome = @Nome,
                    Descricao = @Descricao,
                    Atendimento = @Atendimento,
                    LocalId = @LocalId,
                    LinkOnline = @LinkOnline,
                    SubcategoriaId = @SubcategoriaId,
                    TipoPreco = @TipoPreco,
                    PrecoBase = @PrecoBase
                WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn, tx);

                cmd.Parameters.AddWithValue("@Id", s.Id);
                cmd.Parameters.AddWithValue("@Nome", s.Nome);

                cmd.Parameters.AddWithValue("@Descricao",
                    s.Descricao ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue("@Atendimento",
                    s.Atendimento ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue("@LocalId",
                    s.LocalId.HasValue ? (object)s.LocalId : DBNull.Value);

                cmd.Parameters.AddWithValue("@LinkOnline",
                    string.IsNullOrEmpty(s.LinkOnline)
                        ? (object)DBNull.Value
                        : s.LinkOnline);

                cmd.Parameters.AddWithValue("@SubcategoriaId", s.SubcategoriaId);

                cmd.Parameters.AddWithValue("@TipoPreco", s.TipoPreco);

                cmd.Parameters.AddWithValue("@PrecoBase",
                    s.PrecoBase.HasValue
                    ? s.PrecoBase.Value
                    : DBNull.Value);

                cmd.ExecuteNonQuery();

        }
        // Serviço e regras são publicados juntos, sob o mesmo bloqueio das reservas.
        public int SalvarComDisponibilidade(Servico s, IEnumerable<int> dias, TimeSpan inicio, TimeSpan fim)
        {
            var diasUnicos = dias.Distinct().ToArray();
            if (diasUnicos.Length == 0 || diasUnicos.Any(d => d < 0 || d > 6) ||
                inicio < TimeSpan.Zero || inicio >= TimeSpan.FromDays(1) ||
                fim < TimeSpan.Zero || fim >= TimeSpan.FromDays(1) || inicio == fim)
                throw new FalhaOperacao(TipoFalha.Validacao, "Disponibilidade inválida.");
            using var conn = conexao.Conectar();
            using var tx = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            using (var validar = new SqlCommand(@"
                DECLARE @r int;
                EXEC @r=sys.sp_getapplock @Resource=@Recurso, @LockMode='Exclusive',
                    @LockOwner='Transaction', @LockTimeout=10000;
                IF @r<0 THROW 51001, 'Agenda em atualização.', 1;
                IF @Id<>0 AND NOT EXISTS(SELECT 1 FROM Servicos WHERE Id=@Id AND ProfissionalId=@P)
                    THROW 51002, 'Serviço não encontrado.', 1;
                IF @Local IS NOT NULL AND NOT EXISTS(SELECT 1 FROM Locais WHERE Id=@Local AND ProfissionalId=@P)
                    THROW 51003, 'Local inválido.', 1;", conn, tx))
            {
                validar.Parameters.AddWithValue("@Recurso", "TRAMPO:Profissional:" + s.ProfissionalId);
                validar.Parameters.AddWithValue("@P", s.ProfissionalId);
                validar.Parameters.AddWithValue("@Id", s.Id);
                validar.Parameters.AddWithValue("@Local", (object?)s.LocalId ?? DBNull.Value);
                FalhasSql.Executar(() => validar.ExecuteNonQuery());
            }
            int id = s.Id;
            if (id == 0) id = Inserir(s, conn, tx); else Atualizar(s, conn, tx);
            using (var remover = new SqlCommand("DELETE FROM Disponibilidade WHERE ServicoId=@Id", conn, tx))
            {
                remover.Parameters.AddWithValue("@Id", id);
                remover.ExecuteNonQuery();
            }
            foreach (int dia in diasUnicos)
            {
                using var inserir = new SqlCommand(@"INSERT INTO Disponibilidade
                    (ProfissionalId,ServicoId,DiaSemana,HoraInicio,HoraFim,Ativo)
                    VALUES(@P,@S,@D,@Inicio,@Fim,1)", conn, tx);
                inserir.Parameters.AddWithValue("@P", s.ProfissionalId);
                inserir.Parameters.AddWithValue("@S", id);
                inserir.Parameters.AddWithValue("@D", dia);
                inserir.Parameters.AddWithValue("@Inicio", inicio);
                inserir.Parameters.AddWithValue("@Fim", fim);
                inserir.ExecuteNonQuery();
            }
            tx.Commit();
            return id;
        }
        public bool Excluir(int id)
        {
            int profissionalId = BuscarProfissionalId(id);
            using var conn = conexao.Conectar();
            using var tx = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            using var cmd = new SqlCommand(@"
                DECLARE @r int;
                EXEC @r=sys.sp_getapplock @Resource=@Recurso, @LockMode='Exclusive',
                    @LockOwner='Transaction', @LockTimeout=10000;
                IF @r<0 THROW 51001, 'Agenda em atualização.', 1;
                IF NOT EXISTS(SELECT 1 FROM Servicos WHERE Id=@Id)
                    THROW 51002, 'Serviço não encontrado.', 1;
                IF EXISTS(SELECT 1 FROM Agendamentos WHERE ServicoId=@Id) BEGIN
                    UPDATE Servicos SET Ativo=0 WHERE Id=@Id;
                    SELECT CAST(1 AS bit);
                END ELSE BEGIN
                    DELETE FROM Disponibilidade WHERE ServicoId=@Id;
                    DELETE FROM Servicos WHERE Id=@Id;
                    SELECT CAST(0 AS bit);
                END", conn, tx);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Recurso", "TRAMPO:Profissional:" + profissionalId);
            bool desativado = Convert.ToBoolean(FalhasSql.Executar(() => cmd.ExecuteScalar()));
            tx.Commit();
            return desativado;
        }

        public List<Servico> ListarPublicos(int pagina, int tamanho)
        {
            using var conn = conexao.Conectar();
            using var cmd = new SqlCommand(@"SELECT Id,ProfissionalId,SubcategoriaId,Nome,Descricao,
                Atendimento,TipoPreco,PrecoBase,Ativo FROM Servicos WHERE Ativo=1
                ORDER BY Id OFFSET @Inicio ROWS FETCH NEXT @Tamanho ROWS ONLY", conn);
            cmd.Parameters.AddWithValue("@Inicio", (pagina - 1) * tamanho);
            cmd.Parameters.AddWithValue("@Tamanho", tamanho);
            using var r = cmd.ExecuteReader();
            var lista = new List<Servico>();
            while (r.Read()) lista.Add(new Servico {
                Id=r.GetInt32(0), ProfissionalId=r.GetInt32(1), SubcategoriaId=r.GetInt32(2),
                Nome=r.GetString(3), Descricao=r.IsDBNull(4) ? "" : r.GetString(4),
                Atendimento=r.GetString(5), TipoPreco=r.GetString(6),
                PrecoBase=r.IsDBNull(7) ? null : r.GetDecimal(7), Ativo=r.GetBoolean(8)
            });
            return lista;
        }
        // PUXAR DO BANCO CARDS DINAMICOS
        public Dictionary<string, int> ContarServicosPorCategoria()
        {
            var dict = new Dictionary<string, int>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT c.Nome, COUNT(*) AS Total
        FROM Servicos s
        INNER JOIN Subcategorias sc ON s.SubcategoriaId = sc.Id
        INNER JOIN Categorias c ON sc.CategoriaId = c.Id
        GROUP BY c.Nome";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    dict.Add(reader.GetString(reader.GetOrdinal("Nome")), (int)reader["Total"]);
                }
            }

            return dict;
        }

        public void Desativar(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            UPDATE Servicos
            SET Ativo = 0
            WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }

        public bool TemAgendamentos(int servicoId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT COUNT(1) FROM Agendamentos WHERE ServicoId = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", servicoId);

                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }



    }

}