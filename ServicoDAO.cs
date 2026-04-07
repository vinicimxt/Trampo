using Microsoft.Data.SqlClient;

namespace BD_TRAMPO{

public class ServicoDAO
{
    private Conexao conexao = new Conexao();

    public void Inserir(Servico s)
    {
        using (SqlConnection conn = conexao.Conectar())
        {
            string query = @"INSERT INTO Servicos 
                (ProfissionalId, Nome, Descricao, Contato)
                VALUES (@ProfissionalId, @Nome, @Descricao, @Contato)";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@ProfissionalId", s.ProfissionalId);
            cmd.Parameters.AddWithValue("@Nome", s.Nome);
            cmd.Parameters.AddWithValue("@Descricao", s.Descricao);
            cmd.Parameters.AddWithValue("@Contato", s.Contato);
         

            cmd.ExecuteNonQuery();
        }
    }

    public List<Servico> ListarPorProfissional(int profissionalId)
    {
        List<Servico> lista = new List<Servico>();

        using (SqlConnection conn = conexao.Conectar())
        {
            string query = "SELECT * FROM Servicos WHERE ProfissionalId = @Id";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", profissionalId);

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Servico
                {
                    Id = (int)reader["Id"],
                    ProfissionalId = (int)reader["ProfissionalId"],
                    Nome = reader["Nome"].ToString(),
                    Descricao = reader["Descricao"].ToString(),
                    Contato = reader ["Contato"].ToString()
                });
            }
        }

        return lista;
    }
}

}