# Segurança final — auditoria de entrega

Revisão local de 26/09/2026. Escopo: arquivos rastreados/ignorados, configurações, contratos, autenticação, rede Android, scripts de testes e metadados SQL. Não equivale a pentest nem auditoria exaustiva de todo o histórico Git.

## Resultado e evidências

- Baseline: 278 arquivos rastreados backend e 100 Android; zero arquivos rastreados que correspondam ao `.gitignore` em ambos. Working trees inicialmente limpas.
- Busca por formatos de tokens/chaves privadas/JWT e atribuições suspeitas não identificou segredo operacional nos fontes rastreados examinados. Varredura por padrões tem limites e não garante ausência de todos os tipos de segredo.
- User Secrets fica fora dos repositórios. Não foram impressos os valores. `UserSecretsId` no csproj é identificador de configuração, não chave JWT. Configuração SQL Development usa autenticação integrada, sem senha de banco.
- `.gitignore` cobre bin/obj/build, artifacts/logs, .env, arquivos locais e material de assinatura. Workflows, Gradle wrapper, docs e scripts permanecem versionáveis. Não incluir APKs ou cópia de banco no commit acadêmico.
- ScriptBD continha exemplo administrativo comentado com senha literal; foi substituído por orientação para usar o fluxo de cadastro com hash, preservando o comentário histórico e sem mudar schema ou código executável. Não usar INSERT de senha em texto puro para criar conta.
- Senhas de fixtures existentes no runner Sprint1 são somente de teste e não são credenciais de produção. Demais runners geram valores efêmeros. Logs/screenhots entregáveis não devem conter senha, JWT, conexão ou dados reais.
- Backend valida JWT, perfil e ownership; Web usa sessão e antiforgery; SQL é parametrizado nos fluxos auditados. Hash de senha usa Identity e migra hash legado no login válido.
- Android Release usa configuração de rede que bloqueia HTTP; Debug libera apenas host do emulador. Gradle exige URL HTTPS para Release. Não foi produzido APK Release assinado nem certificado endpoint real.
- Token Android fica em SharedPreferences privado e excluído de backup; não afirmar criptografia em repouso por Keystore. Logout e troca de senha não revogam JWT imediatamente.

## Limites conhecidos

Sem refresh token, revogação central, secret store cloud provisionado ou observabilidade avançada. Sessão MVC em memória. A aplicação contém módulos legados e 109 avisos de nulabilidade. Erros técnicos API são genéricos ao cliente, mas logs de exceção exigem acesso restrito e revisão antes de divulgação. Teste da Sprint8 procura marcador de senha conhecido; não prova ausência universal de dados sensíveis.

Não foram auditados todos os objetos de cada commit antigo, backup externo, logs de terceiros ou configurações privadas do GitHub. Secrets não encontrados no HEAD não significam que jamais existiram no histórico. Não houve reescrita de histórico nem rotação automática de credenciais.

## Checklist antes de publicar

1. Conferir `git status --short`, `git diff --check` e arquivos efetivamente staged.
2. Verificar `git ls-files -ci --exclude-standard` nos dois repositórios: resultado esperado vazio.
3. Revisar imagens individualmente; usar apenas fixture identificada, sem senha/token/contato real.
4. Manter `artifacts/`, logs, APK, local.properties e User Secrets fora do commit.
5. Revisar PR/diff e autorizar push; consultar o CI do SHA enviado, sem reaproveitar resultado de outra revisão.

Nenhuma alteração de comportamento de autenticação, permissões ou schema foi realizada na Sprint10. [Resultados de regressão](TESTES-FINAIS.md).
