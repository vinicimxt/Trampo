# Testes finais — Sprint10

Execução em 26/09/2026, Windows, .NET SDK 10.0.401, SQL Express local e Android JBR/Gradle existentes. Baselines: backend `71f16ce`, Android `ba0064b`. Saídas completas em `artifacts/sprint10/`, ignoradas pelo Git; este documento consolida resultados, não expõe credenciais.

## Regressão executada

| Verificação/comando | Descobertos/contagem | Aprovados | Ignorados | Falhas | Evidência local |
| --- | --- | --- | --- | --- | --- |
| `Scripts/Test-CI.ps1`: restore/build Release/analisadores/publish | Etapas de build/publicação | Concluídas | — | 0 | backend-ci.log |
| Sprint1 `--unit`, dentro do CI | 12 verificações | 12 | 0 | 0 | backend-ci.log |
| Sprint8, dentro do CI | 15 verificações configuração/processo/HTTP | 15 | 0 | 0 | backend-ci.log |
| `Tests/Sprint1/Run.ps1 -Sprint5` | 296 verificações históricas MVC/API/SQL | 296 | 0 | 0 | historica.log |
| `Tests/Sprint9/Run.ps1` | 48 verificações HTTP/API/SQL | 48 | 0 | 0 | sprint9.log |
| `Tests/Sprint6/Run.ps1` | 6 verificações do runner + 1 LiveApiTest Kotlin | 6 + 1 | 0 na execução dedicada | 0 | mobile-api-sql.log; docs/Sprint6/integracao-kotlin.log |
| Android `testDebugUnitTest` | 30 testes JUnit | 29 | 1 | 0 falhas/0 erros | android.log; android-unit-results/*.xml |
| Android `lintDebug` | Análise estática | Concluída | — | 0 erros | app/build/reports/lint-results-debug.txt |
| Android `assembleDebug` | APK Debug | Concluído | — | 0 | android.log |

Todos os comandos terminaram com exit 0. Android comum executado com `--rerun-tasks`; 55 tarefas executadas. Relatórios JUnit da suíte completa foram copiados antes da integração filtrada, pois LiveApiTest substitui o relatório padrão. O APK foi instalado no emulador para o smoke descrito abaixo.

**Contagem:** as 12 verificações puras estão incluídas nas 296 históricas. O aumento de 288 para 296 decorre das oito verificações do cadastro de local já no baseline, não de testes novos na Sprint10. LiveApiTest é o mesmo teste ignorado na suíte comum e executado depois com fixture; não contar como teste novo duas vezes. As seis verificações SQL do runner corroboram esse cenário e não são seis testes JUnit. Não se publica um total agregado heterogêneo de “testes novos”.

## Warnings e limites

Backend: 109 linhas de avisos únicos de nulabilidade na compilação com analisadores, zero erros. Não foram corrigidos/suprimidos nesta sprint. Android lint: 11 warnings (dez sugestões de dependências/versões e um TextFields preexistente para número alfanumérico), zero erros. JVM informa aviso de acesso nativo; build/testes passaram.

Sprint8 valida startup/configuração, health sem banco disponível, Swagger fechado em Production e erro SQL genérico, além do pacote publicado. Não comprova cloud, certificado HTTPS de provedor ou prontidão do banco. Testes de endereço usam provider controlado; não garantem disponibilidade atual do ViaCEP real.

Histórica inclui login/CSRF/ownership, perfis, agenda/concorrência, transições, rollback, serviços/locais e endereços. Sprint9 cobre cadastro, perfil/senha, busca, notificações, suporte e avaliação. Sprint6 executa Retrofit/Kotlin contra API e SQL reais. Não foi feito cálculo de cobertura percentual.

## Smoke atual

Emulador Pixel_6/API 37, APK Debug gerado nesta execução, backend local/SQL reais. Interface inspecionada por UIAutomator e capturas; Home, Notificações e Suporte também passaram por inspeção visual direta. O helper Windows de controle visual falhou ao iniciar; foi usado ADB, sem alterar configurações de segurança do sistema.

Fluxo executado: Cadastro → Home autenticada → Perfil → busca pela fixture → Detalhe → data/horário → revisão → reserva → Meus agendamentos → confirmação de conclusão → avaliação → notificações/leitura → suporte → logout/login. A reserva, avaliação, leitura e chamado foram confirmados no SQL (uma ocorrência de cada), exclusivamente para a fixture.

Para testar conclusão sem esperar atendimento real, **somente a reserva temporária** teve data passada e estado AguardandoCliente preparados via SQL, com flags/valores correspondentes; isso não demonstra finalização profissional pela UI. A criação da reserva foi feita no Mobile. Suporte enviou apenas texto fictício ao banco local. Cancelamento foi coberto pela integração Kotlin/histórica, não repetido neste smoke visual. Alteração de senha/perfil foi coberta pelos testes, mas não repetida na UI nesta sprint.

Web: login profissional com sessão/antiforgery, HTTP 200 em Home, catálogo, dashboard, MeusServicos, Locais e Swagger/OpenAPI. POST real de cadastro de local com id=0 e CEP mascarado redirecionou à lista e persistiu CEP normalizado; confirma também a correção anterior. Validação Web foi HTTP/HTML, **não inspeção visual completa em navegador**. Admin foi coberto pela regressão histórica com fixture; não recebeu nova captura nesta sprint.

Capturas Mobile locais em artifacts/sprint10. Três imagens revisadas e sem dados reais foram incluídas em docs/evidencias: Home, Notificações e Suporte. Outras capturas continuam locais, aguardando seleção/revisão de entrega. Durante smoke foi observado texto de confirmação “Conclus?o confirmada.”; registrar como limitação cosmética preexistente, sem alteração funcional nesta sprint. TalkBack, fontes grandes, orientação horizontal e outras versões Android não foram revalidados.

Houve tentativas técnicas de preparação que falharam (API de RNG incompatível com Windows PowerShell, execução de helper local, stderr do adb e seleção de ação após navegação automática); foram corrigidas/reobservadas antes de prosseguir. Não são falhas dos testes de produto nem evidência de sucesso remoto. Nenhuma mutação de reserva foi repetida às cegas.

## CI remoto

Backend aprovado no SHA do baseline; Android falhou no setup SDK antes de testes, conforme [links e detalhes](DEVOPS-E-CLOUD.md). Nenhum push/dispatch remoto executado nesta sprint. Não confundir resultados locais aprovados com CI Android remoto aprovado.

## Reprodução e isolamento

Comandos em [README](../README.md) e [DevOps](DEVOPS-E-CLOUD.md). Scripts SQL de criação/migração não foram reaplicados ao banco em uso. Integrações executadas sequencialmente, usando registros temporários dos próprios runners, removidos nos blocos finally. Não foram alterados dados preexistentes deliberadamente. Verificação final de resíduos e smoke no [relatório de fechamento](SPRINT-10-FECHAMENTO.md).
## Revalidação Sprint 11 — 10/10/2026

Nova execução local, com logs ignorados em `artifacts/sprint11/`: `Scripts/Test-CI.ps1` passou (restore, Release, 12 verificações sem SQL, publish e 15 da Sprint 8); histórica 296/296; Sprint 9 48/48; integração Mobile/API/SQL 6/6 e LiveApiTest Kotlin aprovado, com fixtures removidas. Android `testDebugUnitTest lintDebug assembleDebug --rerun-tasks` passou: 30 JUnit descobertos, 29 aprovados, um ignorado, zero falhas; lint zero erros/11 avisos; APK Debug gerado. Os 12 testes sem SQL já integram os 296 históricos. O LiveApiTest ignorado na suíte comum foi executado na integração. A compilação .NET incremental reportou zero avisos nesta execução; os 109 avisos de nulabilidade da compilação limpa anterior continuam registrados. O aviso JVM de acesso nativo também reapareceu. [Classificação e CI remoto](SPRINT-11-PRONTIDAO-FINAL.md).

