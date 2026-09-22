# Sprint 6 — Autenticação e Rede

## Sessão

A autenticação usa access token JWT sem refresh token. A Sprint 5.5 foi interrompida antes de implementação e não é pré-requisito desta Sprint.

`SessionManager` usa `SharedPreferences` privadas e guarda somente `access_token`. Não grava senha, CPF, usuário ou endereço. Backup/transferência excluem preferências; `allowBackup` está desabilitado. É armazenamento privado adequado ao escopo acadêmico desta V1, sem Android Keystore.

`AuthInterceptor` envia Bearer apenas quando há token e nunca no login. 401 limpa apenas o token usado na chamada, evitando que resposta atrasada de sessão antiga apague token novo. Não há logging HTTP. Redirect automático e retry estão desabilitados.

Logout limpa sessão local e pilha de navegação. O JWT emitido continua válido no servidor até expirar, pois não há revogação remota. Ao expirar, o usuário faz novo login.

## Configuração

`BuildConfig.API_BASE_URL` é central:

- Debug: `http://10.0.2.2:5165/`, porta real do perfil HTTP.
- Release: propriedade Gradle `trampoApiUrl`, HTTPS e terminada em `/`.
- Sem URL explícita, `validateReleaseApi` bloqueia release com placeholder.

A configuração principal proíbe cleartext. Só debug permite HTTP para `10.0.2.2`. Não há TrustManager permissivo nem SSL desabilitado. A única permissão é INTERNET; não há GPS.

Timeouts: conexão 10s, leitura 15s e chamada 20s. O Android não chama ViaCEP diretamente. O alias `10.0.2.2` segue a [documentação do emulador](https://developer.android.com/studio/run/emulator-networking); HTTP debug usa [Network Security Config](https://developer.android.com/privacy-and-security/security-config).

## Execução local

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File docs/Sprint6/Iniciar-Api.ps1
```

O script reutiliza `Jwt__SigningKey` ou gera chave aleatória apenas no processo. Não imprime nem grava o segredo. Reiniciar com chave nova invalida tokens anteriores.

No Android Studio, abrir o projeto `TRAMPO`, sincronizar Gradle, criar/iniciar AVD e executar debug. Usar conta cliente cadastrada pelo Web. Dispositivo físico não usa `10.0.2.2`; exige acesso próprio ao host ou endpoint HTTPS. Publicação/configuração de produção não foram executadas.
