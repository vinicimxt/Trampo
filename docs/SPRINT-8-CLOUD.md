# Sprint 8 — cloud e fechamento técnico

## Resultado e escopo

TRAMPO preparado para configuração externa e publicação direta, preservando
ASP.NET Core MVC + REST, SQL Server/ADO.NET, JWT e cliente Android Kotlin/XML.
**Nenhum deploy cloud foi realizado.** Não foram criados serviços pagos,
infraestrutura, endpoint fictício, Redis, Docker, autenticação nova ou publicação mobile.

O [baseline anterior às edições](SPRINT-8-BASELINE.md) registra auditoria e testes.
O [roteiro de implantação](IMPLANTACAO.md) concentra configuração Production,
banco, segurança, proxy/TLS, sessão, Data Protection, monitoramento e checklist.

## Implementado

- ConnectionStrings:Xamou é lida pelo IConfiguration do ASP.NET Core.
  Program inicializa Conexao antes das requisições. Como os DAOs legados usam
  new Conexao(), uma ponte estática de configuração preserva seus construtores;
  não houve migração de arquitetura nem reescrita de DAOs.
  O construtor com IConfiguration permite validação isolada. A ponte pressupõe
  uma aplicação por processo; não é um mecanismo para múltiplos tenants.
- A string Windows/SQL Express saiu do C# para appsettings.Development.json.
  Configuração de ambiente/secret store prevalece sobre JSON.
  Production sem conexão explícita falha cedo, com mensagem sem valores sensíveis.
  A validação exige servidor e banco e verifica formato, mas não abre conexão.
- Falha na abertura SQL descarta o recurso e informa apenas código numérico,
  sem concatenar servidor, usuário, senha ou mensagem original.
  O contrato público genérico de erro da API foi preservado.
- GET /health usa health checks nativos, anônimo, HTTP 200/Healthy e no-store.
  É liveness da aplicação, não readiness SQL nem validação da configuração JWT.
- Publish framework-dependent sem apphost, limpo antes da geração e sem
  appsettings.Development.json. Scripts/Test-CI.ps1 inclui a suíte Sprint 8.
- Runners de integração carregam explicitamente configuração Development e
  propagam a mesma conexão ao subprocesso, inclusive no teste de Production.
  Variáveis externas prevalecem: executar somente contra banco de testes local.
- Android não teve fontes/configuração alterados nesta sprint.
  Release continua exigindo endpoint HTTPS real e assinatura futura.

## Arquitetura atual e proposta para o PIM

Atual: navegador → MVC/sessão e Android Debug → API/JWT no mesmo processo
ASP.NET Core → Services → DAOs/ADO.NET → SQL Express Xamou no Windows.

Diagrama textual da **proposta**, independente de fornecedor:

~~~text
Navegador (MVC)       Android (REST/JWT)
       \                    /
        +------ HTTPS -----+
                   |
      Hospedagem ASP.NET Core (.NET 10)
      MVC + API + Services + DAOs
      GET /health (somente liveness)
          |                 |
          | TLS             +--> Logs / alertas / métricas do provedor
          v
    SQL Server gerenciado
    schema + migração 001 + backups testados

Secret store / configuração --> conexão SQL + chave JWT
CI --> artefato Release aprovado --> CD futuro / homologação / aprovação
~~~

Uma única instância é suficiente para a demonstração proposta. Múltiplas instâncias
exigem estratégia de sessão compartilhada e persistência/compartilhamento de
chaves Data Protection. Sticky session não resolve essas necessidades por si só.
Azure App Service/Azure SQL são exemplos possíveis, não serviços provisionados.

## Desenvolvimento local

Executar na raiz backend:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/Start-Dev.ps1
~~~

Mantidos HTTP localhost:5165, emulador 10.0.2.2:5165, perfil Development,
SQL Express integrado e User Secrets de JWT existentes. Nenhuma chave é gerada
a cada execução. Para substituir o banco, configurar ConnectionStrings__Xamou
no ambiente do processo ou ConnectionStrings:Xamou em User Secrets.
Não alterar a string padrão para inserir senha real em arquivo versionado.

## Validação final

| Verificação | Resultado |
| --- | --- |
| Restore/build Release | Aprovados para aplicação e 3 runners |
| Analisadores backend | 109 avisos legados de nulabilidade, zero erros |
| Suíte pura Sprint1 --unit | 4 verificações aprovadas |
| Nova suíte Sprint8 | 15 verificações aprovadas, sem banco real |
| Suíte histórica --sprint5 | 288 verificações aprovadas, zero falhas |
| Integração Kotlin/API/SQL Sprint6 | 6 verificações e 1 LiveApiTest aprovados |
| Android testDebugUnitTest | 24 descobertos, 23 aprovados, 1 ignorado, zero falhas/erros |
| Android lintDebug | Zero erros, 10 avisos preexistentes |
| Android assembleDebug | Aprovado |
| Publish Release | Aprovado; pacote iniciado pelos testes em Production |
| Health Production com SQL indisponível | HTTP 200, Healthy, sem cache |
| Production sem connection string | Startup recusado como esperado |
| Swagger Production | 404 |
| Erro SQL na API / marcador de senha nos logs | Resposta genérica; marcador não exposto |
| Start-Dev Development | /health HTTP 200/Healthy e OpenAPI HTTP 200 |

As quatro verificações puras são parte das 288 históricas; não somar como testes novos.
A suíte Sprint8 inclui testes de configuração e de processo/HTTP local do artefato,
não é toda unitária. O CI agora executa 4 + 15 verificações sem SQL Server real.
Os testes Production locais usaram HTTP loopback para exercitar o processo; não
comprovam certificado, proxy nem HTTPS de um provedor cloud.

Os dez avisos Android são nove sugestões de versões e um TextFields; há ainda
aviso Robolectric sobre acesso nativo no Java 25. Não foram suprimidos.
A primeira tentativa adicional de integração Mobile concorreu com o build do smoke
Start-Dev e encontrou bloqueio do cache MSBuild. A repetição sequencial passou: 6 verificações e o LiveApiTest Kotlin, sem falhas;
não rodar builds .NET sobre o mesmo obj ao mesmo tempo.

Comandos reais:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/Test-CI.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint5
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint6/Run.ps1
# Na raiz Android, JAVA_HOME configurado para JBR 25:
.\gradlew.bat testDebugUnitTest lintDebug assembleDebug --rerun-tasks --console=plain
~~~

Evidências locais ignoradas: artifacts/sprint8-ci.log, sprint8-integration.log,
sprint8-mobile-integration.log, sprint8-start-dev.log (todos em artifacts).
Relatórios unitários Android preservados em artifacts/sprint8-android-results,
pois a execução filtrada LiveApiTest substitui o relatório Android padrão.
Android: sprint8-final.log, app/build/reports/lint-results-debug.*,
app/build/outputs/apk/debug/app-debug.apk. Backend: artifacts/backend.

## Arquivos desta sprint

Criados:
- Tests/Sprint8/Sprint8.csproj
- Tests/Sprint8/Program.cs
- docs/SPRINT-8-BASELINE.md
- docs/SPRINT-8-CLOUD.md
- docs/IMPLANTACAO.md

Alterados:
- conexao.cs
- Program.cs
- appsettings.Development.json
- BD-TRAMPO.csproj
- Scripts/Test-CI.ps1
- Tests/Sprint1/Program.cs
- Tests/Sprint6/Program.cs

Scripts SQL, funcionalidades Web/API, workflow Android e código mobile preservados.
As alterações pendentes da Sprint 7, inclusive a limpeza do índice Git, continuam
pendentes de revisão/commit; não houve reset, commit, push ou nova limpeza do índice.

## Implementado versus planejado

Implementados e testados localmente: externalização SQL, health, publish limpo,
regressão, configuração por ambiente e testes incorporados ao comando da CI.
Os workflows estão preparados em arquivos locais; a execução no GitHub desta
versão ainda não ocorreu. O workflow backend segue chamando o script atualizado.

Planejados: hospedagem e banco cloud, domínio/TLS real, confiança em proxy,
secret store do provedor, backups/restauração, chaves Data Protection duráveis,
estratégia de sessão/escalabilidade, CD com identidade e aprovação, assinatura
Android e endpoint Release real. Docker permanece conscientemente adiado.

Para fechar o PIM: revisar este relatório, coletar screenshots sem secrets da
demonstração local e testes, publicar/versionar os workflows, criar o repositório
Android e preparar a apresentação do diagrama. Se cloud real for requisito da
banca, executar e comprovar o roteiro de implantação; a preparação não a substitui.
