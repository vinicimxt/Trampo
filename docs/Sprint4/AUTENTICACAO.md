# Autenticação — Sprint 4

MVC → sessão existente. API → JWT Bearer pelo mecanismo padrão ASP.NET Core. API Controllers herdam ControllerBase, não BaseController; não criam sessão. A dispensa de antiforgery é limitada aos Controllers API, que não aceitam sessão/cookies como autenticação. O filtro antiforgery global continua protegendo o MVC.

## Senhas e identidade

AutenticacaoService.Entrar delega a UsuarioDAO.BuscarLogin, o mesmo mecanismo usado no login MVC. Seguranca.Verificar mantém hash moderno, compatibilidade SHA-256 e migração automática do hash no login correto. Não existe uma segunda implementação de senha.

Claims: sub (ID), role (tipo normalizado), jti (identificador), nbf/exp, iss/aud. Sem nome/email/senha/hash/documento/telefone no token. ClaimsPrincipal é convertido por UsuarioClaims em UsuarioContexto; Services não recebem ClaimsPrincipal ou HttpContext.

Em cada validação Bearer, o usuário é consultado no banco. Usuário removido ou com perfil diferente do claim invalida o token. Services continuam verificando propriedade e perfil persistido; possuir um ID ou um token de profissional não concede acesso aos recursos alheios.

## Assinatura, configuração e expiração

HS256; chave aleatória de pelo menos 32 bytes, codificada em Base64, fornecida por `Jwt__SigningKey`. Issuer `Trampo`, audience `Trampo.Api`, expiração padrão 15 minutos (configurável entre 1 e 60), tolerância de relógio de 30 segundos. O validador exige assinatura, algoritmo HS256, issuer/audience corretos e expiração.

appsettings.json contém somente Issuer, Audience e ExpirationMinutes. Não há chave padrão, chave real em arquivo nem geração silenciosa de segredo persistente. Sem chave, o MVC e catálogo público continuam disponíveis, login API responde 503 e Bearer não aceita tokens. Chave malformada ou configuração inválida interrompe a inicialização sem imprimir a chave.

Para desenvolvimento em PowerShell, sem imprimir nem gravar o segredo:

```powershell
$bytesJwt = New-Object byte[] 32
$geradorJwt = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$geradorJwt.GetBytes($bytesJwt)
$geradorJwt.Dispose()
$env:Jwt__SigningKey = [Convert]::ToBase64String($bytesJwt)
dotnet run --launch-profile https
```

A variável existe somente nessa sessão do terminal e processos filhos. Reiniciar com outra chave invalida os tokens anteriores. Para remover após encerrar a aplicação: `Remove-Item Env:Jwt__SigningKey`. Em implantação futura, usar configuração secreta administrada, restringir acesso e nunca registrar a variável em logs. Não foi criada infraestrutura de implantação nesta Sprint.

HTTPS deve ser usado por clientes externos. HTTP é utilizado somente nos testes de loopback. O perfil https existente usa certificado de desenvolvimento; configurar a confiança local pelo fluxo normal do .NET se necessário.

## Refresh, logout e revogação

Esta V1 tem somente access token. Ao expirar, o usuário autentica novamente. O cliente realiza logout descartando o token; logout MVC limpa apenas sua sessão e não revoga JWT. Não há endpoint de refresh nem tabela de tokens.

Mudança de perfil/remoção de usuário é observada na próxima requisição. Troca de senha, por si só, ainda não revoga um JWT já emitido; ele pode durar até a expiração mais a tolerância. Comprometimento da chave exige rotação, invalidando todos os tokens assinados com a chave anterior. Não há blacklist por token nesta V1.

Antes do Mobile final: definir refresh token com armazenamento de hash, expiração, rotação, detecção de reutilização, revogação por dispositivo, logout e resposta a comprometimento. Se houver persistência, criar migração incremental; nada disso foi improvisado nesta Sprint. No cliente Mobile, usar armazenamento seguro do sistema operacional, nunca logs ou armazenamento público.

## Limitação de tentativas e CORS

Rate limiter nativo, somente POST /api/v1/auth/login: dez tentativas por minuto por IP remoto, janela fixa, sem fila, 429 com Retry-After. Testado sem afetar login MVC. O limite é local ao processo e reinicia com ele; antes de múltiplas instâncias será necessário definir estratégia compartilhada. Proxies confiáveis e forwarded headers não foram configurados: atrás de proxy, o IP pode ser o do proxy. Não confiar em X-Forwarded-For arbitrário.

Nenhuma política CORS foi adicionada: não há consumidor Web em outro domínio autorizado nesta Sprint, e Mobile nativo não requer CORS. Swagger da mesma origem funciona com Bearer. Se surgir um consumidor Web, configurar origens específicas conforme necessidade.

## Referências de implementação

- [JWT Bearer no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0).
- [Rate limiting nativo](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0).
- [Transformadores OpenAPI](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-10.0).
