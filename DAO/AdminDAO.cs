using Microsoft.Data.SqlClient;
using BD_TRAMPO.Models;

namespace BD_TRAMPO.DAO
{
    public class AdminDAO
    {
        private Conexao conexao = new Conexao();

        public int TotalUsuarios()
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT COUNT(*) FROM Usuarios";

                SqlCommand cmd = new SqlCommand(query, conn);

                return (int)cmd.ExecuteScalar();
            }
        }

        public int TotalProfissionais()
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT COUNT(*) FROM Profissionais";

                SqlCommand cmd = new SqlCommand(query, conn);

                return (int)cmd.ExecuteScalar();
            }
        }

        public int TotalPremium()
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            SELECT COUNT(*)
            FROM Profissionais
            WHERE Plano = 'Premium'
        ";

                SqlCommand cmd = new SqlCommand(query, conn);

                return (int)cmd.ExecuteScalar();
            }
        }

        public int TotalAgendamentos()
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = "SELECT COUNT(*) FROM Agendamentos";

                SqlCommand cmd = new SqlCommand(query, conn);

                return (int)cmd.ExecuteScalar();
            }
        }


        public decimal TotalTaxas()
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            SELECT ISNULL(SUM(Taxa), 0)
            FROM Agendamentos
            WHERE ConfirmadoCliente = 1
            AND FinalizadoProfissional = 1
        ";

                SqlCommand cmd = new SqlCommand(query, conn);

                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }


        public decimal TotalReceitaPremium()
        {
            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
            SELECT ISNULL(SUM(Valor), 0)
            FROM Assinaturas
            WHERE StatusPagamento = 'Pago'
        ";

                SqlCommand cmd = new SqlCommand(query, conn);

                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        public decimal TotalLiquidoPlataforma()
        {
            return TotalTaxas() + TotalReceitaPremium();
        }

        public List<dynamic> UltimasAssinaturas()
        {
            List<dynamic> lista = new List<dynamic>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"
        SELECT TOP 5
            U.Nome,
            A.Plano,
            A.Valor,
            A.DataCriacao
        FROM Assinaturas A

        INNER JOIN Profissionais P
            ON A.ProfissionalId = P.Id

        INNER JOIN Usuarios U
            ON P.UsuarioId = U.Id

        WHERE A.StatusPagamento = 'Pago'

        ORDER BY A.DataCriacao DESC
        ";

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new
                    {
                        Nome = reader["Nome"].ToString(),
                        Plano = reader["Plano"].ToString(),
                        Valor = Convert.ToDecimal(reader["Valor"]),
                        Data = Convert.ToDateTime(reader["DataCriacao"])
                    });
                }
            }

            return lista;
        }

        public List<dynamic> UltimosPagamentos()
        {
            List<dynamic> lista = new List<dynamic>();

            using (SqlConnection conn = conexao.Conectar())
            {
                string query = @"

        SELECT TOP 5

            U.Nome AS NomeCliente,
            S.Nome AS Servico,

            A.ValorFinal,
            A.Taxa,
            A.Data

        FROM Agendamentos A

        INNER JOIN Usuarios U
            ON A.ClienteId = U.Id

        INNER JOIN Servicos S
            ON A.ServicoId = S.Id

        WHERE A.ConfirmadoCliente = 1
        AND A.FinalizadoProfissional = 1

        ORDER BY A.Data DESC
        ";

                SqlCommand cmd = new SqlCommand(query, conn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new
                    {
                        NomeCliente = reader["NomeCliente"].ToString(),
                        Servico = reader["Servico"].ToString(),

                        ValorFinal =
                            Convert.ToDecimal(reader["ValorFinal"]),

                        Taxa =
                            Convert.ToDecimal(reader["Taxa"]),

                        Data =
                            Convert.ToDateTime(reader["Data"])
                    });
                }
            }

            return lista;
        }



    }



}
