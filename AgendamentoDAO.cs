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
        (ClienteId, ServicoId, ProfissionalId, Data, Hora, Status, Descricao)
        VALUES (@ClienteId, @ServicoId, @ProfissionalId, @Data, @Hora, @Status, @Descricao)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ClienteId", ag.ClienteId);
                cmd.Parameters.AddWithValue("@ServicoId", ag.ServicoId);
                cmd.Parameters.AddWithValue("@ProfissionalId", ag.ProfissionalId); // 🔥 AQUI
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
                string query = @"
        SELECT 
            a.Id,
            a.ClienteId,
            p.Id AS ProfissionalId,
            u.Nome AS ProfissionalNome,
            s.Nome AS Servico,
            a.Data,
            a.Hora,
            a.Status,
            a.Descricao
        FROM Agendamentos a
        INNER JOIN Servicos s ON a.ServicoId = s.Id
        INNER JOIN Profissionais p ON s.ProfissionalId = p.Id
        INNER JOIN Usuarios u ON p.UsuarioId = u.Id
        WHERE a.ClienteId = @ClienteId";

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
                        NomeProfissional = reader["ProfissionalNome"].ToString(),
                        Servico = reader["Servico"].ToString(),
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



        public Agendamento BuscarPorId(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT * FROM Agendamentos WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Agendamento
                    {
                        Id = (int)reader["Id"],
                        ClienteId = (int)reader["ClienteId"],
                        ProfissionalId = (int)reader["ProfissionalId"],
                        Data = (DateTime)reader["Data"],
                        Hora = (TimeSpan)reader["Hora"],
                        Status = reader["Status"].ToString(),
                        Descricao = reader["Descricao"].ToString()
                    };
                }
            }

            return null;
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

        public void Cancelar(int id, string status)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "UPDATE Agendamentos SET Status = @Status WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Status", status);

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

        public (int total, int pendentes, int confirmados, int finalizados, int cancelados) DashboardProfissional(int profissionalId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                SELECT 
                    COUNT(*) AS Total,
                    ISNULL(SUM(CASE WHEN Status = 'Pendente' THEN 1 ELSE 0 END), 0) AS Pendentes,
                    ISNULL(SUM(CASE WHEN Status = 'Confirmado' THEN 1 ELSE 0 END), 0) AS Confirmados,
                    ISNULL(SUM(CASE WHEN Status = 'Finalizado' THEN 1 ELSE 0 END), 0) AS Finalizados,
                    ISNULL(SUM(CASE 
                        WHEN Status LIKE 'Cancelado%' THEN 1 
                        ELSE 0 
                    END), 0) AS Cancelados
                FROM Agendamentos
                WHERE ProfissionalId = @ProfissionalId
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return (
                        (int)reader["Total"],
                        (int)reader["Pendentes"],
                        (int)reader["Confirmados"],
                        (int)reader["Finalizados"],
                        (int)reader["Cancelados"]
                    );
                }
            }

            return (0, 0, 0, 0, 0);
        }

        public void Finalizar(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "UPDATE Agendamentos SET Status = 'Finalizado' WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                cmd.ExecuteNonQuery();
            }
        }


    }
}