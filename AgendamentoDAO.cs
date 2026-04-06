using Microsoft.Data.SqlClient;


namespace BD_TRAMPO
{
    public class AgendamentoDAO
    {
        Conexao conexao = new Conexao();

        public void Inserir(Agendamento ag)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                INSERT INTO Agendamentos 
                (ClienteId, ProfissionalId, Data, Hora, Status, Descricao)
                VALUES (@ClienteId, @ProfissionalId, @Data, @Hora, @Status, @Descricao)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ClienteId", ag.ClienteId);
                cmd.Parameters.AddWithValue("@ProfissionalId", ag.ProfissionalId);
                cmd.Parameters.AddWithValue("@Data", ag.Data);
                cmd.Parameters.AddWithValue("@Hora", ag.Hora);
                cmd.Parameters.AddWithValue("@Status", ag.Status);
                cmd.Parameters.AddWithValue("@Descricao", ag.Descricao);

                cmd.ExecuteNonQuery();
            }
        }


        public List<Agendamento> ListarPorCliente(int clienteId)
        {
            List<Agendamento> lista = new List<Agendamento>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT A.*, U.Nome AS NomeProfissional , P.TipoServico FROM Agendamentos A INNER JOIN Profissionais P ON A.ProfissionalId = P.Id INNER JOIN Usuarios U ON P.UsuarioId = U.Id WHERE A.ClienteId = @ClienteId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Agendamento
                    {
                        Id = (int)reader["Id"],
                        ClienteId = (int)reader["ClienteId"],
                        ProfissionalId = (int)reader["ProfissionalId"],
                        NomeProfissional = reader["NomeProfissional"].ToString(),
                        Servico = reader["TipoServico"].ToString(),
                        Data = (DateTime)reader["Data"],
                        Hora = (TimeSpan)reader["Hora"],
                        Status = reader["Status"].ToString(),
                        Descricao = reader["Descricao"].ToString()
                    });
                }
            }

            return lista;
        }



        public List<Agendamento> ListarPorProfissional(int profissionalId)
        {
            List<Agendamento> lista = new List<Agendamento>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT A.*, U.Nome AS NomeCliente FROM Agendamentos A INNER JOIN Clientes C ON A.ClienteId = C.Id INNER JOIN Usuarios U ON C.UsuarioId = U.Id WHERE A.ProfissionalId = @ProfissionalId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Agendamento
                    {
                        Id = (int)reader["Id"],
                        ClienteId = (int)reader["ClienteId"],
                        NomeCliente = reader["NomeCliente"].ToString(),
                        ProfissionalId = (int)reader["ProfissionalId"],
                        Data = (DateTime)reader["Data"],
                        Hora = (TimeSpan)reader["Hora"],
                        Status = reader["Status"].ToString(),
                        Descricao = reader["Descricao"].ToString()
                    });
                }
            }

            return lista;
        }

        public void AtualizarStatus(int id, string status)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "UPDATE Agendamentos SET Status = @Status WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }

        public void Cancelar(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                UPDATE Agendamentos 
                SET Status = 'CanceladoCliente',
                DataCancelamento = GETDATE()
                WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }
        public int ContarPendentes(int clienteId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                SELECT COUNT(*) 
                FROM Agendamentos 
                WHERE ClienteId = @ClienteId 
                AND Status = 'Pendente'";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);

                return (int)cmd.ExecuteScalar();
            }
        }


        public int ContarCancelamentosHoje(int clienteId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                SELECT COUNT(*) 
                FROM Agendamentos
                WHERE ClienteId = @ClienteId
                AND Status LIKE 'Cancelado%'
                AND CAST(DataCancelamento AS DATE) = CAST(GETDATE() AS DATE)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);

                return (int)cmd.ExecuteScalar();
            }
        }

        public (int total, int pendentes, int confirmados, int cancelados) DashboardProfissional(int profissionalId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT 
            COUNT(*) AS Total,
            SUM(CASE WHEN Status = 'Pendente' THEN 1 ELSE 0 END) AS Pendentes,
            SUM(CASE WHEN Status = 'Confirmado' THEN 1 ELSE 0 END) AS Confirmados,
            SUM(CASE WHEN Status LIKE 'Cancelado%' THEN 1 ELSE 0 END) AS Cancelados
        FROM Agendamentos
        WHERE ProfissionalId = @ProfissionalId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return (
                        total: (int)reader["Total"],
                        pendentes: (int)reader["Pendentes"],
                        confirmados: (int)reader["Confirmados"],
                        cancelados: (int)reader["Cancelados"]
                    );
                }

                return (0, 0, 0, 0);
            }
        }


    }
}