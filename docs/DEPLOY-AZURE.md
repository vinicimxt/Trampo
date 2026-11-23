# Deploy acadêmico do TRAMPO no Azure

**Estado atual: PREPARADO PARA DEPLOY.** Este roteiro não cria recursos por si. Não há URL pública confirmada. Preserva ASP.NET Core MVC + REST em .NET 10, Azure SQL Database `Xamou`, Android e Windows Forms como clientes HTTPS.

```text
Navegador ─ HTTPS ─┐
Android ── HTTPS ──┼─ App Service (.NET 10: MVC + /api/v1 + /health)
Desktop ── HTTPS ──┘                         │
                                             └─ TLS ─ Azure SQL (Xamou)
```

## 1. Pré-requisitos e barreira de custo

Instalar Azure CLI e executar `az --version` e `az account show`. Se não houver conta autenticada, executar `az login` na sua máquina; não informar senha no chat. Conferir assinatura, região, cotas e permissões. O ambiente desta auditoria não possui `az`, portanto nenhuma consulta à assinatura ocorreu.

**Antes de criar cada recurso**, registrar esta tabela com valores observados no Portal/CLI da assinatura escolhida:

| Recurso | Plano/tier exigido | Preço/tier identificado | Região | Possível cobrança |
| --- | --- | --- | --- | --- |
| Resource group | Sem SKU | Conferir recursos agregados | A confirmar | O grupo em si não substitui a análise dos recursos |
| App Service plan + Web App | **Free F1**, Windows, .NET 10 disponível | **A confirmar na assinatura/região** | A confirmar | Parar se plano pago, recurso auxiliar ou upgrade for necessário |
| SQL logical server + Xamou | Banco **Free tier**, `AutoPause` no limite | **A confirmar na assinatura/região** | A confirmar | Parar se Free indisponível ou `BillOverUsage` for exigido |
| Rede e serviços adicionais | Somente o estritamente necessário | A confirmar | A confirmar | Parar diante de qualquer cobrança/incerteza |

Prosseguir somente se o portal/CLI confirmar gratuitamente **todos** os recursos e opções. Não usar crédito promocional de duração limitada como prova de tier permanentemente gratuito. O [guia oficial do App Service](https://learn.microsoft.com/azure/app-service/quickstart-dotnetcore) mostra .NET 10 e F1, mas isso não comprova disponibilidade na sua assinatura. A [oferta Azure SQL](https://learn.microsoft.com/azure/azure-sql/database/free-tier?view=azuresql) exige escolher Free e `AutoPause` para evitar excedentes cobrados. F1 tem recursos limitados e sem SLA; SQL Free pode pausar por inatividade ou ao atingir o limite mensal.

## 2. Provisionar somente após confirmação gratuita

No Portal Azure, escolher App Service **Code**, Windows, runtime .NET 10 e plano **Free F1**. Revisar “Review + create” e o custo apresentado antes de criar. Não selecionar Basic, Standard, Premium nem P0/P1/P2. Anotar nome real `<nome>` e região. Ligar **HTTPS Only**.

Criar um servidor lógico Azure SQL e um banco novo `Xamou` pela oferta **Free tier**, com comportamento **Auto-pause the database until next month** ao atingir o limite. A disponibilidade depende da assinatura; `az sql db create` sem `--use-free-limit` pode criar banco cobrável. Após criar, confirmar no recurso `useFreeLimit=true` e `freeLimitExhaustionBehavior=AutoPause`. Se falhar ou for incerto, inspecionar o recurso existente antes de repetir. Não alterar para `BillOverUsage`.

Configurar firewall SQL de modo restrito para o cliente de administração e a saída efetiva do App Service. “Allow Azure services” permite também serviços de outras assinaturas; avaliar e aprovar expressamente essa abertura. Recursos de rede privada podem exigir plano pago, portanto não provisionar automaticamente.

## 3. Banco novo: schema, migração e verificação

`ScriptBD.txt` é para SQL Server local: contém `USE master`, `CREATE DATABASE`, `USE Xamou` e exemplos históricos comentados de DROP/admin. **Não o executar integralmente no Azure.** Conectar diretamente ao banco `Xamou` novo e vazio, com identidade administrativa, e aplicar na ordem:

1. `ScriptsSQL/000_azure_schema_catalogo.sql` — 14 tabelas e catálogo, derivado do corpo de `ScriptBD.txt`.
2. `ScriptsSQL/001_enderecos_geolocalizacao.sql` — evolução aditiva, inclusive `AgendamentoEnderecos`.
3. `ScriptsSQL/azure_validacao.sql` — somente leitura; conferir banco atual `Xamou`, 15 tabelas marcadas `OK`, FK/CHECK habilitadas e confiáveis, índices, 10 colunas de `Locais`, 15 categorias e subcategorias do catálogo.

O bootstrap não é idempotente e não deve ser aplicado a banco existente. Executar pelo Query Editor/SSMS selecionando `Xamou`, ou com `sqlcmd -S <servidor>.database.windows.net -d Xamou -G -b -i ScriptsSQL/000_azure_schema_catalogo.sql` quando a autenticação Microsoft Entra estiver configurada. Repetir `-i` para 001 e validação. Se usar login SQL, obter a senha por mecanismo seguro, fora dos argumentos do processo e do repositório. Registrar resultados sem dados pessoais ou secrets. A compatibilidade T-SQL foi revisada estaticamente; só a execução no Azure a comprova.

Configurar usuário da aplicação com permissões mínimas de dados; separar identidade de migração. O código usa `sys.sp_getapplock` no fluxo de agendamento: testar essa permissão/funcionalidade com a identidade real. Não usar contas pessoais existentes para dados de demonstração.

## 4. Configurar App Service

Preencher os Application settings de [AZURE-CONFIG.md](AZURE-CONFIG.md): `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__Xamou`, `Jwt__SigningKey` próprio de Production e, quando definido, `AllowedHosts`. A conexão Azure SQL deve usar TLS e validar certificado. Não publicar `appsettings.Development.json`, não copiar senha para JSON, README ou workflow. O projeto não possui `appsettings.Production.json`, e não precisa dele para ler variáveis do ambiente.

Usar uma instância no F1. Sessão usa cache em memória, logo reinício pode desconectar usuários. Confirmar persistência das chaves Data Protection no host antes de depender de sessão/antiforgery após deploy. Se for escolhida hospedagem Linux, verificar encaminhamento confiável de `X-Forwarded-Proto` antes de HSTS e redirect HTTPS; [a orientação ASP.NET Core](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0) alerta para loop de redirecionamento em proxy sem configuração. Não confiar indiscriminadamente em headers vindos da internet.

## 5. Publicar sem Docker

No checkout local, `Scripts/Test-CI.ps1` já restaura, compila, testa e publica `artifacts/backend/`. Alternativamente, usar `dotnet publish BD-TRAMPO.csproj -c Release --self-contained false -p:UseAppHost=false -o artifacts/backend` em saída limpa. Conferir que `appsettings.Development.json`, secrets e arquivos de teste não estão no pacote. Criar ZIP com o **conteúdo** de `artifacts/backend`, não com a pasta pai. Com Azure CLI autenticada e Web App F1 já verificado, publicar pelo método oficial ZIP deploy (`az webapp deploy --resource-group <grupo> --name <nome> --src-path <arquivo.zip> --type zip`). Não executar criação rápida que possa escolher plano por padrão.

O workflow `.github/workflows/backend.yml` faz **CI** e gera artefato; não tem credenciais Azure nem CD. Não substituí-lo. Um CD futuro deve usar GitHub Secrets ou OIDC federado, ambiente protegido e identidade de menor privilégio; nenhum segredo literal no YAML. Fazer deploy manual primeiro e guardar o artefato anterior para rollback. Reverter binário não reverte schema SQL.

## 6. Validação pública após deploy

Registrar URL real `https://<nome>.azurewebsites.net/` somente depois de responder. Testar HTTP→HTTPS, `GET /` e `GET /health` (esperado 200), CSS, JS, imagens, logo, ícones, fontes e ausência de links/redirects para localhost/5165. Testar login/cadastro/sessão/antiforgery MVC; reiniciar uma vez para observar o comportamento de sessão. Em Production, DeveloperExceptionPage e Swagger/OpenAPI não podem aparecer; exceções devem ter resposta genérica.

Na API, testar cadastro/login de contas de **demonstração**, Bearer JWT, `/api/v1/auth/me`, categorias, serviços e agendamentos. Verificar 401 sem token, fluxo Cliente (cadastro, catálogo, agendamento) e Profissional (cadastro/login, serviço, recebimento, confirmação), até onde os dados permitirem. `/health` não consulta SQL nem prova que o JWT funciona. Evitar executar suites locais que removem fixtures em banco público.

## 7. Clientes

**Android:** no repositório Android separado, manter os endereços Development existentes (emulador `http://10.0.2.2:5165/`, USB com `adb reverse` em `http://127.0.0.1:5165/`). Após obter URL real, usar `./gradlew assembleDebug -PtrampoApiUrl=https://<nome>.azurewebsites.net/` e `./gradlew assembleRelease -PtrampoApiUrl=https://<nome>.azurewebsites.net/` (no Windows, `gradlew.bat`). Confirmar no código/build que Release rejeita HTTP e ambos aceitam HTTPS. Instalar Debug em dispositivo real e verificar sem `adb reverse`, IP LAN nem mesma rede Wi-Fi. A assinatura Release segue as regras do projeto Android.

**Desktop:** no processo Windows Forms, definir `TRAMPO_API_BASE_URL=https://<nome>.azurewebsites.net/`. Abrir app e testar login, `/api/v1/auth/me` e uma consulta simples. Não mudar o padrão local Development. Os fontes de ambos os clientes não fazem parte deste checkout; execução e instalação ainda dependem deles.

## 8. Atualização, diagnóstico e limites

Para cada versão: CI, backup/estado do SQL, migrações revisadas, publish, smoke de `/`, `/health`, login e catálogo, depois fluxo E2E. Registrar versão e artefato para rollback. Se `/health` falhar, checar startup/runtime .NET 10/App Service logs sem imprimir settings. Se `/health` passar e login falhar, checar acesso SQL, firewall, usuário, chave JWT e catálogo. Se houver redirect circular, conferir HTTPS Only, esquema encaminhado e proxy. Se assets falharem, conferir conteúdo de `wwwroot` e caixa dos caminhos. Se SQL estiver pausado, conferir cota gratuita e conexões abertas; não habilitar excedentes pagos como correção automática.

F1 não oferece SLA nem escala; SQL Free pode ficar indisponível ao atingir a cota mensal com `AutoPause`. Sessão em memória perde estado em restart. Não usar filesystem local do App Service como armazenamento persistente de uploads futuros. Custos de serviços auxiliares, saída de dados e recursos opcionais precisam de revisão própria. O relatório de execução fica em [DEPLOY-AZURE-RELATORIO.md](DEPLOY-AZURE-RELATORIO.md).
