using Microsoft.Data.SqlClient;
namespace BD_TRAMPO.DAO;

public class LocalDAO
{
    private readonly Conexao conexao = new();
    private static Local Ler(SqlDataReader r) => new() {
        Id=r.GetInt32(r.GetOrdinal("Id")),ProfissionalId=r.GetInt32(r.GetOrdinal("ProfissionalId")),
        Nome=r.IsDBNull(r.GetOrdinal("Nome"))?"":r.GetString(r.GetOrdinal("Nome")),
        Endereco=r.GetString(r.GetOrdinal("Endereco")),
        DadosEndereco=EnderecoSql.Ler(r,r.GetString(r.GetOrdinal("Endereco")))
    };
    private static void Parametros(SqlCommand cmd,Local l)
    {
        cmd.Parameters.AddWithValue("@Id",l.Id);
        cmd.Parameters.AddWithValue("@P",l.ProfissionalId);
        cmd.Parameters.AddWithValue("@Nome",l.Nome??"");
        cmd.Parameters.AddWithValue("@Endereco",l.Endereco);
        EnderecoSql.Parametros(cmd,l.DadosEndereco);
    }
    public int Inserir(Local l)
    {
        using var c=conexao.Conectar();
        using var cmd=new SqlCommand(@"INSERT INTO Locais(ProfissionalId,Nome,Endereco,CEP,Logradouro,Numero,
            Complemento,Bairro,Cidade,UF,Latitude,Longitude,EnderecoEstruturado)
            OUTPUT INSERTED.Id VALUES(@P,@Nome,@Endereco,@CEP,@Logradouro,@Numero,@Complemento,@Bairro,@Cidade,@UF,@Latitude,@Longitude,@Estruturado)",c);
        Parametros(cmd,l);return Convert.ToInt32(cmd.ExecuteScalar());
    }
    private static void Bloquear(SqlConnection c,SqlTransaction tx,int profissional)
    {
        using var cmd=new SqlCommand(@"DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@Recurso,
            @LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
            IF @r<0 THROW 51001,'Agenda em atualização.',1;",c,tx);
        cmd.Parameters.AddWithValue("@Recurso","TRAMPO:Profissional:"+profissional);
        FalhasSql.Executar(()=>cmd.ExecuteNonQuery());
    }
    public bool Atualizar(Local l)
    {
        var inicial=BuscarPorId(l.Id);if(inicial==null) return false;
        using var c=conexao.Conectar();using var tx=c.BeginTransaction(System.Data.IsolationLevel.Serializable);
        Bloquear(c,tx,inicial.ProfissionalId);
        Local original;
        using(var buscar=new SqlCommand("SELECT * FROM Locais WITH(UPDLOCK,HOLDLOCK) WHERE Id=@Id",c,tx)) {
            buscar.Parameters.AddWithValue("@Id",l.Id);using var r=buscar.ExecuteReader();
            if(!r.Read())return false;original=Ler(r);
        }
        if(l.ProfissionalId!=0 && l.ProfissionalId!=original.ProfissionalId)return false;
        using(var historico=new SqlCommand("SELECT COUNT(*) FROM Agendamentos WHERE LocalId=@Id",c,tx)) {
            historico.Parameters.AddWithValue("@Id",l.Id);
            bool mudou=original.Nome!=(l.Nome??"") || original.Endereco!=l.Endereco ||
                (original.DadosEndereco?.Estruturado==true || l.DadosEndereco?.Estruturado==true) && original.DadosEndereco!=l.DadosEndereco;
            if(Convert.ToInt32(historico.ExecuteScalar())>0 && mudou)return false;
        }
        l.ProfissionalId=original.ProfissionalId;
        using var cmd=new SqlCommand(@"UPDATE Locais SET Nome=@Nome,Endereco=@Endereco,CEP=@CEP,Logradouro=@Logradouro,
            Numero=@Numero,Complemento=@Complemento,Bairro=@Bairro,Cidade=@Cidade,UF=@UF,Latitude=@Latitude,
            Longitude=@Longitude,EnderecoEstruturado=@Estruturado WHERE Id=@Id AND ProfissionalId=@P",c,tx);
        Parametros(cmd,l);bool alterou=cmd.ExecuteNonQuery()==1;tx.Commit();return alterou;
    }
    public bool Excluir(int id)
    {
        var local=BuscarPorId(id);if(local==null)return false;
        using var c=conexao.Conectar();using var tx=c.BeginTransaction(System.Data.IsolationLevel.Serializable);
        Bloquear(c,tx,local.ProfissionalId);
        using var cmd=new SqlCommand(@"IF EXISTS(SELECT 1 FROM Servicos WHERE LocalId=@Id) OR
            EXISTS(SELECT 1 FROM Agendamentos WHERE LocalId=@Id) SELECT 0;
            ELSE BEGIN DELETE FROM Locais WHERE Id=@Id; SELECT @@ROWCOUNT; END",c,tx);
        cmd.Parameters.AddWithValue("@Id",id);bool removido=Convert.ToInt32(cmd.ExecuteScalar())==1;tx.Commit();return removido;
    }
    public Local? BuscarPorId(int id)
    {
        using var c=conexao.Conectar();using var cmd=new SqlCommand("SELECT * FROM Locais WHERE Id=@Id",c);
        cmd.Parameters.AddWithValue("@Id",id);using var r=cmd.ExecuteReader();return r.Read()?Ler(r):null;
    }
    public List<Local> ListarPorProfissional(int profissionalId)
    {
        using var c=conexao.Conectar();using var cmd=new SqlCommand("SELECT * FROM Locais WHERE ProfissionalId=@P ORDER BY Id",c);
        cmd.Parameters.AddWithValue("@P",profissionalId);using var r=cmd.ExecuteReader();var lista=new List<Local>();
        while(r.Read())lista.Add(Ler(r));return lista;
    }
}
