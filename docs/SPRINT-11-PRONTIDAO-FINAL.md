# Sprint 11 — prontidão técnica para entrega

Auditoria em 10/10/2026. Backend no commit `71f16ce`, Android em `ba0064b`. O Android estava limpo; o backend já continha alterações locais, preservadas. Nenhum commit, push, dispatch, deploy ou mudança funcional ocorreu nesta auditoria. A documentação da Sprint 10 ainda é local e não consta do commit backend remoto.

## CI remoto

| Projeto | Evidência | Resultado |
| --- | --- | --- |
| Backend, HEAD local | [run 36214715828](https://github.com/vinicimxt/Trampo/actions/runs/36214715828), `71f16ce` | Success no commit, sem as alterações locais posteriores |
| Backend, último run | [run 36214779985](https://github.com/vinicimxt/Trampo/actions/runs/36214779985), `55810f2` | Success em revisão remota diferente do HEAD local |
| Android, último run | [run 36214952264](https://github.com/vinicimxt/TRAMPO-APP/actions/runs/36214952264), `ba0064b` | Failure em `android-actions/setup-android@v3`; instalação SDK, Gradle, testes, lint e APK ignorados |

A API pública confirmou as etapas: checkout e setup-java passaram. O endpoint de download do log completo retornou HTTP 403 sem autenticação. A mensagem interna da falha, portanto, não foi obtida e a causa exata segue desconhecida. O workflow não foi alterado por suposição. Próximo passo: ler o log integral com acesso ao repositório, corrigir a primeira causa comprovada, enviar a correção e verificar novo run com `testDebugUnitTest`, `lintDebug` e `assembleDebug`. Não há execução remota Android aprovada nesta data.

Configuração Android: Temurin Java 25; setup-android v3; platform 37; build-tools 36.0.0; AGP 9.4.1; wrapper Gradle 9.6.0 com SHA-256; setup-gradle v4 com cache. `gradlew` está versionado em modo Git `100644`, mas é invocado por `bash ./gradlew`; essa etapa nem foi alcançada no run falho. O modo merece revisão futura, sem atribuir a ele a falha observada.

## Regressão local

Logs estão em `artifacts/sprint11/`, ignorados pelo Git. Os runners que compartilham backend/SQL foram executados em sequência.

| Verificação | Resultado |
| --- | --- |
| `Scripts/Test-CI.ps1` | Exit 0; restore, build Release, 12 verificações sem SQL, publish Release e 15 verificações Sprint 8 |
| `Tests/Sprint1/Run.ps1 -Sprint5` | Exit 0; 296 verificações históricas, fixture removida |
| `Tests/Sprint9/Run.ps1` | Exit 0; 48 verificações, fixtures removidas |
| `Tests/Sprint6/Run.ps1` | Exit 0; seis verificações Mobile/API/SQL e LiveApiTest Kotlin, fixtures removidas |
| Android `testDebugUnitTest lintDebug assembleDebug --rerun-tasks` | Exit 0; 30 JUnit descobertos, 29 aprovados, um ignorado, zero falhas/erros; lint zero erros/11 avisos; APK Debug gerado |

As 12 verificações sem SQL pertencem às 296 históricas. LiveApiTest é o teste ignorado na suíte comum e executado na integração. Não somar contagens duplicadas ou heterogêneas. A compilação incremental atual reportou zero avisos .NET; execução limpa anterior registrou 109 avisos de nulabilidade. Permanecem 11 avisos Android e aviso JVM de acesso nativo. Não houve novo smoke visual nesta sprint.

Sprint 8 validou o artefato publicado em Production: `/health` anônimo responde 200/`Healthy`, sem cache e sem secret/connection string. É liveness, não readiness SQL/JWT. Production sem `ConnectionStrings__Xamou` não inicia. Com conexão externa e SQL indisponível, inicia e mantém esse health. Swagger/OpenAPI é Development-only; erros API não expõem detalhes. `Jwt__SigningKey` externo é necessário para login API em Production; sem ele o MVC ainda inicia, mas o login API fica indisponível. Validar isso antes de hospedar.

## Segurança, Cloud e Docker

Busca por padrões em arquivos rastreados dos dois repositórios não achou chave privada, PAT GitHub ou signing key literal. Matches de `Password=` no backend são fixtures geradas de testes ou placeholder de implantação; no Android são nomes de campos/variáveis. Valores não foram impressos. A busca não equivale a auditoria exaustiva do histórico. User Secrets não está rastreado. Ignore rules cobrem `bin`, `obj`, `artifacts`, logs, `.env`, secrets, `build`, APK e keystore. Android Release bloqueia HTTP; só Debug permite cleartext para `10.0.2.2`. Release exige URL HTTPS e não há APK assinado.

Implementados localmente: configuração externa, User Secrets em Development, health, publish Release, workflows de CI e arquitetura documentada. Propostos: hospedagem ASP.NET Core, SQL Server gerenciado, TLS/domínio, secret store, backups, observabilidade e CD. Não existe deploy cloud. A rubrica oficial do PIM não foi fornecida; se exigir implantação real, decidir manualmente antes de provisionar.

Docker segue evolução futura. O desenvolvimento usa SQL Express no Windows com autenticação integrada e `Trusted_Connection`. Um Dockerfile só do Web não provisiona SQL, rede, identidade de banco, secrets, TLS ou operação. O publish direto já gera artefato Release. Não há exigência de container comprovada.

Evidências: runs acima, logs locais, testes Sprint 8 para health/Production/publish, Swagger Development e smoke Web/Mobile anteriores em [TESTES-FINAIS](TESTES-FINAIS.md), [arquitetura](ARQUITETURA-FINAL.md), [banco/MER](BANCO-DE-DADOS.md) e [capturas](EVIDENCIAS-CHECKLIST.md). Faltam capturas finais revisadas de várias telas Web/Mobile/Swagger/CI e ensaio da apresentação. Nenhuma captura nova sem revisão de credenciais.

## Decisão por área

| Área | Classificação | Base / pendência |
| --- | --- | --- |
| Web | PRONTO | Regressão HTTP/MVC passou; revisar visualmente alterações locais de UX |
| API | PRONTO | Regressão e integrações passaram |
| Mobile | PRONTO | Debug local passou; sem Release assinado ou endpoint público |
| Banco | PRONTO | SQL local/fixtures validados; MER e migração documentados |
| Segurança | PENDÊNCIA NÃO BLOQUEANTE | Configuração e revisão de produção dependem do ambiente futuro; limites JWT documentados |
| Testes | PRONTO | Suítes locais e integração aprovadas; avisos conhecidos |
| CI Backend | PRONTO | Run verde no commit local; mudanças locais posteriores ainda sem run |
| CI Android | PENDÊNCIA BLOQUEANTE | Último run remoto falhou antes dos testes; exige log integral e novo run verde |
| Cloud | PENDÊNCIA NÃO BLOQUEANTE | Proposta sem deploy; reclassificar se rubrica exigir implantação |
| Docker | PRONTO | Adiado com justificativa técnica |
| Documentação | PRONTO | Documentos finais revisados; rubrica oficial ausente |
| Evidências | PENDÊNCIA NÃO BLOQUEANTE | Completar capturas finais revisadas |
| Apresentação | PENDÊNCIA NÃO BLOQUEANTE | Ensaiar demo com fixtures novas e conferir rubrica |

**Recomendação: NO-GO para declarar fechamento técnico integral enquanto o CI Android remoto estiver vermelho.** A demonstração local funciona, mas a falha remota deve ser mostrada como tal. Conferir a rubrica oficial antes do aceite acadêmico: deploy cloud, procedures/triggers, formato MER e evidências podem mudar a classificação.

