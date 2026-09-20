using Microsoft.Data.SqlClient;


namespace BD_TRAMPO
{
    public class AgendamentoDAO
    {
        Conexao conexao = new Conexao();

        public int Inserir(Agendamento ag)
        {
            using var conn = conexao.Conectar();
            using var tx = conn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            SqlCommand Comando(string sql)
            {
                var c = new SqlCommand(sql, conn, tx);
                c.Parameters.AddWithValue("@ClienteId", ag.ClienteId);
                c.Parameters.AddWithValue("@ProfissionalId", ag.ProfissionalId);
                c.Parameters.AddWithValue("@ServicoId", ag.ServicoId);
                c.Parameters.AddWithValue("@Data", ag.Data.Date);
                return c;
            }
            void Bloquear(string recurso)
            {
                using var c = new SqlCommand(@"
                    DECLARE @r int;
                    EXEC @r = sys.sp_getapplock @Resource=@Recurso,
                        @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000;
                    SELECT @r;", conn, tx);
                c.Parameters.AddWithValue("@Recurso", recurso);
                if (Convert.ToInt32(c.ExecuteScalar()) < 0)
                    throw new InvalidOperationException("Agenda em atualização. Tente novamente.");
            }
            // Ordem estável: serializa também o limite de pendentes do cliente.
            Bloquear("TRAMPO:Cliente:" + ag.ClienteId);
            Bloquear("TRAMPO:Profissional:" + ag.ProfissionalId);
            var inicio = ag.Data.Date + ag.Hora;
            if (ag.Hora < TimeSpan.Zero || ag.Hora >= TimeSpan.FromDays(1) ||
                inicio <= DateTime.Now || ag.Data.Date > DateTime.Today.AddMonths(3))
                throw new InvalidOperationException("Data ou horário inválido.");

            string modalidade, plano;
            int usuarioProfissional, usuarioCliente;
            int? local;
            using (var c = Comando(@"SELECT s.Atendimento, s.LocalId, p.Plano, p.UsuarioId
                FROM Servicos s JOIN Profissionais p ON p.Id=s.ProfissionalId
                WHERE s.Id=@ServicoId AND s.ProfissionalId=@ProfissionalId AND s.Ativo=1"))
            using (var r = c.ExecuteReader())
            {
                if (!r.Read()) throw new InvalidOperationException("Serviço indisponível.");
                modalidade = r.GetString(0);
                local = r.IsDBNull(1) ? null : r.GetInt32(1);
                plano = r.GetString(2);
                usuarioProfissional = r.GetInt32(3);
            }
            using (var c = Comando("SELECT UsuarioId, BloqueadoAte FROM Clientes WHERE Id=@ClienteId"))
            using (var r = c.ExecuteReader())
            {
                if (!r.Read()) throw new InvalidOperationException("Cliente não encontrado.");
                usuarioCliente = r.GetInt32(0);
                if (usuarioCliente == usuarioProfissional)
                    throw new InvalidOperationException("Você não pode agendar seu próprio serviço.");
                if (!r.IsDBNull(1) && r.GetDateTime(1) > DateTime.Now)
                    throw new InvalidOperationException("Você está bloqueado temporariamente.");
            }
            if ((ag.Descricao?.Length ?? 0) > 255 || (ag.EnderecoCliente?.Length ?? 0) > 255)
                throw new InvalidOperationException("Descrição ou endereço muito longo.");
            ag.LocalId = modalidade == "Local" ? local : null;
            if (modalidade == "Local")
            {
                using var c = Comando("SELECT COUNT(*) FROM Locais WHERE Id=@LocalId AND ProfissionalId=@ProfissionalId");
                c.Parameters.AddWithValue("@LocalId", (object?)local ?? DBNull.Value);
                if ((int)c.ExecuteScalar() != 1) throw new InvalidOperationException("Local inválido.");
            }
            if (modalidade == "Domicilio" && string.IsNullOrWhiteSpace(ag.EnderecoCliente))
                throw new InvalidOperationException("Informe o endereço de atendimento.");
            if (modalidade != "Domicilio") ag.EnderecoCliente = null;

            var regras = new List<Disponibilidade>();
            using (var c = Comando(@"SELECT DiaSemana, HoraInicio, HoraFim FROM Disponibilidade
                WHERE ServicoId=@ServicoId AND ProfissionalId=@ProfissionalId AND Ativo=1"))
            using (var r = c.ExecuteReader())
                while (r.Read()) regras.Add(new Disponibilidade {
                    DiaSemana=r.GetInt32(0), HoraInicio=r.GetTimeSpan(1), HoraFim=r.GetTimeSpan(2), Ativo=true });
            if (!RegrasAgenda.Horarios(regras, ag.Data).Contains(inicio))
                throw new InvalidOperationException("Horário fora da disponibilidade.");

            using (var c = Comando(@"SELECT Data, HoraInicio, HoraFim FROM BloqueiosAgenda
                WHERE ProfissionalId=@ProfissionalId AND Data BETWEEN DATEADD(day,-1,@Data) AND DATEADD(day,1,@Data)"))
            using (var r = c.ExecuteReader())
                while (r.Read())
                {
                    var bInicio = r.GetDateTime(0).Date + r.GetTimeSpan(1);
                    var bFim = r.GetDateTime(0).Date + r.GetTimeSpan(2);
                    if (bFim <= bInicio) bFim = bFim.AddDays(1);
                    if (RegrasAgenda.Sobrepoe(inicio, inicio.AddHours(1), bInicio, bFim))
                        throw new InvalidOperationException("Horário bloqueado pelo profissional.");
                }

            using (var c = Comando(@"SELECT Data, Hora FROM Agendamentos
                WHERE ProfissionalId=@ProfissionalId
                AND Data BETWEEN DATEADD(day,-1,@Data) AND DATEADD(day,1,@Data)
                AND Status NOT IN ('Cancelado','CanceladoCliente','CanceladoProfissional')"))
            using (var r = c.ExecuteReader())
                while (r.Read())
                {
                    var outro = r.GetDateTime(0).Date + r.GetTimeSpan(1);
                    if (RegrasAgenda.Sobrepoe(inicio, inicio.AddHours(1), outro, outro.AddHours(1)))
                        throw new InvalidOperationException("O profissional já possui atendimento neste horário.");
                }

            using (var c = Comando("SELECT COUNT(*) FROM Agendamentos WHERE ClienteId=@ClienteId AND Status='Pendente'"))
                if ((int)c.ExecuteScalar() >= 5) throw new InvalidOperationException("Você já possui cinco pedidos pendentes.");
            if (plano != "Premium")
            {
                using var c = Comando(@"SELECT COUNT(*) FROM Agendamentos WHERE ProfissionalId=@ProfissionalId
                    AND Data>=@InicioSemana AND Data<DATEADD(day,7,@InicioSemana)
                    AND Status NOT IN ('Cancelado','CanceladoCliente','CanceladoProfissional')");
                c.Parameters.AddWithValue("@InicioSemana", ag.Data.Date.AddDays(-(int)ag.Data.DayOfWeek));
                if ((int)c.ExecuteScalar() >= 3)
                    throw new InvalidOperationException("Limite semanal do plano gratuito atingido.");
            }
            using var inserir = Comando(@"INSERT INTO Agendamentos
                (ClienteId,ServicoId,ProfissionalId,Data,Hora,Status,Descricao,EnderecoCliente,LocalId)
                OUTPUT INSERTED.Id
                VALUES (@ClienteId,@ServicoId,@ProfissionalId,@Data,@Hora,'Pendente',@Descricao,@Endereco,@Local)");
            inserir.Parameters.AddWithValue("@Hora", ag.Hora);
            inserir.Parameters.AddWithValue("@Descricao", ag.Descricao ?? "");
            inserir.Parameters.AddWithValue("@Endereco", (object?)ag.EnderecoCliente ?? DBNull.Value);
            inserir.Parameters.AddWithValue("@Local", (object?)ag.LocalId ?? DBNull.Value);
            int id = (int)inserir.ExecuteScalar();
            foreach (var usuario in new[] { usuarioCliente, usuarioProfissional })
            {
                using var n = new SqlCommand(@"INSERT INTO Notificacoes
                    (UsuarioId,Titulo,Mensagem,Tipo,ReferenciaId) VALUES
                    (@Usuario,'Novo agendamento','Pedido enviado para confirmação do profissional.','Agendamento',@Id)", conn, tx);
                n.Parameters.AddWithValue("@Usuario", usuario);
                n.Parameters.AddWithValue("@Id", id);
                n.ExecuteNonQuery();
            }
            tx.Commit();
            return id;
        }
        public bool HorarioOcupado(int servicoId, DateTime data, TimeSpan hora)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                SELECT COUNT(*) 
                FROM Agendamentos
                WHERE ProfissionalId = (SELECT ProfissionalId FROM Servicos WHERE Id=@ServicoId)
                AND Data = @Data
                AND Hora = @Hora
                AND Status NOT IN ('Cancelado','CanceladoCliente','CanceladoProfissional')";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ServicoId", servicoId);
                cmd.Parameters.AddWithValue("@Data", data);
                cmd.Parameters.AddWithValue("@Hora", hora);

                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }



        public List<TimeSpan> BuscarHorariosOcupados(int servicoId, DateTime data)
        {
            List<TimeSpan> lista = new List<TimeSpan>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
                SELECT Hora 
                FROM Agendamentos
                WHERE ProfissionalId = (SELECT ProfissionalId FROM Servicos WHERE Id=@ServicoId)
                AND Data = @Data
                AND Status NOT IN ('Cancelado','CanceladoCliente','CanceladoProfissional')";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ServicoId", servicoId);
                cmd.Parameters.AddWithValue("@Data", data);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add((TimeSpan)reader["Hora"]);
                }
            }

            return lista;
        }


        public List<Agendamento> ListarPorCliente(int clienteId, int usuarioId)
        {
            List<Agendamento> lista = new List<Agendamento>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            SELECT 
                A.Id,
                A.ClienteId,
                A.ProfissionalId,
                A.Data,
                A.Hora,
                A.Status,
                A.ConfirmadoProfissional,
                A.FinalizadoProfissional,
                A.ConfirmadoCliente,
                A.Descricao,
                A.EnderecoCliente,
                A.LocalId,
                

                U.Nome AS ProfissionalNome,
                P.Contato AS ContatoProfissional,
                U.Telefone AS TelefoneProfissional,
                S.Nome AS Servico,
                SC.Nome AS Subcategoria,
                S.LinkOnline,
                S.Atendimento,
                S.TipoPreco,
                S.PrecoBase,
                L.Endereco AS EnderecoLocal,

                CASE 
                    WHEN av.Id IS NOT NULL THEN 1 
                    ELSE 0 
                END AS JaAvaliado

            FROM Agendamentos a
            INNER JOIN Servicos s ON a.ServicoId = s.Id
            INNER JOIN Profissionais p ON s.ProfissionalId = p.Id
            INNER JOIN Usuarios u ON p.UsuarioId = u.Id
            LEFT JOIN Subcategorias SC ON S.SubcategoriaId = SC.Id
            LEFT JOIN Locais l ON a.LocalId = l.Id 

            LEFT JOIN Avaliacoes av 
                ON av.AgendamentoId = a.Id 
                AND av.UsuarioId = @UsuarioId

            WHERE a.ClienteId = @ClienteId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

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
                        Subcategoria = reader["Subcategoria"] != DBNull.Value
                        ? reader["Subcategoria"].ToString()
                        : "",
                        Atendimento = reader["Atendimento"].ToString(),
                        LinkOnline = Seguranca.UrlHttpValida(reader["LinkOnline"].ToString())
                            ? reader["LinkOnline"].ToString() : null,
                        Data = (DateTime)reader["Data"],
                        Hora = (TimeSpan)reader["Hora"],
                        Status = reader["Status"].ToString(),

                        ConfirmadoProfissional = reader["ConfirmadoProfissional"] != DBNull.Value && (bool)reader["ConfirmadoProfissional"],
                        FinalizadoProfissional = reader["FinalizadoProfissional"] != DBNull.Value && (bool)reader["FinalizadoProfissional"],
                        ConfirmadoCliente = reader["ConfirmadoCliente"] != DBNull.Value && (bool)reader["ConfirmadoCliente"],

                        Descricao = reader["Descricao"] != DBNull.Value
                            ? reader["Descricao"].ToString() : "",

                        EnderecoCliente = reader["EnderecoCliente"] != DBNull.Value
                            ? reader["EnderecoCliente"].ToString() : "",

                        EnderecoLocal = reader["EnderecoLocal"] != DBNull.Value
                            ? reader["EnderecoLocal"].ToString() : "",

                        ContatoProfissional =
                            reader["ContatoProfissional"] != DBNull.Value
                            ? reader["ContatoProfissional"].ToString()
                            : reader["TelefoneProfissional"]?.ToString(),

                        JaAvaliado = reader["JaAvaliado"] != DBNull.Value && (int)reader["JaAvaliado"] == 1,
                        TipoPreco = reader["TipoPreco"].ToString(),

                        PrecoBase = reader["PrecoBase"] != DBNull.Value
                        ? Convert.ToDecimal(reader["PrecoBase"])
                        : null,
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
                string query = @"
                SELECT 
                    A.*,
                    U.Nome AS NomeCliente,
                    P.Plano AS PlanoProfissional,
                    U.Telefone AS ContatoCliente,
                    S.Nome AS Servico, 
                    S.LinkOnline,
                    S.Atendimento,
                    S.TipoPreco,
                    S.PrecoBase,
                    L.Endereco AS EnderecoLocal,
                    SC.Nome AS Subcategoria

                FROM Agendamentos A

                INNER JOIN Clientes C 
                    ON A.ClienteId = C.Id

                INNER JOIN Usuarios U 
                    ON C.UsuarioId = U.Id

                INNER JOIN Profissionais P
                    ON A.ProfissionalId = P.Id

                INNER JOIN Servicos S 
                    ON A.ServicoId = S.Id 

                INNER JOIN Subcategorias SC 
                    ON S.SubcategoriaId = SC.Id

                LEFT JOIN Locais L 
                    ON A.LocalId = L.Id

                WHERE A.ProfissionalId = @ProfissionalId";

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


                        ConfirmadoProfissional = reader["ConfirmadoProfissional"] != DBNull.Value && (bool)reader["ConfirmadoProfissional"],
                        FinalizadoProfissional = reader["FinalizadoProfissional"] != DBNull.Value && (bool)reader["FinalizadoProfissional"],
                        ConfirmadoCliente = reader["ConfirmadoCliente"] != DBNull.Value && (bool)reader["ConfirmadoCliente"],

                        Descricao = reader["Descricao"] != DBNull.Value
                            ? reader["Descricao"].ToString()
                            : "",
                        Servico = reader["Servico"].ToString(),
                        Subcategoria = reader["Subcategoria"] != DBNull.Value
                            ? reader["Subcategoria"].ToString()
                            : "",
                        EnderecoCliente = reader["EnderecoCliente"] != DBNull.Value
                            ? reader["EnderecoCliente"].ToString()
                            : "",

                        ContatoCliente =
                            reader["ContatoCliente"] != DBNull.Value
                            ? reader["ContatoCliente"].ToString()
                            : "",

                        LinkOnline = Seguranca.UrlHttpValida(reader["LinkOnline"].ToString())
                            ? reader["LinkOnline"].ToString()
                            : null,

                        Atendimento = reader["Atendimento"].ToString(),

                        EnderecoLocal = reader["EnderecoLocal"] != DBNull.Value
                        ? reader["EnderecoLocal"].ToString()
                        : "",

                        TipoPreco = reader["TipoPreco"].ToString(),

                        PrecoBase = reader["PrecoBase"] != DBNull.Value
                        ? Convert.ToDecimal(reader["PrecoBase"])
                        : null,

                        PlanoProfissional = reader["PlanoProfissional"].ToString(),
                    });
                }
            }

            return lista;
        }



        public Agendamento BuscarPorId(int id)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"SELECT 
                    A.*,
                    C.UsuarioId,
                    S.Atendimento,
                    S.TipoPreco,
                    S.PrecoBase,
                    P.Contato AS ContatoProfissional,
                    UProf.Telefone AS TelefoneProfissional,
                    UCli.Telefone AS ContatoCliente,
                    S.LinkOnline,
                    L.Endereco AS EnderecoLocal
                FROM Agendamentos A
              
                    LEFT JOIN Clientes C 
                        ON A.ClienteId = C.Id

                    LEFT JOIN Usuarios UCli 
                        ON C.UsuarioId = UCli.Id

                    LEFT JOIN Profissionais P 
                        ON A.ProfissionalId = P.Id

                    LEFT JOIN Usuarios UProf 
                        ON P.UsuarioId = UProf.Id

                    INNER JOIN Servicos S 
                        ON A.ServicoId = S.Id

                    LEFT JOIN Locais L 
                        ON A.LocalId = L.Id
                WHERE A.Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Agendamento
                    {
                        Id = (int)reader["Id"],
                        ClienteId = (int)reader["ClienteId"],
                        ServicoId = (int)reader["ServicoId"],
                        UsuarioId = reader["UsuarioId"] != DBNull.Value
                        ? (int)reader["UsuarioId"]
                        : 0,
                        ProfissionalId = (int)reader["ProfissionalId"],
                        Data = (DateTime)reader["Data"],
                        Hora = (TimeSpan)reader["Hora"],
                        Status = reader["Status"].ToString(),

                        ContatoProfissional =
                            reader["ContatoProfissional"] != DBNull.Value
                            ? reader["ContatoProfissional"].ToString()
                            : reader["TelefoneProfissional"]?.ToString(),

                        ContatoCliente =
                            reader["ContatoCliente"] != DBNull.Value
                            ? reader["ContatoCliente"].ToString()
                            : "",

                        ConfirmadoProfissional = (bool)reader["ConfirmadoProfissional"],
                        FinalizadoProfissional = (bool)reader["FinalizadoProfissional"],
                        ConfirmadoCliente = (bool)reader["ConfirmadoCliente"],

                        Descricao = reader["Descricao"] != DBNull.Value
                        ? reader["Descricao"].ToString()
                        : "",

                        EnderecoCliente = reader["EnderecoCliente"] != DBNull.Value
                        ? reader["EnderecoCliente"].ToString()
                        : "",

                        TipoPreco = reader["TipoPreco"].ToString(),

                        PrecoBase = reader["PrecoBase"] != DBNull.Value
                        ? Convert.ToDecimal(reader["PrecoBase"])
                        : null,
                    };
                }
            }

            return null;
        }

        public bool Confirmar(int id, int profissionalId)
        {
            using var conn = conexao.Conectar();
            using var cmd = new SqlCommand(@"UPDATE Agendamentos SET Status='Confirmado', ConfirmadoProfissional=1
                WHERE Id=@Id AND ProfissionalId=@ProfissionalId AND Status='Pendente' AND FinalizadoProfissional=0", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);
            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Cancelar(int id, string status, int usuarioId)
        {
            using var conn = conexao.Conectar();
            using var cmd = new SqlCommand(@"UPDATE a SET Status=@Status, DataCancelamento=SYSDATETIME()
                FROM Agendamentos a
                WHERE a.Id=@Id AND a.Status IN ('Pendente','Confirmado') AND a.FinalizadoProfissional=0
                AND ((@Status='CanceladoCliente' AND EXISTS(SELECT 1 FROM Clientes c WHERE c.Id=a.ClienteId AND c.UsuarioId=@Usuario))
                  OR (@Status='CanceladoProfissional' AND EXISTS(SELECT 1 FROM Profissionais p WHERE p.Id=a.ProfissionalId AND p.UsuarioId=@Usuario)))", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Usuario", usuarioId);
            return cmd.ExecuteNonQuery() == 1;
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



        public bool Finalizar(int id, int profissionalId, decimal valorFinal, decimal taxa, decimal valorLiquido)
        {
            using var conn = conexao.Conectar();
            using var cmd = new SqlCommand(@"UPDATE Agendamentos SET Status='AguardandoCliente',
                FinalizadoProfissional=1, ValorFinal=@Valor, Taxa=@Taxa, ValorLiquido=@Liquido
                WHERE Id=@Id AND ProfissionalId=@Profissional AND Status='Confirmado'
                    AND ConfirmadoProfissional=1 AND FinalizadoProfissional=0", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Profissional", profissionalId);
            cmd.Parameters.AddWithValue("@Valor", valorFinal);
            cmd.Parameters.AddWithValue("@Taxa", taxa);
            cmd.Parameters.AddWithValue("@Liquido", valorLiquido);
            return cmd.ExecuteNonQuery() == 1;
        }

        public bool ConfirmarCliente(int id, int usuarioId)
        {
            using var conn = conexao.Conectar();
            using var cmd = new SqlCommand(@"UPDATE a SET ConfirmadoCliente=1, Status='Finalizado'
                FROM Agendamentos a JOIN Clientes c ON c.Id=a.ClienteId
                WHERE a.Id=@Id AND c.UsuarioId=@Usuario AND a.FinalizadoProfissional=1
                  AND a.ConfirmadoProfissional=1 AND a.ConfirmadoCliente=0
                  AND a.Status IN ('AguardandoCliente','Finalizado')", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Usuario", usuarioId);
            return cmd.ExecuteNonQuery() == 1;
        }
        // METODO CONTA PREMIUIM

        public int ContarAgendamentosSemana(int profissionalId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                DateTime inicioSemana = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

                DateTime fimSemana = inicioSemana.AddDays(7);

                string query = @"
            SELECT COUNT(*)
            FROM Agendamentos
            WHERE ProfissionalId = @ProfissionalId
            AND Data >= @InicioSemana
            AND Data < @FimSemana
            AND Status NOT IN ('Cancelado','CanceladoCliente','CanceladoProfissional')";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);
                cmd.Parameters.AddWithValue("@InicioSemana", inicioSemana);
                cmd.Parameters.AddWithValue("@FimSemana", fimSemana);

                return (int)cmd.ExecuteScalar();
            }
        }

        // DASHBOARD ,calculo direto do banco
        public (decimal faturamento, decimal taxas, decimal liquido) DashboardFinanceiro(int profissionalId)

        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            SELECT
                ISNULL(SUM(ValorFinal), 0) AS Faturamento,
                ISNULL(SUM(Taxa), 0) AS Taxas,
                ISNULL(SUM(ValorFinal - Taxa), 0) AS Liquido
            FROM Agendamentos
            WHERE ProfissionalId = @ProfissionalId
            AND ConfirmadoCliente = 1
            AND FinalizadoProfissional = 1
                ";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    decimal faturamento =
                        Convert.ToDecimal(reader["Faturamento"]);

                    decimal taxas =
                        Convert.ToDecimal(reader["Taxas"]);

                    decimal liquido =
                        Convert.ToDecimal(reader["Liquido"]);

                    return (faturamento, taxas, liquido);
                }
            }

            return (0, 0, 0);
        }

        // Cards dinamicos dos ultimos pagamentos feitos
        public List<Agendamento> UltimosPagamentos(int profissionalId)
        {
            List<Agendamento> lista = new List<Agendamento>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            SELECT TOP 5
                A.ValorFinal,
                A.Data,
                A.Hora,

                S.Nome AS Servico,

                U.Nome AS NomeCliente

            FROM Agendamentos A

            INNER JOIN Servicos S
                ON A.ServicoId = S.Id

            INNER JOIN Clientes C
                ON A.ClienteId = C.Id

            INNER JOIN Usuarios U
                ON C.UsuarioId = U.Id

            WHERE A.ProfissionalId = @ProfissionalId
            AND A.ConfirmadoCliente = 1
            AND A.FinalizadoProfissional = 1

            ORDER BY A.Data DESC ";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Agendamento
                    {
                        Servico = reader["Servico"].ToString(),

                        NomeCliente = reader["NomeCliente"].ToString(),

                        Data = Convert.ToDateTime(reader["Data"]),

                        Hora = (TimeSpan)reader["Hora"],

                        ValorFinal = Convert.ToDecimal(reader["ValorFinal"])
                    });
                }
            }

            return lista;
        }

        public int ContarAgendamentos(int profissionalId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            SELECT COUNT(*)
            FROM Agendamentos
            WHERE ProfissionalId = @ProfissionalId
        ";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                return (int)cmd.ExecuteScalar();
            }
        }

        //PARA EXIBIÇÃO  NO PAINEL
        public int ContarPendentesProfissional(int profissionalId)
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT COUNT(*) 
        FROM Agendamentos 
        WHERE ProfissionalId = @ProfissionalId 
        AND Status = 'Pendente'";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@ProfissionalId", profissionalId);

                return (int)cmd.ExecuteScalar();
            }
        }





    }
}