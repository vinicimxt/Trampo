// using BD_TRAMPO.Models;
// using Microsoft.Data.SqlClient;

// namespace BD_TRAMPO.DAO
// {
//     public class BloqueioAgendaDAO
//     {
//         Conexao conexao = new Conexao();

//         public List<BloqueioAgenda> ListarPorData(int profissionalId, DateTime data)
//         {
//             List<BloqueioAgenda> lista = new List<BloqueioAgenda>();

//             using (SqlConnection conn = conexao.Conectar())
//             {
//                 string sql = @"
//                     SELECT * FROM BloqueiosAgenda
//                     WHERE ProfissionalId = @id
//                     AND Data = @data";

//                 SqlCommand cmd = new SqlCommand(sql, conn);
//                 cmd.Parameters.AddWithValue("@id", profissionalId);
//                 cmd.Parameters.AddWithValue("@data", data.Date);

//                 SqlDataReader reader = cmd.ExecuteReader();

//                 while (reader.Read())
//                 {
//                     lista.Add(new BloqueioAgenda
//                     {
//                         Id = (int)reader["Id"],
//                         ProfissionalId = (int)reader["ProfissionalId"],
//                         Data = (DateTime)reader["Data"],
//                         HoraInicio = (TimeSpan)reader["HoraInicio"],
//                         HoraFim = (TimeSpan)reader["HoraFim"],
//                         Motivo = reader["Motivo"].ToString()
//                     });
//                 }
//             }

//             return lista;
//         }
//     }
// }