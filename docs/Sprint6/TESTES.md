# Sprint 6 — Testes

## Android local

No projeto Android:

```powershell
$env:JAVA_HOME = 'C:\Program Files\Android\Android Studio\jbr'
.\gradlew.bat :app:assembleDebug :app:testDebugUnitTest :app:lintDebug --console=plain
```

Resultado: 23 aprovados, zero falhas. São 18 testes novos com MockWebServer, quatro novos com Robolectric e um teste original do template. O `LiveApiTest` fica ignorado nessa execução porque exige fixtures próprias.

Os testes cobrem Bearer, login/me, logout, 401/403, token antigo, perfil cliente, DTOs, paginação, agenda, payload permitido, cancelamento, CEP, status 400–500, timeout, cancelamento de coroutine, prevenção de reenvio, launcher→login, validação dos campos, inflação de todas as telas XML e sessão privada.

## Kotlin contra API e SQL reais

Na raiz do backend:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint6/Run.ps1
```

O runner compila a API, cria duas contas temporárias, profissional, local e três ofertas; inicia a API em 127.0.0.1:5179 com JWT efêmero; executa o mesmo Retrofit/interceptor/repositórios do app; cria reservas Online, Local e Domicilio; consulta lista/detalhes; cancela a Online; confirma dados no SQL e remove somente os IDs da execução em `finally`.

Resultado observado: `LiveApiTest` aprovado e seis verificações do runner aprovadas. Foram confirmadas três reservas, duas pendentes, uma cancelada e dois snapshots físicos. Credenciais/JWT não são impressos nem persistidos. O Android continua sem acesso SQL; SQL existe apenas no runner Windows.

## Build e limites

APK debug: `C:\Users\Vinicius\AndroidStudioProjects\TRAMPO\app\build\outputs\apk\debug\app-debug.apk`, 7.331.103 bytes.

Lint: zero erros e 10 avisos. Restam sugestões de atualização de dependências e a heurística sobre número do endereço ser texto; ele permanece texto para aceitar letras e “s/n”. Não houve supressão de lint. O Robolectric/JBR registra aviso de acesso nativo futuro, sem falha.

Esta Sprint não reexecutou os 288 testes históricos do backend porque não alterou funcionalidade da API. O teste Kotlin real recompilou a aplicação sem warnings/erros. A validação não comprova visual em dispositivo, TalkBack, comportamento OEM nem exatamente uma criação após queda de rede.
