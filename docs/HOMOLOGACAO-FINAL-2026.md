# Homologação Final — TRAMPO

## Ambiente

Execução local em **07/11/2026**, fuso America/Sao_Paulo. Contas usadas: **Conta de teste Cliente** e **Conta de teste Profissional**. Nenhuma credencial ou JWT consta neste documento. Não houve commit, push, merge, deploy, alteração de senha nem escrita SQL direta.

- Windows, .NET SDK 10.0.401, SQL Server Express (`MSSQL$SQLEXPRESS`) ativo e banco Development `Xamou` com autenticação integrada. A connection string foi verificada sem divulgar valores; `Jwt:SigningKey` está em User Secrets.
- Backend já estava em execução a partir de `bin/Debug/net10.0` em `http://localhost:5165/`. `GET /health`, `/` e `/api/v1/categorias` responderam 200; categorias retornou 15 registros e confirmou conexão ao banco. O processo preexistente não foi interrompido.
- Android Debug aponta para `http://10.0.2.2:5165/`; Desktop usa `http://localhost:5165/`. O AVD `Pixel_6` existe, mas encerrou com `x86_64 emulation currently requires hardware acceleration` e `Android Emulator hypervisor driver is not installed`. A ferramenta visual do Windows encerrou inesperadamente em duas inicializações. Interfaces não receberam inspeção manual tela a tela nem capturas novas.
- Alterações locais preexistentes em backend e README Android foram preservadas. Desktop começou limpo.

## Versões testadas

| Projeto | HEAD | Configuração |
| --- | --- | --- |
| Backend/Web/API | `0608614` + alterações locais preexistentes | .NET 10, Debug e Release |
| Android | `bdbed6d` + README local preexistente | Kotlin, Debug |
| Desktop | `dc6e5af` | .NET 10 Windows Forms, Debug e Release |

## Backend/API

- `dotnet restore BD-TRAMPO.csproj`, `dotnet build -c Debug` e `-c Release`: **passaram, 0 erros e 0 avisos** nas duas compilações incrementais.
- `Tests/booking-contract`: **2/2 verificações passaram** (nome do serviço/participantes no contrato de agendamento e média/contagem de avaliações). `Tests/binding-check`: **1/1 passou** (edição sem `SubcategoriaId` e rejeição da forma antiga). São runners executáveis, não testes xUnit; não foram somados a suítes históricas.
- Os projetos de teste emitiram `NU1902` e `NU1903` para a dependência transitiva `System.Security.Cryptography.Pkcs` 9.0.13. A exposição em produção não foi determinada; a dependência não foi alterada.
- `Scripts/Test-CI.ps1` e `docs/TESTES-FINAIS.md` citam `Tests/Sprint1`, `Sprint6`, `Sprint8` e `Sprint9`, ausentes neste checkout. Seus números históricos não foram reaproveitados. `Tests/auth-check` contém somente artefatos compilados, sem runner fonte. A suíte CI completa não é reproduzível neste estado.
- API real: login e `/auth/me` confirmaram os perfis Cliente e Profissional. Senha inválida e acesso protegido sem JWT retornaram **401**; Cliente em `/servicos/me`, `/locais` e confirmação profissional, e Profissional em `/perfil`, retornaram **403**. Recurso inexistente retornou **404**. Catálogo, categorias, agendamentos, notificações e suporte foram consultados. CEP válido retornou 200; inválido, 400.
- Serviço principal `288` foi criado com subcategoria real Tecnologia / Design de Sites, modalidade Online e preço 150,00. Edição para 175,00 e descrição atualizada retornou 204 e persistiu. Busca pública o encontrou; agenda expôs horário disponível. A primeira tentativa de POST com texto acentuado em codificação padrão do Windows PowerShell retornou 400; repetir com JSON UTF-8 explícito retornou 201. Não foi considerado defeito do produto.
- Agendamento `309` foi criado (201), visto por Cliente e Profissional como **[PIM-TESTE] Design de Landing Page**, confirmado (204), finalizado (204) depois do início e confirmado pelo Cliente (204). Reserva duplicada do mesmo horário retornou **409**. Avaliar antes da conclusão e finalizar antes do início retornaram **400**.
- Local `95` foi criado e editado após consulta de CEP; excluir enquanto vinculado ao serviço local `289` retornou **409**. Serviço `289` e local `95` foram removidos pelos endpoints próprios. O runner Desktop criou e removeu serviço `290` e local `96`.

## Web — Cliente

Smoke HTTP/HTML com sessão e antiforgery reais: login redirecionou ao catálogo; `/`, busca com `[PIM-TESTE]`, meus agendamentos, perfil, notificações e suporte responderam **200**. Busca e lista de agendamentos continham o nome do serviço de teste. Dashboard profissional retornou **403**. Logout respondeu 200. Navegação visual, filtros, formulários, detalhe em modal, cancelamento e acessibilidade precisam de teste em navegador.

## Web — Profissional

Smoke HTTP/HTML com sessão e antiforgery reais: login redirecionou ao dashboard; dashboard, serviços próprios, locais, agendamentos recebidos, edição do serviço e perfil responderam **200**. Dashboard, serviços e agendamentos continham a fixture. Logout respondeu 200. Criação, edição e confirmação foram exercitadas pela API, não pelos formulários Web. Disponibilidade, bloqueios, suporte e estados visuais precisam de inspeção no navegador.

## Android — Cliente

`gradlew.bat testDebugUnitTest lintDebug assembleDebug` passou após configurar `JAVA_HOME` para o JBR já instalado do Android Studio apenas no processo. A primeira tentativa sem JDK no PATH falhou antes de iniciar o Gradle. Relatórios JUnit: **36 encontrados, 35 aprovados, 1 ignorado, 0 falhas**. Lint: **0 erros, 21 avisos**. APK Debug gerado. O emulador não iniciou por falta de aceleração; login e fluxos de Cliente na UI não foram homologados. O nome do serviço foi verificado no backend e runner Desktop, não na tela Android.

## Android — Profissional

As telas e chamadas REST profissionais existem no código, mas não foram executadas no emulador. A API confirmou serviços, locais e agendamentos profissionais; rotas de avaliações recebidas, notificações, suporte e perfil têm lacunas descritas em **Bugs encontrados**. A área Profissional permanece **parcial**.

## Desktop — Cliente

Build Debug/Release passou com **0 erros e 0 avisos**. O runner não visual executou **124/124 verificações**, incluindo login multiperfil simulado, contratos de catálogo, agenda, reserva, lista com nome do serviço, avaliação, perfil, notificações, suporte, 401/403 e layout mínimo dos Forms. Não há modo de smoke real do Cliente no runner atual. A UI e o fluxo Cliente em API real não foram homologados diretamente no Desktop.

## Desktop — Profissional

`dotnet restore` e builds Debug/Release passaram sem erros ou avisos. O runner **124/124** inclui Forms profissionais, contratos, tratamento de erro e navegação não visual. Smoke real `--smoke-final` autenticou, carregou **5 agendamentos, 3 serviços e 2 locais** e limpou o JWT no logout. `--smoke` criou, editou e removeu serviço `290`; `--smoke-local` consultou CEP, criou, editou e removeu local `96`. Ambos passaram. Não houve inspeção visual de LoginForm/MainForm ou interação manual com botões.

## Testes de integração cruzada

- **API → Web:** serviço criado na API apareceu na busca Web Cliente, dashboard/serviços Web Profissional e listas de agendamentos dos dois perfis.
- **API → Desktop:** runner Desktop Profissional leu dados reais do mesmo backend e banco; CRUD de serviço/local Desktop refletiu no banco e foi limpo.
- **Web → Android, Android → Desktop, Desktop → Web Cliente e Web → Desktop Cliente:** não executados como ações de interface. Não há evidência para afirmar sincronização visual entre essas plataformas.
- O fluxo completo Cliente/Profissional **passou pela API e SQL**, mas não passou integralmente pelas três interfaces. O bug histórico de exibir apenas `Serviço #ID` foi negado nos contratos API, listas API e lista Web; Android e tela Desktop real seguem sem verificação visual.

## Segurança

401 sem JWT e com senha incorreta; 403 em papéis incompatíveis; 404 inexistente; 409 em conflito de horário, local vinculado e avaliação duplicada. Ownership do serviço e agendamento foi derivado das contas autenticadas nos fluxos executados. Não se testou acesso de um terceiro profissional por falta de outra conta autorizada. Tokens não foram impressos e senhas foram lidas somente do anexo em memória/processo. Não houve alteração de senha.

## Persistência

Consultas `SELECT` via `sqlcmd` no banco configurado confirmaram: serviço `288` a 175,00; agendamento `309` em **Finalizado** com valor final 175,00; avaliação `72` nota 5 ligada ao agendamento e profissional corretos; **5 notificações** com referência ao agendamento; suporte `8` **Aberto**. Após limpeza, serviço `288` ficou **inativo** por possuir histórico; agendamento, avaliação, notificações e chamado permaneceram por regra/integridade. Serviços auxiliares `289`/`290` e locais `95`/`96` não constam mais no SQL. Não se usou SQL para mutação.

## Avaliações

Cliente avaliou com nota 5 e comentário `[PIM-TESTE]`; consulta de estado passou a `jaAvaliado=true`; tentativa repetida retornou **409**. O serviço público mostrou média **5** e total **1** antes de ser desativado. Comentários individuais não estão disponíveis em `/api/v1/servicos/{id}/avaliacoes` (**404**). A rota profissional `/api/v1/profissionais/me/avaliacoes` também retornou **404**.

## Notificações

Criação, confirmação e finalização geraram notificações no SQL. Cliente listou notificações e marcou uma nova como lida via API (204, leitura confirmada). API de notificações para Profissional retornou **403**; a tela Android Profissional não consegue usar o contrato atual.

## Suporte

Cliente criou chamado `[PIM-TESTE] Homologação` (204) e o encontrou na listagem com status **Aberto**. A API não oferece detalhe/resposta de Admin neste fluxo. Profissional em `/api/v1/suporte` recebeu **403**. Chamado `8` foi mantido por segurança, pois não há exclusão apropriada no fluxo testado.

## Acessibilidade

Verificação estática Android: 27 layouts, 29 ocorrências de `contentDescription` e 21 de `labelFor`; lint terminou sem erros, com 21 avisos. Runner Desktop verificou controles, nomes/campos e acomodação no tamanho mínimo. TalkBack, fonte ampliada, áreas de toque, contraste real, ordem de foco/Tab e leitor de tela no Desktop não foram testados por indisponibilidade das interfaces nesta execução.

## Bugs encontrados

| ID | Gravidade | Plataforma/perfil | Reproduzir | Esperado | Obtido/causa | Status |
| --- | --- | --- | --- | --- | --- | --- |
| HF-01 | MÉDIO | Android Cliente/Profissional | GET `/api/v1/servicos/{id}/avaliacoes` e `/api/v1/profissionais/me/avaliacoes` | Dados de avaliações para telas existentes | 404; contratos REST não implementados no backend atual | Aberto; requer contrato real |
| HF-02 | MÉDIO | Android Profissional | GET `/api/v1/notificacoes` e `/api/v1/suporte` com JWT Profissional | Dados para telas presentes | 403; controllers limitados ao papel Cliente | Aberto; requer definição de acesso e comportamento profissional |
| HF-03 | BAIXO | Documentação Android | Seguir referência a `docs/ANDROID-MULTIPERFIL.md` no README | Documento existente | Alvo inexistente | Corrigido para `docs/MATRIZ-WEB-API-MOBILE.md` |

HF-01/HF-02 são lacunas já descritas parcialmente nos READMEs, mas foram reproduzidas na API atual. Não se modificou uma funcionalidade só para passar o teste. **Contagem:** 0 críticos, 0 altos, 2 médios, 1 baixo (documentação corrigida). O aviso NU1903 de dependência exige triagem separada; não há evidência suficiente para classificá-lo como falha explorável do produto.

## Bugs corrigidos

- Somente HF-03: referência quebrada no README Android corrigida preservando o conteúdo preexistente. Nenhum código funcional foi alterado.

## Pendências

1. Habilitar aceleração do emulador em ambiente apropriado e executar as telas Android Cliente/Profissional, incluindo rotação, estados vazios, falhas de API, TalkBack e nome do serviço no agendamento.
2. Recuperar a ferramenta visual do Windows ou testar manualmente em navegador e WinForms. Repetir login, busca, CRUD, agendamento, avaliação, suporte, foco e contraste em ambos os perfis.
3. Resolver HF-01/HF-02 com definição explícita de escopo e autorização; repetir testes das telas dependentes.
4. Restaurar ou atualizar os runners históricos referidos por `Scripts/Test-CI.ps1`/`docs/TESTES-FINAIS.md`; hoje não existem no checkout. Triar NU1902/NU1903.
5. Completar testes cruzados por ações reais nas três interfaces. Validar comentários publicados e avaliação recebida quando houver contrato REST.
6. Fixtures mantidas por segurança: serviço inativo `288`, agendamento `309`, avaliação `72`, chamado `8` e notificações associadas. Todos pertencem a esta execução. Não excluir histórico nem chamado por SQL.

## Matriz de homologação

`PARCIAL` indica teste de API/HTTP/runner sem confirmação visual da plataforma. `N/A` indica função não destinada ao perfil. `NÃO IMPLEMENTADO` indica contrato ausente/restrito.

| Funcionalidade | Web Cliente | Web Profissional | Android Cliente | Android Profissional | Desktop Cliente | Desktop Profissional | Resultado |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Login/logout | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | API/Web e runner passaram; UI pendente |
| Perfil | PARCIAL | PARCIAL | PARCIAL | NÃO IMPLEMENTADO | PARCIAL | PARCIAL | Rota API Profissional 403 |
| Busca/categorias | PARCIAL | N/A | PARCIAL | N/A | PARCIAL | N/A | API e HTML passaram |
| Serviços | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | CRUD real API/Desktop runner |
| Locais/CEP | N/A | PARCIAL | N/A | PARCIAL | N/A | PARCIAL | CRUD/conflito real API/Desktop runner |
| Disponibilidade | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | Agenda real API; UI pendente |
| Agendamento | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | Fluxo real API/SQL |
| Confirmação/recusa | N/A | PARCIAL | N/A | PARCIAL | N/A | PARCIAL | Confirmação API; recusa não executada |
| Finalização | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | Finalização/conclusão API |
| Cancelamento | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | Contratos testados; ação real não executada |
| Avaliação | PARCIAL | PARCIAL | PARCIAL | NÃO IMPLEMENTADO | PARCIAL | PARCIAL | Criação/duplicidade API; telas pendentes |
| Notificações | PARCIAL | PARCIAL | PARCIAL | NÃO IMPLEMENTADO | PARCIAL | PARCIAL | Cliente API; Profissional 403 |
| Suporte | PARCIAL | PARCIAL | PARCIAL | NÃO IMPLEMENTADO | PARCIAL | PARCIAL | Cliente API; Profissional 403 |

## Resultado final

**REVISÃO NECESSÁRIA.** O backend, banco, builds, runners disponíveis e fluxo principal por API/SQL passaram. A homologação completa de Web, Android e Desktop não pode ser declarada porque o emulador e a automação visual falharam, os testes cruzados por interface não foram executados e há dois gaps REST reproduzidos. Não foi identificado bug crítico nem perda de dados.

### Resumo objetivo

| Item | Resultado |
| --- | --- |
| Backend iniciou / banco conectado | SIM / SIM |
| Web Cliente / Web Profissional | PARCIAL / PARCIAL |
| Android Cliente / Android Profissional | PARCIAL / PARCIAL |
| Desktop Cliente / Desktop Profissional | PARCIAL / PARCIAL |
| Fluxo end-to-end | OK na API/SQL; não homologado entre UIs |
| Serviço criado / editado | SIM / SIM |
| Agendamento criado / profissional confirmou / atendimento finalizado | SIM / SIM / SIM |
| Avaliação criada / duplicada bloqueada | SIM / SIM (409) |
| Persistência SQL / autorização / 401 / 403 | SIM / SIM / SIM / SIM |
| Builds | Backend Debug/Release 0/0; Desktop Debug/Release 0/0; Android Debug passou |
| Testes | Backend contrato 2/2, binding 1/1; Desktop 124/124 + 3 smokes reais; Android 35/36 aprovados, 1 ignorado; lint 0 erros/21 avisos |
| Bugs críticos / altos / médios / baixos | 0 / 0 / 2 / 1 |
| Código corrigido | Nenhum; somente README Android |
| Fixtures restantes | Serviço 288 inativo; agendamento 309; avaliação 72; suporte 8; notificações vinculadas |
| Documentação atualizada | Este relatório e referência do README Android |
| Git | Sem commit/push; estados finais documentados abaixo |

### Estado Git final

Coleta antes da última leitura de status. As alterações de código e documentação já existentes no backend e a alteração anterior no README Android não foram revertidas. **Arquivos criados/editados nesta homologação:** `docs/HOMOLOGACAO-FINAL-2026.md`, `docs/evidencias-homologacao/README.md` e `.gitkeep` nas três subpastas; correção de uma referência no `README.md` Android. Artefatos locais ignorados: `artifacts/homologacao/fixture.json`, sem credenciais.

Backend/Web/API:

```text
 M Api/Contracts/ClienteContracts.cs
 M Api/Contracts/HttpContracts.cs
 M Controllers/Api/ClienteAtendimentoApiController.cs
 M Controllers/Api/ServicosApiController.cs
 M DAO/AgendamentoDAO.cs
 M DAO/ServicoDAO.cs
 M DAO/SuporteDAO.cs
 M Models/Servico.cs
 M README.md
 M Services/ClienteAtendimentoService.cs
 M Services/ServicoService.cs
 M docs/API-FINAL.md
 M docs/ARQUITETURA-FINAL.md
 M docs/MATRIZ-WEB-API-MOBILE.md
?? Tests/booking-contract/
?? docs/HOMOLOGACAO-FINAL-2026.md
?? docs/SPRINT-13-DESKTOP-CLIENTE-AUDITORIA.md
?? docs/SPRINT-13-DESKTOP-MULTIPERFIL.md
?? docs/evidencias-homologacao/
```

Android:

```text
 M README.md
```

Desktop: limpo (`git status --short` sem saída). Nenhum commit, push ou deploy foi feito.
