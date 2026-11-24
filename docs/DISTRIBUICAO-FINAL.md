# Distribuicao local TRAMPO - 23/11/2026

## Resultado

- **APK COM RESSALVAS.** `dist/TRAMPO.apk` e instalavel e usa a API online, mas e um build Debug assinado pela chave Android Debug. Nao havia keystore Release propria; o Release compilado ficou sem assinatura e nao foi entregue.
- **DESKTOP COM RESSALVAS.** `dist/Trampo.Desktop.exe` e um publish unico `win-x64`, self-contained e abriu a janela de acesso. O cliente HTTP Release autenticou online sem configuracao manual, mas login pela janela, perfil Profissional e execucao em outro computador sem .NET nao foram testados.

A API publica `https://trampo.tryasp.net/health` respondeu HTTP 200 com `Healthy`. A verificacao descrita neste relatorio nao incluiu novo deploy nem publicacao em loja.

## Arquivos de entrega

| Arquivo | Tamanho | SHA-256 |
| --- | ---: | --- |
| `C:\Users\Vinicius\Desktop\Trampo\dist\TRAMPO.apk` | 7.637.465 bytes | `78999D1B7919F3D7A09ABEA04190C0D9FAAA072B9153C8369417B22DF9A1ECEA` |
| `C:\Users\Vinicius\Desktop\Trampo\dist\Trampo.Desktop.exe` | 116.594.860 bytes | `EF32556496A92DDF5620379850031F1ACC36A0F4E26D89F0C0ABF9513C736FB7` |
| `dist/README-ANDROID.txt` | 426 bytes | Instrucoes para instalar no celular. |
| `dist/README-DESKTOP.txt` | 462 bytes | Instrucoes para executar no Windows. |

A pasta `dist/` foi ignorada pelo Git para evitar commit acidental dos binarios grandes.

## Android

- Comando do APK entregue: `assembleDebug -PtrampoApiUrl=https://trampo.tryasp.net/`. A URL exata com barra final aparece no `BuildConfig` gerado e em um dos arquivos DEX do APK. As strings DEX nao contem os literais locais `http://127.0.0.1:5165/` ou `http://10.0.2.2:5165/`.
- `apksigner verify`: assinatura v2 valida, 1 signatario, certificado `CN=Android Debug`. A chave de desenvolvimento nao serve como identidade de publicacao definitiva. O APK tem `android:debuggable=true`; `android:testOnly` nao aparece no manifesto.
- `aapt dump badging`: pacote `com.example.trampo`, `versionCode=1`, `versionName=1.0`, `minSdk=24`, `targetSdk=37`, rotulo `TRAMPO`.
- `testDebugUnitTest`: 35 aprovados, 1 ignorado. `lintDebug`, `assembleDebug` e `assembleRelease -PtrampoApiUrl=https://trampo.tryasp.net/`: sucesso.
- O Release gerou `app-release-unsigned.apk`, nao copiado para `dist`. Seu `BuildConfig` gerado usa `https://trampo.tryasp.net/`, e `app/src/main/res/xml/network_security_config.xml` bloqueia cleartext. O Debug entregue permite HTTP apenas para os hosts locais de desenvolvimento em seu recurso de rede, embora a URL ativa compilada seja HTTPS online.
- Nao havia aparelho ou emulador em `adb devices`; instalacao manual, abertura, login e uso pelo celular nao foram observados. O APK entregue pode ser instalado pelo gerenciador de arquivos, sem Android Studio, USB ou ADB.
- Foi adicionada configuracao opcional via `keystore.properties` ignorado pelo Git e `keystore.properties.example` sem segredos. Para trocar este APK pelo Release definitivo, o responsavel precisa gerar sua propria chave com `keytool`, preencher `keystore.properties` localmente e executar `assembleRelease -PtrampoApiUrl=https://trampo.tryasp.net/`. O README Android detalha o processo. A assinatura com outra chave pode exigir desinstalar este APK Debug antes da nova instalacao.

## Desktop

- Publish: `dotnet publish Trampo.Desktop/Trampo.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishTrimmed=false`. A pasta de publish continha o EXE e um PDB opcional; apenas o EXE foi copiado. O EXE isolado em `dist/` iniciou e mostrou `TRAMPO | Acesso Cliente e Profissional`.
- Build Release: sucesso, zero avisos/erros. Runner existente: 124 verificacoes aprovadas. Build Debug: sucesso, zero avisos/erros.
- Sem `TRAMPO_API_BASE_URL`, o cliente Release usou a API online: login da conta ficticia, `/api/v1/auth/me` com perfil Cliente, 15 categorias, catalogo e agendamentos consultados, perfil carregado e JWT limpo no logout. O catalogo online retornou lista vazia; detalhe e reserva nao puderam ser testados. A interface grafica nao foi automatizada para fazer login ou navegar como Cliente/Profissional.
- O fallback Debug continua `http://localhost:5165/`. `TRAMPO_API_BASE_URL` continua sobrescrevendo a URL em ambos os builds, inclusive `http://localhost:5165/` para desenvolvimento local. O cliente usa `HttpClient` padrao sem bypass de certificado.
- O EXE nao recebeu assinatura de editor; o Windows SmartScreen pode mostrar aviso. Self-contained elimina a dependencia de instalar .NET separadamente segundo a configuracao de publish, mas nao houve teste em uma maquina limpa sem .NET.

## Seguranca e limites

O codigo dos dois clientes foi pesquisado por connection strings, JWT signing key, credenciais de hospedagem, token GitHub e bypass TLS; nenhum padrao de segredo foi encontrado. `keystore.properties`, arquivos de chave, senhas e tokens nao foram incluidos em `dist/`. O Android e o Desktop acessam apenas a API; nao incluem conexao direta ao SQL Server. Este exame de padroes nao substitui uma auditoria completa de binarios.

O teste Android funcional, o login visual Desktop, o perfil Profissional e o fluxo de agendamento compartilhado entre clientes continuam pendentes. O catalogo publico online estava vazio na verificacao. O diretorio nao rastreado `Trampo.Desktop/Properties/` ja existia antes desta tarefa e nao foi modificado.

## Arquivos de codigo e documentacao alterados

- Android: `app/build.gradle.kts`, `README.md`, `keystore.properties.example` (alem de alteracoes preexistentes da integracao online).
- Desktop: `Trampo.Desktop/Api/TrampoApiClient.cs`, `README.md` (alem do runner de teste preexistente da integracao online).
- Backend/Web: `.gitignore`, este relatorio e a pasta local `dist/` ignorada. O backend e o banco nao foram alterados.
