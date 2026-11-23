# Auditoria para deploy acadêmico no Azure

Estado em 22/11/2026: **PREPARADO PARA DEPLOY**, sem assinatura/tier verificados e sem deploy. A auditoria cobre este repositório de backend. Android e Desktop são repositórios separados e seus fontes não estão aqui; as afirmações sobre eles abaixo vêm do README e da documentação existente, não de uma compilação nesta execução.

## O que já está pronto

- `BD-TRAMPO.csproj` usa ASP.NET Core MVC em `net10.0`; Web e REST `/api/v1/...` compartilham o processo. `Scripts/Test-CI.ps1` já produz publish Release e o workflow `.github/workflows/backend.yml` publica o artefato de CI. Não há CD com credenciais.
- `Conexao` lê `ConnectionStrings:Xamou` via `IConfiguration`; `appsettings.json` não contém conexão nem senha. O valor SQL Express está somente em `appsettings.Development.json`, excluído do publish.
- JWT usa configuração externa para `Jwt:SigningKey` (Base64 de pelo menos 32 bytes), valida emissor, público e assinatura, e não imprime a chave. A chave ausente ainda permite iniciar MVC e `/health`, mas impede emissão de JWT; login REST precisa ser testado separadamente.
- `Program.cs` usa `UseHttpsRedirection`, HSTS fora de Development, middleware de erro, `UseStaticFiles`, sessão, antiforgery e Swagger/OpenAPI apenas em Development. `/health` é anônimo e verifica apenas que o processo responde.
- Não foi encontrado `AllowAnyOrigin` nem configuração de CORS no backend. Clientes Android/WinForms nativos não exigem CORS de navegador.
- Não foi encontrado código de upload/gravação persistente em disco nos controllers, services, DAOs ou integrações auditados. `wwwroot` contém recursos estáticos do projeto; nenhum caminho `C:\Users\...` foi encontrado nessas fontes.
- Redirects MVC encontrados usam `RedirectToAction`, sem URL local fixa. O perfil Development em `Properties/launchSettings.json` usa `localhost:5165`.
- `_Layout.cshtml` também carrega Google Fonts, Font Awesome por CDN e VLibras de domínios públicos; conferir esses recursos na demonstração e registrar eventual bloqueio externo.

## O que funciona só localmente ou exige ajuste operacional

| Item | Estado e ação para Production |
| --- | --- |
| SQL | `appsettings.Development.json` usa `localhost\\SQLEXPRESS`, autenticação integrada e `TrustServerCertificate=True`. Configurar `ConnectionStrings__Xamou` no App Service com Azure SQL, `Encrypt=True` e certificado validado. |
| Banco inicial | `ScriptBD.txt` inicia com `USE master`, `CREATE DATABASE Xamou`, `USE Xamou`; `USE` não troca de banco no Azure SQL Database. Há blocos comentados de DROP/debug/admin. Usar `ScriptsSQL/000_azure_schema_catalogo.sql` apenas em banco novo vazio, seguido de `001_enderecos_geolocalizacao.sql`. |
| JWT | Fornecer chave Production nova pelo App Service Configuration. `/health` 200 não comprova JWT nem conexão SQL. |
| Ambiente | Fixar `ASPNETCORE_ENVIRONMENT=Production`; verificar que `DOTNET_ENVIRONMENT` não o contradiz. DeveloperExceptionPage e Swagger não devem aparecer. |
| HTTPS e proxy | Ativar HTTPS Only no App Service. Preferir App Service Windows/IIS para a integração automática de esquema encaminhado; testar redirects e antiforgery. Em Linux/proxy diferente, configurar forwarded headers **somente** após identificar proxies confiáveis, antes de HSTS/redirect. |
| Sessão | `AddDistributedMemoryCache` mantém sessão na memória do processo. Reinício perde sessões; F1 não escala horizontalmente. Verificar persistência das chaves Data Protection e teste de login após restart/deploy. |
| Assets | `UseStaticFiles` e `wwwroot` existem; CSS/JS/imagens/fontes e diferenças de caixa no caminho precisam de teste na URL real. |
| Android | README aponta emulador para `10.0.2.2:5165` e Release HTTPS por `trampoApiUrl`. O projeto Android não está neste checkout; confirmar Debug com parâmetro, rejeição de HTTP em Release e build antes de afirmar validação. |
| Desktop | README documenta `TRAMPO_API_BASE_URL`, padrão `localhost:5165`. O projeto Desktop não está neste checkout; testar cliente HTTPS após deploy. |
| Rede SQL | Firewall do servidor lógico precisa permitir apenas o acesso necessário. A opção geral “Allow Azure services” inclui serviços de outras assinaturas; não ativar sem decisão explícita. |

## Compatibilidade dos scripts SQL

O schema inicial usa tipos, `IDENTITY`, PK/FK/CHECK e índices T-SQL usuais. A migração 001 é aditiva, transacional e usa `sys.check_constraints`, `OBJECT_ID`, `COL_LENGTH` e DDL de banco. Não há dependência ativa de `xp_`, filesystem, filegroups ou SQL Agent nesses trechos. `sp_fkeys` aparece apenas em comentário histórico. A compatibilidade final depende da execução em um Azure SQL novo; ainda não foi comprovada remotamente.

`000_azure_schema_catalogo.sql` preserva as 14 tabelas, dois índices únicos explícitos e INSERTs de catálogo de `ScriptBD.txt`, removendo somente o preâmbulo de criação/seleção de banco e os blocos comentados finais. `azure_validacao.sql` consulta tabelas, FK, CHECK, índices, colunas da migração e totais do catálogo sem alterar dados. Nunca executar o bootstrap em banco existente.

## Busca dirigida

Termos auditados em fontes/configuração: `localhost`, `127.0.0.1`, `10.0.2.2`, `5165`, `SQLEXPRESS`, `MSSQLLocalDB`, `Trusted_Connection`, `ConnectionStrings`, `Jwt`, `SigningKey`, `ASPNETCORE_ENVIRONMENT`, `Development`, `Production`, `UseHttpsRedirection`, HSTS e CORS. `MSSQLLocalDB` não apareceu na configuração ativa. Ocorrências locais em testes, documentação e script de desenvolvimento não são URLs de Production.

Referências: [diferenças T-SQL do Azure SQL](https://learn.microsoft.com/azure/azure-sql/database/transact-sql-tsql-differences-sql-server), [proxy no ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0), [firewall Azure SQL](https://learn.microsoft.com/azure/azure-sql/database/firewall-configure?view=azuresql).
