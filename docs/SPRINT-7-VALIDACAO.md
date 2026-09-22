# Validação e inventário — Sprint 7

Validação local em 21/09/2026. Não houve execução remota de GitHub Actions,
publicação, commit, deploy ou teste em emulador nesta sprint.

## Resultados

| Verificação | Resultado |
| --- | --- |
| Scripts/Test-CI.ps1: restore dos 3 projetos | Aprovado |
| Build Release e analisadores .NET | Aprovado; 109 avisos legados de nulabilidade, zero erros na aplicação; runners sem avisos adicionais |
| Runner Sprint1 --unit | 4 aprovadas, zero falhas, sem SQL/HTTP/JWT |
| Runner Sprint1 -Sprint5 | 288 aprovadas, zero falhas; SQL e aplicação reais |
| Runner Sprint6 | 6 verificações aprovadas, zero falhas; 1 LiveApiTest Kotlin aprovado |
| Android testDebugUnitTest | 24 descobertos: 23 aprovados, 1 ignorado, zero falhas/erros |
| Android lintDebug | Zero erros, 10 avisos preexistentes |
| Android assembleDebug | Aprovado |
| Start-Dev.ps1 | OpenAPI respondeu HTTP 200; processo de smoke encerrado |
| Release sem trampoApiUrl | Recusado na tarefa preReleaseBuild, como esperado |
| Release com URL HTTP | Recusado na configuração, como esperado |
| Validação isolada de uma URL HTTPS | Aprovada, inclusive reutilizando configuration cache |
| Git diff --check / índice | Sem erros de whitespace |
| Arquivos ignorados ainda rastreados | Zero após limpeza do índice |

As quatro verificações de CI já fazem parte das 288: não somar como novos testes.
As seis verificações do runner Mobile acompanham um único teste Kotlin de integração.
No Android, o teste ignorado é LiveApiTest; ele foi executado separadamente com
fixtures pelo runner Windows, não é uma falha nem um teste local aprovado.

Os 10 avisos de lint são nove sugestões de atualização de dependências e um
TextFields no campo de número de endereço. Não foram suprimidos: número de endereço
pode conter texto e a revisão funcional fica para outra sprint. O Robolectric
também avisa sobre acesso nativo no Java 25; não impediu a execução dos testes.

O domínio example.org foi usado apenas como entrada efêmera para a tarefa
validateReleaseApi, sem rede, sem APK Release e sem ser configurado como produção.
O teste positivo também confirmou que a tarefa não captura o script Gradle de
forma incompatível com configuration cache. Endpoint real continua pendente.

## Evidências e comandos

Na raiz backend:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/Test-CI.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint5
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint6/Run.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/Start-Dev.ps1
~~~

Logs locais ignorados: artifacts/backend-ci.log, artifacts/integration-backend.log,
artifacts/integration-mobile.log, artifacts/start-dev.log.
Artefato: artifacts/backend, publicação framework-dependent.

Na raiz Android, JAVA_HOME apontando para o JBR do Android Studio:

~~~powershell
.\gradlew.bat testDebugUnitTest lintDebug assembleDebug --rerun-tasks --console=plain
.\gradlew.bat :app:preReleaseBuild --console=plain
.\gradlew.bat :app:validateReleaseApi -PtrampoApiUrl=http://10.0.2.2:5165/ --console=plain
~~~

Os dois últimos comandos são testes negativos: devem retornar exit 1.
Logs locais: sprint7-validation.log, sprint7-release-missing.log,
sprint7-release-http.log e sprint7-release-cache.log.
Relatórios JUnit XML: app/build/test-results/testDebugUnitTest.
Lint: app/build/reports/lint-results-debug.html, .txt e .sarif.
APK: app/build/outputs/apk/debug/app-debug.apk.

## Arquivos criados

Backend:
- .github/workflows/backend.yml
- .gitignore
- global.json
- Scripts/Start-Dev.ps1
- Scripts/Test-CI.ps1
- docs/SPRINT-7-BASELINE.md
- docs/SPRINT-7-DEVOPS.md
- docs/SPRINT-7-VALIDACAO.md

Android:
- .github/workflows/android.yml
- CI-CD.md

## Arquivos alterados

Backend:
- BD-TRAMPO.csproj: excluir documentação, scripts, workflows e artefatos do publish.
- Tests/Sprint1/Program.cs: modo --unit antes de qualquer acesso ao banco.
- docs/Sprint6/Iniciar-Api.ps1: delegar à inicialização persistente da Sprint 7.

Android:
- .gitignore: gerados, arquivos locais e material de assinatura.
- app/build.gradle.kts: retirar endpoint fictício padrão, validar URI HTTPS de
  Release e manter compatibilidade da tarefa com configuration cache.

Além disso, 521 arquivos ignorados foram retirados somente do índice Git backend,
sem removê-los do disco. Isso aparece como exclusão staged; os novos fontes da
sprint permanecem disponíveis para revisão. Alterações preexistentes preservadas.

## Limitações e pendências

CI preparado e comandos locais aprovados; runners Linux/GitHub ainda não executados.
Android precisa de repositório Git e remoto para ativar seu workflow.
A cobertura backend sem SQL é limitada a quatro verificações.
Banco ainda fixo em SQL Express com autenticação integrada Windows.
Docker conscientemente adiado. Cloud, assinatura/endpoint Release, proteção de
branches, redução de warnings e expansão de testes ficam para a Sprint 8.
Estratégia de secrets e justificativas em [DevOps](SPRINT-7-DEVOPS.md).

