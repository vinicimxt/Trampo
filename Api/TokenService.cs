using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using BD_TRAMPO.Models;
using BD_TRAMPO.Api.Contracts;

namespace BD_TRAMPO.Api;

public sealed class JwtConfiguracao
{
    public string Issuer { get; set; } = "Trampo";
    public string Audience { get; set; } = "Trampo.Api";
    public int ExpirationMinutes { get; set; } = 15;
    public string? SigningKey { get; set; }

    public SymmetricSecurityKey? Chave()
    {
        if (string.IsNullOrWhiteSpace(SigningKey)) return null;
        byte[] bytes;
        try { bytes = Convert.FromBase64String(SigningKey); }
        catch (FormatException) { throw new InvalidOperationException("Jwt:SigningKey deve ser Base64 válido."); }
        if (bytes.Length < 32) throw new InvalidOperationException("Jwt:SigningKey deve ter ao menos 32 bytes.");
        return new(bytes);
    }
}

public sealed class TokenService(JwtConfiguracao configuracao)
{
    public LoginResponse Emitir(Usuario usuario)
    {
        var chave = configuracao.Chave() ?? throw new ApiIndisponivelException();
        var agora = DateTime.UtcNow;
        var expira = agora.AddMinutes(configuracao.ExpirationMinutes);
        var token = new JwtSecurityToken(configuracao.Issuer, configuracao.Audience,
            [new Claim("sub", usuario.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
             new Claim("role", usuario.Tipo.ToLowerInvariant()),
             new Claim("jti", Guid.NewGuid().ToString("N"))],
            agora, expira, new SigningCredentials(chave, SecurityAlgorithms.HmacSha256));
        return new(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", expira);
    }
}

public sealed class ApiIndisponivelException : Exception;
