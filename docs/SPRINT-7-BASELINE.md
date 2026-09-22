# Baseline da Sprint 7 — 21/09/2026

Auditoria realizada antes das edições. Backend em Desktop/Trampo, Git origin
https://github.com/vinicimxt/Trampo.git, HEAD 72f3caf. Já havia alterações de código,
documentação e binários; foram preservadas. Android em AndroidStudioProjects/TRAMPO,
sem .git e sem remoto. Nenhum AGENTS.md encontrado nos projetos.

- Backend: .NET SDK 10.0.401, net10.0, solução BD-TRAMPO.sln contém apenas a aplicação.
  Tests/Sprint1 e Tests/Sprint6 são runners console, não VSTest/xUnit.
- dotnet restore BD-TRAMPO.sln: aprovado.
- dotnet build BD-TRAMPO.sln -c Release --no-restore: aprovado, 109 avisos, zero erros.
- powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint5:
  288 verificações aprovadas, zero falhas. SQL Express/Xamou real.
- powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint6/Run.ps1:
  6 verificações aprovadas e um teste LiveApiTest Kotlin aprovado contra API/SQL reais.
- Android: AGP 9.4.1, Wrapper Gradle 9.6.0 com SHA-256 configurado,
  daemon Java 25 (JBR 25.0.3 local), bytecode Java 11, compile/target SDK 37,
  minSdk 24, Build Tools 36.0.0.
- gradlew.bat testDebugUnitTest lintDebug assembleDebug: aprovado.
  24 testes descobertos, 23 aprovados, 1 ignorado (LiveApiTest requer fixtures),
  zero falhas/erros. Lint: zero erros, 10 avisos (9 de versões e 1 TextFields).
  Aviso adicional Robolectric sobre acesso nativo no Java 25.
- PowerShell bloqueia scripts por padrão; Bypass foi usado apenas no processo.
- Backend sem .gitignore. 495 arquivos rastreados nos diretórios gerados inspecionados.
- Scripts existentes: Tests/Sprint1/Run.ps1, Tests/Sprint6/Run.ps1 e
  docs/Sprint6/Iniciar-Api.ps1. O último gerava nova chave JWT a cada execução.
- appsettings*.json sem chave JWT; UserSecretsId presente no csproj.
  Conexao contém autenticação integrada Windows, sem senha SQL.
- Android mantém HTTP apenas para 10.0.2.2 no recurso Debug e bloqueia cleartext
  em Release. Release já bloqueava o endpoint example.invalid na tarefa preReleaseBuild.
- Busca por candidatos a secrets nos fontes/configurações encontrou referências,
  fixtures de teste e campos de login; nenhum segredo real confirmado.
  Não foram exibidos User Secrets, tokens nem credenciais do ambiente.
  Esta auditoria não certifica todo o histórico Git nem conteúdo binário.

