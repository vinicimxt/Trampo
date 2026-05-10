using Microsoft.Data.SqlClient;
using BD_TRAMPO.Models;

namespace BD_TRAMPO.DAO
{
    public class SuporteDAO
    {
        Conexao conexao = new Conexao();

        //  INSERIR (abrir ticket)
        public void Inserir(Suporte c)
        {
            using (SqlConnection conn = conexao.Conectar())
            {

                string sql = @"
                    INSERT INTO ContatoSuporte
                    (UsuarioId, Tipo, Assunto, Mensagem, Status)
                    VALUES
                    (@UsuarioId, @Tipo, @Assunto, @Mensagem, 'Aberto')";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", c.UsuarioId);
                cmd.Parameters.AddWithValue("@Tipo", c.Tipo);
                cmd.Parameters.AddWithValue("@Assunto", c.Assunto);
                cmd.Parameters.AddWithValue("@Mensagem", c.Mensagem);

                cmd.ExecuteNonQuery();
            }
        }

        //  LISTAR TODOS (ADMIN FUTURO)
        public List<Suporte> ListarTodos()
        {
            List<Suporte> lista = new List<Suporte>();

            using (SqlConnection conn = conexao.Conectar())
            {
                conn.Open();

                string sql = "SELECT * FROM ContatoSuporte ORDER BY DataCriacao DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Suporte
                    {
                        Id = (int)dr["Id"],
                        UsuarioId = (int)dr["UsuarioId"],
                        Tipo = dr["Tipo"].ToString(),
                        Assunto = dr["Assunto"].ToString(),
                        Mensagem = dr["Mensagem"].ToString(),
                        Status = dr["Status"].ToString(),
                        DataCriacao = (DateTime)dr["DataCriacao"]
                    });
                }
            }

            return lista;
        }

        //  LISTAR POR USUÁRIO (MEUS CONTATOS)
        public List<Suporte> ListarPorUsuario(int usuarioId)
        {
            List<Suporte> lista = new List<Suporte>();

            using (SqlConnection conn = conexao.Conectar())
            {
                conn.Open();

                string sql = @"
                    SELECT * FROM ContatoSuporte
                    WHERE UsuarioId = @UsuarioId
                    ORDER BY DataCriacao DESC";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    lista.Add(new Suporte
                    {
                        Id = (int)dr["Id"],
                        UsuarioId = (int)dr["UsuarioId"],
                        Tipo = dr["Tipo"].ToString(),
                        Assunto = dr["Assunto"].ToString(),
                        Mensagem = dr["Mensagem"].ToString(),
                        Status = dr["Status"].ToString(),
                        DataCriacao = (DateTime)dr["DataCriacao"]
                    });
                }
            }

            return lista;
        }

        //  ATUALIZAR STATUS (ADMIN)
        public void AtualizarStatus(int id, string status)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                conn.Open();

                string sql = @"
                    UPDATE ContatoSuporte
                    SET Status = @Status
                    WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }

        //  BUSCAR POR ID
        public Suporte BuscarPorId(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                conn.Open();

                string sql = "SELECT * FROM ContatoSuporte WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    return new Suporte
                    {
                        Id = (int)dr["Id"],
                        UsuarioId = (int)dr["UsuarioId"],
                        Tipo = dr["Tipo"].ToString(),
                        Assunto = dr["Assunto"].ToString(),
                        Mensagem = dr["Mensagem"].ToString(),
                        Status = dr["Status"].ToString(),
                        DataCriacao = (DateTime)dr["DataCriacao"]
                    };
                }
            }

            return null;
        }
    }
}