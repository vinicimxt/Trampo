# Relatório de preparação MonsterASP.NET — TRAMPO

Data: 22/11/2026. **Estado: PREPARADO PARA DEPLOY.** Nenhuma conta, Website Free, MSSQL Free, certificado, URL pública ou deploy foi criado nesta sessão. Não houve commit nem push. A versão online precisa ser publicada pelo usuário no painel, após enviar os arquivos revisados ao GitHub.

Atualização em 23/11/2026: o usuário informou que já preparou a conta e os schemas no MonsterASP.NET. Essa informação ainda não foi verificada no painel; URL pública, configuração e testes hospedados continuam pendentes.

## Compatibilidade encontrada

- `BD-TRAMPO.csproj` usa `net10.0`; `BD-TRAMPO.sln` inclui apenas o backend. MonsterASP.NET informa suporte a .NET 10 em IIS, MVC, Web API, MSSQL, variáveis de ambiente e [Git deploy com Build .NET app](https://help.monsterasp.net/websites/git-deploy/github-repository). A compatibilidade efetiva no Website Free ainda depende de publicação e teste.
- `global.json` fixa o SDK `10.0.401` com `latestPatch`. O suporte do provedor ao runtime .NET 10 não comprova disponibilidade dessa faixa de SDK no builder Git; verificar o primeiro build remoto e consultar suporte se ocorrer erro de SDK. O SDK não foi alterado nesta tarefa.
- Production lê `ConnectionStrings__Xamou` pelo `IConfiguration`; o nome físico MSSQL do painel pode ser diferente de `Xamou`. `appsettings.Development.json` contém apenas a conexão SQL Express integrada e não entra no publish. `Jwt__SigningKey` deve ser configurada no painel. A falta da chave não impede `/health`, mas deixa login REST indisponível.
- `Program.cs` usa tratamento de erro MVC/API, `UseHttpsRedirection`, HSTS fora de Development, `UseStaticFiles` e Swagger/OpenAPI somente em Development. Sem CORS permissivo. `ASPNETCORE_URLS` não é fixado no código; `launchSettings.json` é local.
- `ScriptBD.txt` tem `USE master`, `CREATE DATABASE`, `USE Xamou` e trechos de DROP comentados; não deve ser importado inteiro no compartilhado. O corpo de schema/catálogo tem 14 tabelas e dois índices únicos explícitos. A migração 001 é aditiva/transacional. Compatibilidade com o MSSQL do host foi revisada estaticamente, não comprovada por importação remota.
- Não foram identificados serviços de background pesados, upload persistente ou caminhos absolutos `C:\Users\...` nas fontes auditadas. `CacheEnderecos` tem limite de 500 entradas; sessão é em memória. O plano Free anunciado tem [256 MB de RAM](https://help.monsterasp.net/websites/configuration/memory-limit), portanto consumo e reinícios devem ser observados após deploy.
- Android e Desktop estão em outros repositórios. README e documentação existente descrevem `trampoApiUrl` e `TRAMPO_API_BASE_URL`, mas builds e testes reais desses clientes não ocorreram aqui.

## Arquivos desta tarefa

Criados:

- `ScriptsSQL/MonsterASP/001_schema.sql` — schema/catálogo do `ScriptBD.txt`, com guarda para banco vazio, sem operações de criação/seleção/remoção de banco.
- `ScriptsSQL/MonsterASP/002_enderecos.sql` — migração 001 adaptada apenas no comentário introdutório para o banco físico do painel.
- `ScriptsSQL/MonsterASP/003_validacao.sql` — consultas somente leitura de banco, tabelas, constraints, índices, colunas e catálogo.
- `docs/DEPLOY-MONSTERASP.md` — guia e checklist.
- `docs/DEPLOY-MONSTERASP-RELATORIO.md` — este relatório.

Alterado:

- `Scripts/Test-CI.ps1` — substituídas referências a projetos Sprint1/6/8 ausentes neste checkout pelos três runners existentes que não precisam de SQL nem credenciais; mantido publish Release e verificação de DLL, `wwwroot` e ausência de `appsettings.Development.json`.

Arquivos Azure ainda não versionados já estavam no workspace antes desta tarefa e não foram alterados.

## Resultado local

| Verificação | Resultado |
| --- | --- |
| `dotnet restore BD-TRAMPO.csproj` | Passou |
| `dotnet build BD-TRAMPO.csproj --no-restore` | Passou; 109 avisos de nulabilidade preexistentes, 0 erros |
| `dotnet build BD-TRAMPO.csproj -c Release --no-restore` | Passou; 0 avisos no build incremental, 0 erros |
| `dotnet publish BD-TRAMPO.csproj -c Release --no-restore` | Passou; DLL, `web.config` e `wwwroot` presentes, configuração Development ausente |
| `Scripts/Test-CI.ps1` | Passou após correção; binding, contrato de agendamento, feedback de erro e publish |
| Smoke local com `ASPNETCORE_ENVIRONMENT=Production` e configuração fictícia | `/health` 200, `/css/site.css` 200, `/openapi/v1.json` 404, `/api/v1/auth/me` sem token 401 |
| MSSQL MonsterASP, `GET /`, login Web/API, clientes | Não testados sem banco/URL reais |

O primeiro CI falhou por `Tests/Sprint1/Sprint1.csproj` inexistente; a referência foi corrigida e a repetição passou. `stabilization-contract` requer API local, serviço público e contas de teste informadas por variáveis; não foi executado sem fixtures. O scan de arquivos versionados encontrou configuração SQL Express local e referências/valores de exemplo, sem chave JWT ou conexão online literal em `appsettings.json` ou workflow. Isso não substitui revisão do histórico Git antes de publicar.

## Configuração Production e pendências manuais

No painel, definir `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__Xamou` com a string **Local access** do MSSQL Free, e `Jwt__SigningKey` nova em Base64 com pelo menos 32 bytes aleatórios. Nunca colocar esses valores em JSON, documentação ou Git. A URL dos clientes permanece o placeholder `https://SEU-SUBDOMINIO/` até a URL real e o certificado funcionarem.

Pendentes: confirmar planos Free no painel; criar Website e MSSQL; importar 001/002 e validar 003; configurar variáveis; conectar GitHub e publicar manualmente; solicitar HTTPS ao suporte Free; testar Web/API/SQL e depois Android/Desktop com contas fictícias. Segundo o [guia HTTPS do provedor](https://help.monsterasp.net/domains-https/https/lets-encrypt), o suporte ativa HTTPS no Free após a publicação inicial. Não classificar como `DEPLOY REALIZADO` sem URL real respondendo, nem como `DEPLOY VALIDADO` sem testes funcionais.
