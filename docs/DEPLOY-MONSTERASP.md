# Deploy acadêmico do TRAMPO no MonsterASP.NET

**Estado: PREPARADO PARA DEPLOY.** Este guia não contém domínio nem credenciais reais. A URL online será `https://SEU-SUBDOMINIO/` quando a hospedagem e o HTTPS estiverem ativos. A aplicação mantém ASP.NET Core MVC + REST em .NET 10 e MSSQL; o ambiente local continua em `http://localhost:5165/`.

## Antes de começar

- O [plano Free](https://help.monsterasp.net/getting-started/first-steps/create-first-website) oferece um site em subdomínio `runasp.net` ou `tryasp.net`, 256 MB de RAM e 5 GB de disco. O [MSSQL Free](https://help.monsterasp.net/databases/mssql/create-database) oferece 1 GB, somente no datacenter europeu. Confirme **Select FREE website** e **Select FREE database** no painel; não selecione Premium ou complementos pagos.
- Pelos [termos do serviço](https://www.monsterasp.net/Terms/), Free destina-se a desenvolvimento, teste e aprendizado, sem SLA nem backup do provedor e sem processamento de dados pessoais em produção. Use somente contas e dados fictícios na demonstração; mantenha uma cópia exportada dos dados necessários.
- O projeto a publicar é **`BD-TRAMPO.csproj`**, na raiz. `BD-TRAMPO.sln` contém apenas esse projeto, mas há projetos de teste em `Tests/`; escolha explicitamente o `.csproj` Web se o painel pedir. O `TargetFramework` já é `net10.0`. Não é necessário Docker.
- O Git deploy lê o **commit remoto**, não os arquivos ainda não commitados deste checkout. Depois de revisar, o usuário precisará versionar e enviar as alterações para que o painel as encontre. Esta preparação não faz commit, push ou deploy.

## Passo a passo

1. **Criar conta Free.** Acessar MonsterASP.NET e confirmar que a conta/plano escolhido é gratuito.
2. **Criar Website Free.** Em *Websites → Add website*, escolher **Select FREE website**, subdomínio gratuito e *My own application*. Anotar a URL real; não colocá-la no código antes de estar ativa.
3. **Criar MSSQL Free.** Em *Databases → Add database*, escolher **Select FREE database** e SQL Server 2025, de preferência no mesmo datacenter europeu do site. O painel cria o banco e seu usuário. O nome físico costuma ser `db...`; **`Xamou` é a chave lógica** lida pelo backend em `ConnectionStrings:Xamou`, não precisa ser o nome físico no SQL Server. Não renomear apenas para forçar `Xamou`.
4. **Importar scripts no banco vazio.** Abrir o banco criado e usar *Import → Upload SQL file* nesta ordem: `ScriptsSQL/MonsterASP/001_schema.sql`, depois `002_enderecos.sql`. A primeira cria tabelas, constraints, chaves, índices e catálogo; a segunda aplica a migração de endereços. Confirmar o banco selecionado e ler o log completo de cada importação **antes** da próxima etapa. O [importador](https://help.monsterasp.net/databases/mssql/import-sql-file) pode continuar após erros e deixar aplicação parcial; não repetir o schema às cegas. Nunca importar `ScriptBD.txt` integralmente: ele contém `USE master`, `CREATE DATABASE` e blocos históricos de DROP comentados.
5. **Validar schema.** Executar `ScriptsSQL/MonsterASP/003_validacao.sql` via *Run T-SQL* ou Import; confirmar `BancoAtual` igual ao nome físico do painel, 15 tabelas `OK`, FK/CHECK confiáveis, índices, colunas de `Locais`, 15 categorias e subcategorias. `/health` não verifica o banco. O [editor Run T-SQL](https://help.monsterasp.net/databases/mssql/run-tsql) tem timeout e limite de linhas; para script grande, usar Import.
6. **Copiar a conexão local do host.** No banco, abrir *Users → Connection string* e copiar **Local access** para o site hospedado no mesmo plano. Usar o `Database=` fornecido pelo painel; não usar `localhost\\SQLEXPRESS`, `Trusted_Connection=True` nem a string *Remote access* do computador. A [documentação do host](https://help.monsterasp.net/databases/mssql/connection-string) distingue essas duas strings. Não colar a senha no Git ou nos logs.
7. **Configurar variáveis do site.** Em *Websites → site → Scripting → Environment variables*, adicionar `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__Xamou=<string local fornecida pelo painel>` e `Jwt__SigningKey=<Base64 de 32 bytes aleatórios ou mais, nova para Production>`. Reiniciar a aplicação após salvar. O [painel aceita `__`](https://help.monsterasp.net/websites/configuration/environment-variables) para a hierarquia do `IConfiguration`. Manter `DOTNET_ENVIRONMENT` coerente, se existir. A chave JWT nunca deve ser impressa ou versionada.
8. **Conectar GitHub.** Em *Websites → site → Git deploy*, informar a URL HTTPS do repositório terminada em `.git`, selecionar branch e **Build .NET app**. Se o painel listar vários projetos, escolher `BD-TRAMPO.csproj`. Repositório privado exige token no painel; público não. Não escolher *Static* nem habilitar deploy automático pago. O [Git deploy Free](https://help.monsterasp.net/websites/git-deploy/github-repository) permite *Pull latest* e *Build & publish* manuais.
9. **Build & Publish.** Após o usuário enviar as alterações ao GitHub, usar *Pull latest* e *Build & publish*. Examinar *Recent builds*. O site IIS deve receber o publish compilado, incluindo `web.config` e `wwwroot`. Não publicar `appsettings.Development.json`.
10. **Testar liveness.** Conferir `GET /health` no endereço disponível inicialmente. Antes do certificado, usar HTTP apenas para essa verificação sem enviar login, tokens ou dados pessoais. O 200 confirma que o processo está ativo; não comprova SQL/JWT. Caso a aplicação redirecione para HTTPS ainda indisponível, verificar o log do host e aguardar o certificado antes do teste final.
11. **Solicitar HTTPS no Free.** Com a aplicação já publicada, abrir chamado no painel pedindo ativação HTTPS para o subdomínio. No [plano Free o suporte ativa o certificado](https://help.monsterasp.net/domains-https/https/lets-encrypt); depois de `https://SEU-SUBDOMINIO/` responder, habilitar *HTTPS Redirect*. Validar HTTP→HTTPS e ausência de loops antes de usar Android/Desktop.
12. **Configurar Android.** No repositório Android separado, manter Debug local (`http://10.0.2.2:5165/` no emulador; `http://127.0.0.1:5165/` com `adb reverse` em aparelho). Para Debug online, executar `./gradlew assembleDebug -PtrampoApiUrl=https://SEU-SUBDOMINIO/` (ou `gradlew.bat` no Windows). Release deve continuar exigindo HTTPS; confirmar no projeto Android antes de distribuir. Não incluir secrets do servidor no APK.
13. **Configurar Desktop.** No processo Windows Forms, definir `TRAMPO_API_BASE_URL=https://SEU-SUBDOMINIO/` e iniciar normalmente. A URL local padrão é `http://localhost:5165/`; trocar a variável não exige recompilação. Confirmar login, `/api/v1/auth/me` e consulta simples.

## Verificação funcional online

Após HTTPS: `GET /`, `GET /health`, CSS, JS, logo, imagens e fontes; cadastro/login/perfil/sessão/antiforgery, catálogo e agendamentos MVC. Confirmar que redirects não apontam a localhost, `127.0.0.1` ou porta 5165. Em Production, detalhes de exceção e Swagger/OpenAPI não podem ficar públicos.

Na API, testar com **contas fictícias**: `/api/v1/auth/login`, `/api/v1/auth/me` com JWT, serviços, categorias, agendamentos, locais, avaliações, notificações e suporte. Conferir `401` sem token e fluxo Cliente/Profissional. Não mudar contratos REST nem ativar `AllowAnyOrigin`; clientes Android e WinForms nativos não dependem de CORS. Não executar testes locais que escrevam ou removam fixtures sobre o banco online.

## Limites e diagnóstico

O backend usa sessão em memória e cache CEP limitado a 500 entradas; não há serviço de background pesado identificado. Em 256 MB, medir uso real em *Resource* após publicar: reinícios podem perder sessões. O [painel informa reciclagem ao atingir RAM](https://help.monsterasp.net/websites/configuration/memory-limit). Free não tem backup automático; exportar cópia após criar catálogo/dados fictícios. `wwwroot` é incluído no publish local; fontes externas Google Fonts, Font Awesome CDN e VLibras dependem de terceiros.

Se o *Build .NET app* falhar por SDK, conferir no log se o servidor possui a faixa fixada em `global.json` (`10.0.401`, `latestPatch`). O provedor anuncia runtime .NET 10, mas isso não confirma o SDK exato do builder; consultar o suporte antes de alterar a versão do projeto. O publish local pode ser enviado por FTP/SFTP como alternativa documentada pelo provedor.

Se `/health` falhar, verificar *Recent builds*, runtime .NET 10, variáveis e logs de startup, sem registrar valores secretos. Se `/health` passar mas `/` ou login falhar, verificar banco físico, importação, string *Local access*, usuário MSSQL e chave JWT. Se HTTPS falhar, confirmar chamado/certificado e só então ativar redirect. `ASPNETCORE_URLS` e `launchSettings.json` são para execução local; no IIS do MonsterASP usar a configuração do site, sem fixar localhost ou porta 5165.

## Checklist pré-deploy

- [x] `dotnet restore`, build Debug, build Release e publish Release passaram localmente.
- [x] Scripts MSSQL para banco hospedado vazio preparados, sem `CREATE DATABASE`/`USE` ativos.
- [x] Connection string e JWT de Production fora dos arquivos versionados auditados; valores reais ainda não configurados.
- [ ] `ASPNETCORE_ENVIRONMENT=Production` configurado no painel.
- [x] `/health` respondeu 200 em smoke test **local** Production; falta teste no host.
- [ ] Android Debug/Release verificados com URL HTTPS real; fontes Android não estão neste checkout.
- [ ] Desktop verificado com URL HTTPS real; fontes Desktop não estão neste checkout.
- [x] Publish contém `wwwroot`; busca de fontes não encontrou caminhos absolutos locais.
- [ ] Website/MSSQL Free criados, schema importado e HTTPS ativado.

**Estados:** `PREPARADO PARA DEPLOY` agora; `DEPLOY REALIZADO` somente com URL real respondendo; `DEPLOY VALIDADO` somente após Web, API e SQL testados. O resultado desta sessão está em [DEPLOY-MONSTERASP-RELATORIO.md](DEPLOY-MONSTERASP-RELATORIO.md).
