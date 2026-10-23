# DevOps e Cloud — consolidação final

Inspeção em 26/09/2026. Fontes: [workflow backend](../.github/workflows/backend.yml), [script CI](../Scripts/Test-CI.ps1), [workflow Android](https://github.com/vinicimxt/TRAMPO-APP/blob/ba0064bf748b8905a9b38bd7a47f60ae2d50df66/.github/workflows/android.yml). Relatórios históricos das Sprints [7](SPRINT-7-DEVOPS.md) e [8](SPRINT-8-CLOUD.md) preservados; suas pendências descrevem aquele momento.

## Git e execução remota real

Ambos os projetos possuem Git e remoto origin. Baseline local backend `71f16ceefbebd21b7097f29ad1f43dbe9ad6eaae`; Android `ba0064bf748b8905a9b38bd7a47f60ae2d50df66`. Working trees limpas no início. Nenhum push, commit, reset, rebase ou reescrita de histórico foi feito nesta sprint.

Consulta pública à API do GitHub em 26/09/2026:

| Repositório/run | Commit | Resultado real | Alcance |
| --- | --- | --- | --- |
| [Backend CI 36214715828](https://github.com/vinicimxt/Trampo/actions/runs/36214715828) | `71f16ce` | success | Mesmo baseline local; não inclui documentos ainda não enviados |
| [Backend CI 36214779985](https://github.com/vinicimxt/Trampo/actions/runs/36214779985) | `55810f2` | success | Outra revisão remota; não afirmar que é o HEAD local |
| [Android CI 36214952264](https://github.com/vinicimxt/TRAMPO-APP/actions/runs/36214952264) | `ba0064b` | failure | Etapa `android-actions/setup-android@v3`; testes/lint/APK não executados |

A causa interna exata do setup Android não foi estabelecida pelas anotações públicas consultadas. Há avisos de runtime Node das actions e de ausência de relatórios, mas isso não demonstra causalidade. Não alterar SDK/actions por suposição nesta sprint. Próximo passo: examinar o log completo do job, corrigir a causa comprovada e disparar CI após push autorizado. O sucesso local Android não substitui o resultado remoto.

## Pipelines existentes

Backend: `push`, `pull_request`, `workflow_dispatch`; Ubuntu; .NET conforme `global.json`; restore/build Release com analisadores, runner Sprint1 `--unit`, publish e runner Sprint8. Runners Sprint1/6/8 são compilados. Sprint9 e integrações SQL não são executados pelo workflow. Upload de `artifacts/backend`, retenção 7 dias. `dotnet test` não executa esses runners de console.

Android: mesmos gatilhos; Java 25, SDK Android 37/build-tools 36.0.0, Gradle; `testDebugUnitTest lintDebug assembleDebug`; upload de relatórios e APK Debug por 7 dias. `LiveApiTest` é ignorado sem fixture e executado separadamente pelo runner Windows Sprint6. Workflows têm `contents: read`, concorrência por ref e limites de tempo. CD automático, aprovação de ambiente e deploy não estão implementados.

## Configuração e secrets

Development: SQL Express com autenticação integrada configurado em appsettings.Development.json, sem senha de banco. JWT persistente via User Secrets ou `Jwt__SigningKey`; `Scripts/Start-Dev.ps1` não gera chave a cada execução. Nunca versionar saída de `dotnet user-secrets list`.

Production: `ConnectionStrings__Xamou` e `Jwt__SigningKey` externos; issuer/audience/expiração também configuráveis. Conexão ausente impede startup; JWT ausente impede login API. User Secrets é recurso de desenvolvimento, não secret store de produção. Não incluir conexão/JWT em artefatos, logs, screenshots ou comandos de apresentação.

Android Debug permite HTTP apenas para `10.0.2.2`; Release bloqueia cleartext e exige propriedade `trampoApiUrl` HTTPS válida terminada em `/`. Não há endpoint público de produção, APK Release assinado ou publicação Play Store comprovados.

## Publicação e operação

`Scripts/Test-CI.ps1` publica framework-dependent em `artifacts/backend` sem appsettings.Development.json, testes ou documentação. `GET /health` é liveness anônimo e sem cache; não é readiness de SQL. Swagger/OpenAPI é Development-only. Sessão usa memória local: reinício pode encerrar sessões Web.

Proposta de cloud: cliente Web/Android → HTTPS → ASP.NET Core → SQL Server gerenciado; secrets externos, logs, health e backups. Diagrama em [arquitetura](ARQUITETURA-FINAL.md), procedimento em [implantação](IMPLANTACAO.md). Nada foi provisionado/deployado nesta sprint. Docker, observabilidade avançada, restauração testada em provedor e múltiplas instâncias continuam futuros.

## Reprodução local

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/Test-CI.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint5
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint9/Run.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint6/Run.ps1
```

Executar integrações somente no SQL local de desenvolvimento/testes e sequencialmente (mesmo obj e portas fixas). Na raiz Android, com JAVA_HOME configurado para o JBR compatível:

```powershell
.\gradlew.bat testDebugUnitTest lintDebug assembleDebug --rerun-tasks --console=plain
```

Resultados/contagens atuais em [TESTES-FINAIS](TESTES-FINAIS.md). Não somar assertions históricas repetidas nem chamar verificação de host de teste unitário.
## Atualização Sprint 11 — 10/10/2026

A consulta pública ao GitHub manteve os resultados acima: backend verde em `71f16ce` e no run posterior `55810f2`; Android vermelho em `ba0064b`. O job Android falhou em `setup-android@v3`; `setup-java` passou e as etapas SDK, Gradle, testes, lint e APK foram ignoradas. A API de download do log integral retornou HTTP 403 sem autenticação, portanto a causa interna continua não comprovada. Não houve mudança no workflow ou nova execução remota. [Decisão e regressão Sprint 11](SPRINT-11-PRONTIDAO-FINAL.md).

A configuração Android foi conferida: Java 25, platform 37, build-tools 36.0.0, Gradle 9.6.0, AGP 9.4.1, setup-gradle v4/cache e wrapper versionado. `gradlew` está em modo Git `100644`, mas o comando usa `bash ./gradlew`, e essa etapa não foi alcançada no run falho. Docker permanece futuro: o SQL Express/Trusted_Connection do desenvolvimento Windows não seria provisionado por um Dockerfile isolado da aplicação, tampouco secrets, TLS e operação. Cloud continua proposta; não houve deploy.

## Atualização pré-Git — Desktop, 12/10/2026

O Windows Forms tem workflow próprio em `.github/workflows/desktop.yml`, com `contents: read`, restore, build Release e runner não visual em `windows-latest`. Build Release e 70 verificações não visuais passaram localmente, conforme [Sprint 12 Desktop](SPRINT-12-DESKTOP.md). O diretório Desktop ainda não possui repositório Git; não há execução remota desse workflow nem status de CI para afirmar. A situação cloud, Docker e CD acima permanece proposta/pendente, sem deploy real.
