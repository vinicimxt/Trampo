# Auditoria final pré-Git — TRAMPO / PIM IV

Data: 12/10/2026. Escopo: Backend/Web/API, Android e Windows Forms locais. Auditoria estática de arquivos rastreados e novos, regras de ignore, workflows, documentação e histórico Git disponível; inspeção visual das 12 capturas versionáveis. Nenhum commit, push, release, deploy, limpeza do índice ou reescrita de histórico foi executado. Padrões automatizados e inspeção local não certificam ausência absoluta de segredo ou dado pessoal; a confirmação das fixtures e o diff final pertencem ao grupo.

## Backend

**Status: REVISÃO NECESSÁRIA.** Branch `main`, remoto `origin` configurado. Ao fim da auditoria, 35 arquivos rastreados modificados, 23 entradas `??` no `git status --short` (incluindo diretórios de novos documentos/evidências), nada staged. As alterações anteriores da aplicação foram preservadas e exigem revisão de diff funcional antes de selecionar arquivos para commit.

`appsettings.Development.json` contém conexão local SQL Express com autenticação integrada, sem senha SQL. `appsettings.json` não embute credencial nem JWT. Configuração de Production usa valores externos; `UserSecretsId` no projeto é apenas identificador e deve permanecer. User Secrets ficam fora do repositório. `ScriptBD.txt` contém `DROP DATABASE` e `DROP TABLE` para reconstrução local de desenvolvimento: executar somente em banco descartável, nunca em produção. A migração incremental não altera o schema nesta auditoria. O exemplo histórico comentado de conta Admin recebeu endereço reservado de exemplo no arquivo de trabalho; o histórico ainda contém um endereço não reconhecível como fixture e uma senha de exemplo, tratados abaixo.

Workflows Backend têm `contents: read` e não exibem segredo literal. `artifacts/`, logs, saídas de build, resultados e User Secrets permanecem fora do conjunto versionável. Não houve build nem execução de testes nesta auditoria, pois as mudanças aplicadas foram de ignore, exemplo comentado e documentação.

## Android

**Status: SEGURO PARA COMMIT/PUSH quanto à auditoria local**, após revisão normal do único diff (`.gitignore`). Branch `main`, remoto `origin` configurado; um arquivo modificado, nenhum novo ou staged. `local.properties` existe apenas localmente e está ignorado; não há keystore, arquivo de assinatura, APK ou AAB rastreado. `TokenStore` não contém JWT embutido. Debug usa o endereço local do emulador; Release exige URL HTTPS válida e rejeita endereço vazio/HTTP. Não existe endpoint Release público configurado, condição documentada. Manifest principal bloqueia cleartext; a exceção de desenvolvimento está na configuração Debug. Workflow Android tem permissão `contents: read` e não contém segredo literal. O último CI remoto descrito em `DEVOPS-E-CLOUD.md` falhou na preparação do Android; não há nova execução remota confirmada nesta auditoria.

## Desktop

**Status: REVISÃO NECESSÁRIA.** O diretório Windows Forms ainda não tem `.git`; portanto branch, remoto, staging, arquivos rastreados e histórico Git não são auditáveis. Foi criado `.gitignore` antes de qualquer inicialização/versionamento. O workflow próprio recebeu `contents: read`; existe apenas localmente, sem CI remoto confirmado. Fonte, Forms, Designer, `.resx`, projetos, solução, testes e Assets oficiais permanecem versionáveis. `.vs/`, `bin/`, `obj/`, `*.user`, resultados e logs ficam ignorados.

A inspeção de código não encontrou acesso direto a SQL Server, connection string, JWT hardcoded ou persistência de credencial. `TRAMPO_API_BASE_URL` configura o endpoint; o padrão HTTP é local de desenvolvimento. O token fica em `UserSession` na memória. `Assets/Trampo-Logo.png` e `Assets/Trampo.ico` são recursos da aplicação, sem screenshot privado. A validação local anterior registra build Release, 70 verificações não visuais e smoke da API em `SPRINT-12-DESKTOP.md`; não equivale a CI remoto ou inspeção visual integral das oito janelas.

## Secrets

**Secret real encontrado: NÃO, dentro do alcance desta auditoria.** A varredura dos arquivos publicáveis atuais não encontrou padrão de PAT, JWT literal, chave privada PEM, URL com credenciais ou atribuição de senha/chave de assinatura fora de fixtures. Referências a `Password`, `Jwt`, `Bearer` e `ConnectionStrings` em fonte e documentação são nomes de campo, placeholders, tratamento de autenticação ou entradas de teste. Valores de User Secrets não foram impressos nem copiados. Logs locais examinados não apresentaram esses padrões e seguem ignorados.

O histórico Backend contém um bloco SQL comentado com senha de exemplo e endereço repetido que precisa ser classificado pelo responsável: se qualquer um foi usado de fato como credencial, revogar/rotacionar primeiro e decidir depois, em procedimento separado, sobre limpeza do histórico. A amostra foi tratada como *possível dado pessoal*, sem declarar que é uma conta real. Nenhum valor é reproduzido aqui.

## Privacidade

No arquivo atual `ScriptBD.txt`, dois usos do endereço do exemplo Admin foram substituídos por `admin@example.invalid`; a linha de inserção com senha literal já estava removida da árvore de trabalho antes desta auditoria. A versão do `HEAD` ainda preserva o exemplo anterior. Seis documentos históricos de Sprint 6, 8.5 e 9 incluem caminhos locais de usuário; não foram reescritos por serem registros históricos. Documentos finais novos/atualizados usam links relativos e não exibem caminho pessoal. Não foram detectados e-mails fora de domínio de exemplo nos arquivos publicáveis atuais. Nomes exibidos em capturas de demonstração parecem fixtures, mas a origem deve ser confirmada pelo grupo.

## Gitignore

Backend: cobertura de `bin/`, `obj/`, `.vs/`, `*.user`, `*.suo`, `artifacts/`, `TestResults/`, logs, `.env*`, secrets locais e material de assinatura; foram acrescidos `coverage/` e temporários. Android: cobertura de Gradle, builds, IDE, `local.properties`, keystores e logs; foram acrescidos `key.properties`, `signing.properties`, `*.apk` e `*.aab`. Desktop: novo `.gitignore` com builds, IDE, resultados, logs, temporários, configurações locais e material de assinatura. Fontes, documentação, scripts SQL, workflows, wrapper Gradle e Assets oficiais continuam elegíveis para Git.

## Arquivos rastreados indevidamente

`git ls-files -ci --exclude-standard` não retornou arquivos no Backend nem no Android após as mudanças de ignore. **Arquivos a retirar somente do índice: nenhum identificado.** Desktop ainda não possui índice; revisar o primeiro `git status` antes de adicionar arquivos. Não houve `git rm`, alteração de staging ou limpeza massiva.

## Histórico Git

Backend: 70 commits inspecionáveis; Android: 2. Busca conservadora nas revisões disponíveis não encontrou PAT, JWT literal, PEM privado, `.env`, `local.properties`, keystore, APK/AAB ou connection string com senha como artefato histórico. O `HEAD` do Backend contém o comentário SQL Admin acima, com um endereço não exemplar repetido em três linhas e uma senha de exemplo; a correção de trabalho não o retira dos commits existentes. A classificação de autenticidade desse endereço/credencial é **pendência manual antes de push público**. Não foi feito rewrite. Desktop: sem histórico Git para revisar.

## Screenshots

Inspeção visual e de metadados PNG: as 12 imagens em `docs/evidencias/` não contêm chunks textuais ou EXIF. Nenhuma mostrou senha, token, e-mail pessoal aparente ou notificação privada. Classificação individual quanto à privacidade observável:

| Arquivo em `docs/evidencias/` | Classificação |
| --- | --- |
| `mobile-home.png` | APTA PARA PUBLICAÇÃO |
| `mobile-notificacoes.png` | APTA PARA PUBLICAÇÃO |
| `mobile-suporte.png` | APTA PARA PUBLICAÇÃO |
| `web-sprint105/admin-mobile.png` | APTA PARA PUBLICAÇÃO |
| `web-sprint105/agendamentos-mobile.png` | APTA PARA PUBLICAÇÃO |
| `web-sprint105/cadastro-mobile.png` | APTA PARA PUBLICAÇÃO |
| `web-sprint105/catalogo-mobile.png` | APTA PARA PUBLICAÇÃO |
| `web-sprint105/home-desktop.png` | APTA PARA PUBLICAÇÃO |
| `web-sprint105/home-mobile.png` | APTA PARA PUBLICAÇÃO |
| `web-sprint105/login-mobile.png` | APTA PARA PUBLICAÇÃO |
| `web-sprint105/perfil-mobile.png` | APTA PARA PUBLICAÇÃO |
| `web-sprint105/servicos-desktop.png` | APTA PARA PUBLICAÇÃO |

As **quatro capturas Desktop enviadas na conversa** não são arquivos do repositório: **NÃO PUBLICAR na forma bruta**, pois mostram outras aplicações e dados do ambiente, incluindo o nome exibido no painel. Recortar/anonimizar e rever caso sejam incorporadas ao PIM. Faltam capturas das janelas DetalheAgendamento, Servico, Locais e Local, além de redimensionamento, mensagens e fluxos visuais completos. O checklist Android/Web ainda tem jornadas não capturadas; imagens já aptas não comprovam todo o fluxo. Confirmar que todas as contas vistas nas 12 imagens são fixtures antes da publicação final.

## Documentação PIM

Conferidos `README.md`, requisitos, arquitetura, banco/MER, API, DevOps/Cloud, ágil, segurança, testes, roteiro, checklist de evidências, fechamento Sprint 10 e Sprint 12 Desktop. README e arquitetura já descreviam Web MVC (Cliente/Profissional/Admin), Android (Cliente), Desktop (Profissional), API/Services, DAO/ADO.NET e SQL Server. Android e Desktop usam a API, sem acesso direto ao SQL; Web usa sessão e Desktop JWT. Nenhum documento final afirma deploy cloud real, Play Store publicada ou Desktop multiplataforma.

Atualizados `PIM-REQUISITOS.md`, `MATRIZ-WEB-API-MOBILE.md` (nome histórico preservado), `ROTEIRO-APRESENTACAO.md`, `DEVOPS-E-CLOUD.md`, `AGIL.md` e `EVIDENCIAS-CHECKLIST.md` para incorporar o Desktop e os limites de evidência. `SPRINT-10-FECHAMENTO.md` e demais relatos históricos foram preservados. A checagem de links Markdown locais não encontrou caminho quebrado. Cloud permanece proposta, sem deploy; Docker e CD não implementados. Procedures/triggers de negócio não foram adicionados; confirmar se a rubrica oficial os exige.

## Pendências

1. **Antes de push público do Backend:** responsável confirmar se o endereço e a senha do exemplo SQL presentes no histórico pertenceram a conta real. Se sim, revogar/rotacionar a credencial e definir tratamento do histórico em tarefa separada. Se forem puramente fictícios, registrar essa confirmação. Não expor os valores na revisão.
2. Revisar o diff completo do Backend, que já possuía numerosas alterações funcionais e novos arquivos antes desta auditoria; selecionar somente os arquivos pretendidos para commit. Nenhum arquivo está staged.
3. Inicializar/configurar Git para o Desktop somente após revisar seu primeiro inventário de arquivos; então conferir branch, remoto, índice e histórico disponível. Revalidar CI remoto depois de eventual push autorizado.
4. Revisar/anonimizar as quatro capturas Desktop da conversa e completar as quatro janelas e estados visuais pendentes. Concluir as capturas de jornadas Android/Web ainda marcadas no checklist, se exigidas para a entrega.
5. Conferir a rubrica oficial do PIM: procedures/triggers, MER formal, acessibilidade, caracterização empresarial/impacto, cloud real e evidências exigidas. O projeto documenta essas limitações sem alegar implementação inexistente.
6. Diagnosticar com log completo e revalidar o CI Android remoto; a falha histórica ocorreu antes de testes, lint e APK. CI Desktop remoto ainda não existe.

## Recomendação

**NO-GO para push público conjunto neste momento.** Android está seguro na inspeção local; Backend depende da confirmação do exemplo já presente no histórico e da revisão do diff amplo; Desktop depende de Git inicializado e inventário do primeiro commit. Capturas brutas do Desktop permanecem fora da entrega. Depois dessas verificações manuais, repetir `git status`, revisar o conjunto staged e decidir os commits/push por repositório. Nenhuma ação de publicação foi executada nesta auditoria.
