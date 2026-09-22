# Sprint 7 — DevOps, automação e preparação para implantação

## Objetivo e baseline

Automatizar build, testes e análise sem mudar funcionalidades MVC, REST ou Android.
O [baseline auditado](SPRINT-7-BASELINE.md) registra os comandos e resultados anteriores.
A solução .NET contém somente a aplicação; os dois projetos de testes são executáveis
console. Portanto, usar apenas dotnet test produziria uma falsa impressão de cobertura.

## Arquitetura de CI

Há dois workflows independentes, disparados em push, pull_request e workflow_dispatch.
Ambos têm contents: read, limite de duração, cancelamento de execução anterior da mesma
referência e artefatos retidos por sete dias. Não há deploy automático, secrets de
produção, emulador, SQL Server hospedado, serviços pagos ou infraestrutura cloud.

### Backend

Arquivo: ../.github/workflows/backend.yml.
Ubuntu + actions/setup-dotnet, SDK definido no global.json (10.0.401, latestPatch).
Scripts/Test-CI.ps1 executa restore e build Release nos três csproj, com os
analisadores do SDK habilitados; executa o runner Sprint1 com --unit; publica
a aplicação em artifacts/backend. Qualquer falha interrompe o pipeline.
Os avisos legados permanecem visíveis e não foram suprimidos nem convertidos
globalmente em erros nesta sprint.

--unit executa quatro verificações existentes: salt aleatório, senha correta,
senha incorreta e agenda que atravessa meia-noite. Não abre SQL, HTTP ou a aplicação,
nem requer JWT. É uma cobertura pequena e explícita, não substitui a suíte completa.
O fluxo anterior --sprint5 permanece com 288 verificações (incluindo essas quatro).
Tests/Sprint6 compila no CI, mas sua integração real não é executada ali.

### Android

Arquivo no projeto Android: .github/workflows/android.yml.
Deve ser versionado na raiz do futuro repositório Android, junto com os fontes,
gradlew, gradlew.bat, gradle/wrapper e gradle/gradle-daemon-jvm.properties.
O diretório Android ainda não tem Git/remoto; o workflow está preparado, mas não
fica ativo até o projeto ser publicado no GitHub. Não foi criado remoto fictício.

Ubuntu + Temurin 25 (mesma versão principal exigida pelo daemon existente),
Android SDK platform 37 / Build Tools 36.0.0 e Gradle Wrapper 9.6.0.
Comando: bash ./gradlew testDebugUnitTest lintDebug assembleDebug --no-daemon --console=plain.
O bash dispensa depender do bit executável do wrapper no primeiro commit Windows.
Relatórios de testes/lint são anexados mesmo em falhas; APK é anexado após sucesso.
LiveApiTest usa a condição preexistente de fixtures: sem TRAMPO_LIVE_URL,
é registrado como ignorado. Não foi removido nem disfarçado como aprovado.

## Secrets e configurações

Jwt:SigningKey continua em .NET User Secrets local, com UserSecretsId preservado.
Scripts/Start-Dev.ps1 usa o perfil http existente em Development. Não altera,
gera, imprime ou solicita uma chave a cada execução. O comando antigo
docs/Sprint6/Iniciar-Api.ps1 delega ao novo script.
Uma variável Jwt__SigningKey existente tem a precedência padrão do ASP.NET Core.
Em ambientes hospedados, configurá-la no gerenciador de secrets/ambiente do serviço.
Jwt__Issuer e Jwt__Audience também podem ser configurados por ambiente.
Não colocar chaves reais em appsettings, YAML, scripts, capturas ou logs.
Se for necessário provisionar outra máquina, fazer a configuração de User Secrets
uma única vez com uma chave forte; não sobrescrever a configuração existente.

O CI sem banco não precisa de chave JWT. Runners de integração existentes criam
chaves aleatórias apenas para seus processos de teste. Senhas/endereços de fixtures
não são credenciais de produção. A busca nos fontes/configurações não confirmou
segredos reais; não é uma auditoria completa do histórico Git ou de todos os binários.

Android Debug permanece em http://10.0.2.2:5165/. A exceção HTTP continua restrita
ao recurso Debug. ACCESS_LOCAL_NETWORK e permissões existentes foram preservados.
Release não tem endpoint padrão fictício: a propriedade trampoApiUrl deve ser
informada explicitamente, com HTTPS e barra final; a tarefa preReleaseBuild exige
host válido, sem credenciais, query ou fragmento. O domínio .invalid é recusado.
A configuração de rede principal continua proibindo cleartext. Nenhum endpoint
de produção, keystore ou assinatura de distribuição foi criado.

## Comandos locais

Executar na raiz do backend, com .NET SDK 10.0.401 e SQL Express/Xamou preparados
conforme ScriptBD.txt e documentação das sprints anteriores. A conexão atual usa
a identidade Windows, localhost\SQLEXPRESS e Trusted_Connection.

~~~powershell
# Mesmo restore/build/análise/testes/publish do workflow:
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/Test-CI.ps1

# Desenvolvimento: utiliza User Secrets persistentes.
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/Start-Dev.ps1

# Integração histórica MVC/API/SQL, cria e limpa suas próprias fixtures:
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint5

# Integração Kotlin/API/SQL, exige projeto Android, JBR e SDK locais:
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint6/Run.ps1
~~~

Bypass aplica-se somente ao processo; não é necessário alterar ExecutionPolicy
global. Não executar as integrações contra banco de produção. Os runners usam
portas locais 5177/5178 e devem ser executados sequencialmente.

Na raiz Android, com SDK configurado em local.properties (não versionado):

~~~powershell
$env:JAVA_HOME = 'C:\Program Files\Android\Android Studio\jbr'
.\gradlew.bat testDebugUnitTest lintDebug assembleDebug --console=plain
# Sem URL, este comando deve falhar:
.\gradlew.bat :app:validateReleaseApi
# Para uma implantação futura, informar a URL HTTPS REAL com -PtrampoApiUrl.
~~~

## Artefatos e Git

Backend: artifacts/backend, pacote framework-dependent que exige runtime .NET 10.
Android: app/build/outputs/apk/debug/app-debug.apk, APK de desenvolvimento.
Relatórios Android: app/build/reports e app/build/test-results.
Logs locais desta sprint ficam em artifacts/ e sprint7-*.log (ignorados).
Nenhum artefato representa implantação concluída ou Release Android distribuível.

O novo .gitignore backend exclui bin/obj, saída isolada dos testes, .vs,
configurações pessoais, logs, material de assinatura, secrets e artifacts.
521 arquivos já rastreados e agora ignorados foram retirados somente do índice
Git: permanecem no disco, inclusive alterações anteriores. As exclusões estão
staged para o próximo commit; não houve commit, push nem reescrita de histórico.
O .gitignore sozinho não corrigiria arquivos já rastreados.
O csproj também impede que docs/scripts/workflows/artifacts sejam copiados ao publish.
No Android, .gitignore foi ampliado para build de todos os módulos, .kotlin,
.idea, logs e assinatura, preservando o Wrapper versionável.

## Docker e Sprint 8

Docker foi conscientemente adiado. Conexao instancia SQL Server Express local
com autenticação integrada Windows e TrustServerCertificate=True.
Dentro de container, localhost apontaria para o próprio container, e a identidade
Windows atual não é transportada automaticamente para Linux.
Um Dockerfile isolado não resolveria rede, autenticação e certificados; produzir
uma imagem como se já fosse implantável esconderia essas dependências.

Antes de cloud: externalizar a conexão SQL preservando o fallback local,
selecionar hospedagem/identidade, configurar TLS válido e acesso mínimo ao banco,
provisionar secrets, persistir chaves Data Protection e planejar sessão MVC,
backups, schema/migrações, observabilidade, health checks, rollback e homologação.
Reduzir avisos legados e ampliar testes sem banco gradualmente. Publicar o projeto
Android em repositório próprio; executar os workflows no GitHub e configurar
branch protection. Preparar endpoint real e assinatura Android fora do Git.

## Referências

- [GitHub: build e testes .NET](https://docs.github.com/en/actions/tutorials/build-and-test-code/net)
- [Android: AGP 9.4](https://developer.android.com/build/releases/agp-9-4-0-release-notes)

Os comandos foram validados localmente. Execuções no GitHub e implantação cloud
não foram realizadas nesta sessão. Resultados finais: [validação](SPRINT-7-VALIDACAO.md).

