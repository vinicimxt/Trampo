using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

public static class Seguranca
{
    // Apenas o hasher padrão; preserva a autenticação MVC por sessão.
    private static readonly PasswordHasher<object> Hasher = new(Options.Create(
        new PasswordHasherOptions { IterationCount = 210_000 }));
    private static readonly object Usuario = new();

    public static string GerarHash(string senha) => Hasher.HashPassword(Usuario, senha);

    public static bool Verificar(string hash, string senha, out bool atualizar)
    {
        atualizar = false;
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(senha)) return false;
        if (hash.Length == 64 && hash.All(Uri.IsHexDigit))
        {
            bool correto = CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(hash), SHA256.HashData(Encoding.UTF8.GetBytes(senha)));
            atualizar = correto;
            return correto;
        }
        try
        {
            var resultado = Hasher.VerifyHashedPassword(Usuario, hash, senha);
            atualizar = resultado == PasswordVerificationResult.SuccessRehashNeeded;
            return resultado != PasswordVerificationResult.Failed;
        }
        catch (FormatException) { return false; }
    }

    public static bool UrlHttpValida(string? valor) =>
        Uri.TryCreate(valor, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}