using Microsoft.Data.SqlClient;

namespace BD_TRAMPO.DAO
{


    public class DisponibilidadeDAO
    {
        private Conexao conexao = new Conexao();

        public List<Disponibilidade> BuscarPorServico(int servicoId)
        {
            List<Disponibilidade> lista = new List<Disponibilidade>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                SELECT Id, ProfissionalId, ServicoId, DiaSemana,
                HoraInicio, HoraFim, Ativo
                FROM Disponibilidade
                WHERE ServicoId = @ServicoId
                AND Ativo = 1
            ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ServicoId", servicoId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Disponibilidade
                    {
                        Id = (int)reader["Id"],
                        ProfissionalId = (int)reader["ProfissionalId"],
                        ServicoId = (int)reader["ServicoId"],
                        DiaSemana = (int)reader["DiaSemana"],
                        HoraInicio = (TimeSpan)reader["HoraInicio"],
                        HoraFim = (TimeSpan)reader["HoraFim"],
                        Ativo = (bool)reader["Ativo"]
                    });
                }
            }

            return lista;
        }


        public void Inserir(Disponibilidade d)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            INSERT INTO Disponibilidade
            (ProfissionalId, ServicoId, DiaSemana, HoraInicio, HoraFim, Ativo)
            VALUES
            (@ProfissionalId, @ServicoId, @DiaSemana, @HoraInicio, @HoraFim, 1)
        ";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", d.ProfissionalId);
                cmd.Parameters.AddWithValue("@ServicoId", d.ServicoId);
                cmd.Parameters.AddWithValue("@DiaSemana", d.DiaSemana);
                cmd.Parameters.AddWithValue("@HoraInicio", d.HoraInicio);
                cmd.Parameters.AddWithValue("@HoraFim", d.HoraFim);

                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(Disponibilidade d)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            UPDATE Disponibilidade SET
                DiaSemana = @DiaSemana,
                HoraInicio = @HoraInicio,
                HoraFim = @HoraFim,
                Ativo = @Ativo
            WHERE Id = @Id
        ";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Id", d.Id);
                cmd.Parameters.AddWithValue("@DiaSemana", d.DiaSemana);
                cmd.Parameters.AddWithValue("@HoraInicio", d.HoraInicio);
                cmd.Parameters.AddWithValue("@HoraFim", d.HoraFim);
                cmd.Parameters.AddWithValue("@Ativo", d.Ativo);

                cmd.ExecuteNonQuery();
            }
        }

        public void Desativar(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "UPDATE Disponibilidade SET Ativo = 0 WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }


    }

}