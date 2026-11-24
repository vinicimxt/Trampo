# Integracao online dos clientes - 23/11/2026

**Classificacao: INTEGRACAO ONLINE COM RESSALVAS.** A API publica, o build Android online e o cliente HTTP Desktop foram validados. A instalacao e o login no Android, o login pela janela Desktop e a prova de um agendamento compartilhado entre os tres clientes permanecem pendentes.

## Ambiente e API

- URL online: `https://trampo.tryasp.net/`.
- `GET /health`: HTTP 200, corpo `Healthy`.
- `GET /`: HTTP 200, pagina HTML recebida (39.736 bytes). Nao houve navegacao funcional da Web.
- Conta ficticia Cliente criada por `POST /api/v1/auth/register` (201), sem dados pessoais reais. A senha e o JWT nao foram gravados em arquivo ou neste relatorio.
- `POST /api/v1/auth/login`: 200. `GET /api/v1/auth/me`: 401 sem token e 200 com o JWT da conta ficticia.
- `GET /api/v1/categorias`: 200, 15 categorias no smoke Desktop.
- `GET /api/v1/servicos`: 200, lista vazia. Sem servico ativo para testar detalhe e reserva.
- `GET /api/v1/agendamentos` autenticado: 200, lista vazia. `GET /api/v1/servicos/me` com Cliente: 403. `GET /api/v1/servicos/2147483647`: 404.
- Nao foi produzido um conflito legitimo para conferir 409. Nenhum contrato, regra de negocio, script SQL ou dado existente foi alterado.

## Android

- Debug local padrao: `http://127.0.0.1:5165/` com ADB reverse. Para Emulator sem reverse: `-PtrampoDebugApiUrl=http://10.0.2.2:5165/`. O backend local usa `http://localhost:5165/`.
- Debug online: `./gradlew assembleDebug -PtrampoApiUrl=https://trampo.tryasp.net/` (ou `.\gradlew.bat` no Windows). `trampoDebugApiUrl` continua tendo prioridade quando fornecida; sem propriedades, o Debug permanece local.
- `app/build/generated/source/buildConfig/debug/com/example/trampo/BuildConfig.java` gerado com `API_BASE_URL = "https://trampo.tryasp.net/"`, incluindo a barra final. Antes do build online, o mesmo arquivo gerado continha `http://127.0.0.1:5165/` no build local. O APK online foi gerado em `app/build/outputs/apk/debug/app-debug.apk` (7.637.465 bytes).
- `testDebugUnitTest`: 35 aprovados, 1 ignorado (teste de API live que exige fixtures); `lintDebug` e `assembleDebug`: sucesso. O build Debug online e `validateReleaseApi` com a URL HTTPS: sucesso. `validateReleaseApi` com `http://trampo.tryasp.net/`: falhou como esperado.
- Release mantem `cleartextTrafficPermitted=false`. Somente o recurso de rede Debug permite HTTP para `10.0.2.2`, `localhost` e `127.0.0.1`. Retrofit usa `BuildConfig.API_BASE_URL`.
- `adb devices` nao listou aparelho ou emulador. Portanto, nao houve `installDebug`, `adb reverse`, logcat, login Android, Home, catalogo ou agendamentos no aparelho. O APK online nao depende de ADB reverse pela URL compilada, mas o trafego real do Android ainda nao foi observado.

## Desktop

- URL local padrao em `TrampoApiClient`: `http://localhost:5165/`. A variavel `TRAMPO_API_BASE_URL=https://trampo.tryasp.net/` e lida na inicializacao do processo e nao requer recompilacao. `HttpClient` usa HTTPS publico sem bypass de certificado encontrado.
- `dotnet build Trampo.Desktop.slnx --configuration Release`: sucesso, zero avisos e zero erros. Runner existente: 124 verificacoes aprovadas.
- Smoke online somente de leitura, usando o `TrampoApiClient` real: login e `/api/v1/auth/me` confirmaram perfil Cliente; categorias (15), catalogo (0), agendamentos (0) e perfil carregaram. O JWT foi limpo da sessao ao final.
- A aplicacao WinForms foi iniciada com a variavel online e exibiu a janela de acesso. A automacao da interface Windows falhou ao iniciar, antes de inserir credenciais; portanto, login visual, painel e navegacao pela janela nao foram validados. O processo de teste foi fechado.

## Fluxo integrado e limites

A mesma conta online criada pela API foi autenticada pelo cliente HTTP do Desktop, comprovando que ambos acessaram o ambiente publicado. A conta nao foi criada pela interface Web, nao foi usada no Android, e a lista publica de servicos estava vazia. Assim, nao foi possivel demonstrar servico visto em outro cliente, reserva, profissional recebendo agendamento nem identidade do mesmo banco por um fluxo completo entre Web, Android e Desktop. O teste de perfil Profissional tambem ficou pendente por falta de uma conta ficticia desse perfil.

`http://localhost:5165/health` nao respondeu nesta execucao. Os caminhos locais permanecem configurados e o BuildConfig local foi gerado antes do build online, mas nao houve regressao funcional contra a API local.

## Arquivos alterados

- Android: `app/build.gradle.kts`, `README.md`.
- Desktop: `Tests/Trampo.Desktop.Tests/Program.cs`, `README.md`.
- Backend/Web: `docs/INTEGRACAO-ONLINE-CLIENTES.md` (somente este relatorio).

O diretorio nao rastreado `Trampo.Desktop/Properties/` ja existia antes desta tarefa e nao foi modificado. A verificacao nao incluiu novo deploy. Nao foram adicionados secrets aos clientes. Scripts opcionais de inicializacao nao foram criados porque os comandos Gradle e `dotnet run` documentados ja atendem aos dois modos.
