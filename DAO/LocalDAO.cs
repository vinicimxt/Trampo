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
                cmd.Parameters.AddWithValue("@Nome", (object?)l.Nome ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Endereco", l.Endereco);

                cmd.ExecuteNonQuery();
            }
        }

        public bool Atualizar(Local l)
        {
            var original = BuscarPorId(l.Id);
            if (original == null) return false;
            using var conn = conexao.Conectar();
            using var tx = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            using var cmd = new SqlCommand(@"
                DECLARE @r int;
                EXEC @r=sys.sp_getapplock @Resource=@Recurso, @LockMode='Exclusive',
                    @LockOwner='Transaction', @LockTimeout=10000;
                IF @r<0 THROW 50001, 'Agenda em atualização.', 1;
                IF EXISTS(SELECT 1 FROM Agendamentos WHERE LocalId=@Id)
                    AND EXISTS(SELECT 1 FROM Locais WHERE Id=@Id
                        AND (ISNULL(Nome,'')<>ISNULL(@Nome,'') OR Endereco<>@Endereco))
                    SELECT 0;
                ELSE BEGIN
                    UPDATE Locais SET Nome=@Nome, Endereco=@Endereco WHERE Id=@Id;
                    SELECT @@ROWCOUNT;
                END", conn, tx);
            cmd.Parameters.AddWithValue("@Recurso", "TRAMPO:Profissional:" + original.ProfissionalId);
            cmd.Parameters.AddWithValue("@Id", l.Id);
            cmd.Parameters.AddWithValue("@Nome", (object?)l.Nome ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Endereco", l.Endereco);
            bool atualizado = Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            tx.Commit();
            return atualizado;
        }
        // Sem coluna Ativo: recusa exclusão vinculada, preservando dados e histórico.
        public bool Excluir(int id)
        {
            var local = BuscarPorId(id);
            if (local == null) return false;
            using var conn = conexao.Conectar();
            using var tx = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            using var cmd = new SqlCommand(@"
                DECLARE @r int;
                EXEC @r=sys.sp_getapplock @Resource=@Recurso, @LockMode='Exclusive',
                    @LockOwner='Transaction', @LockTimeout=10000;
                IF @r<0 THROW 50001, 'Agenda em atualização.', 1;
                IF EXISTS(SELECT 1 FROM Servicos WHERE LocalId=@Id)
                    OR EXISTS(SELECT 1 FROM Agendamentos WHERE LocalId=@Id)
                    SELECT 0;
                ELSE BEGIN
                    DELETE FROM Locais WHERE Id=@Id;
                    SELECT @@ROWCOUNT;
                END", conn, tx);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Recurso", "TRAMPO:Profissional:" + local.ProfissionalId);
            bool removido = Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            tx.Commit();
            return removido;
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
                        ProfissionalId = (int)reader["ProfissionalId"],
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
                        ProfissionalId = (int)reader["ProfissionalId"],
                        Nome = reader["Nome"].ToString(),
                        Endereco = reader["Endereco"].ToString()
                    });
                }
            }

            return lista;
        }
    }
}