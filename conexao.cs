using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BD_TRAMPO;

public class Conexao
{
    private static IConfiguration? configuracao;
    private readonly string stringConexao;

    // Ponte de compatibilidade: os DAOs existentes criam Conexao diretamente.
    // O bootstrap configura uma unica aplicacao por processo, antes de atender requests.
    public static void Configurar(IConfiguration configuration)
    {
        _ = new Conexao(configuration); // Valida sem conectar e sem expor o valor.
        configuracao = configuration;
    }

    public Conexao() : this(configuracao ??
        throw new InvalidOperationException("A configuração do banco não foi inicializada."))
    {
    }

    public Conexao(IConfiguration configuration)
    {
        var valor = configuration.GetConnectionString("Xamou");
        if (string.IsNullOrWhiteSpace(valor))
            throw new InvalidOperationException("Configure ConnectionStrings:Xamou para este ambiente.");
        try
        {
            var dados = new SqlConnectionStringBuilder(valor);
            if (string.IsNullOrWhiteSpace(dados.DataSource) || string.IsNullOrWhiteSpace(dados.InitialCatalog))
                throw new ArgumentException();
            stringConexao = dados.ConnectionString;
        }
        catch (ArgumentException)
        {
            // Erros de parsing podem incluir valores fornecidos: nao propagamos a mensagem original.
            throw new InvalidOperationException("ConnectionStrings:Xamou inválida: informe servidor e banco.");
        }
    }

    public SqlConnection Conectar()
    {
        var conn = new SqlConnection(stringConexao);
        try
        {
            conn.Open();
            return conn;
        }
        catch (SqlException ex)
        {
            conn.Dispose();
            throw new InvalidOperationException($"Não foi possível conectar ao banco configurado. Código SQL: {ex.Number}.");
        }
        catch
        {
            conn.Dispose();
            throw;
        }
    }
}
