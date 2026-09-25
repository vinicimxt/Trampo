# Sprint 9 — funcionalidades Mobile do cliente

## Auditoria anterior à implementação — 23/09/2026

Conferidos Controllers MVC/API, Views, Services, DAOs, Models/DTOs, Android (Activities/ViewModels/Repositories/Retrofit/TokenStore/XML/testes) e metadados do SQL Express Xamou. Não há AGENTS.md nos projetos consultados. Git inicial: somente relatório 8.5 não rastreado; Android sem Git.

| Funcionalidade | Web | API antes | Android antes | Observação |
| --- | --- | --- | --- | --- |
| Cadastro | Sim, autentica | Não | Não | Nome/email 100, telefone opcional 20, senha 8–1024; Identity PasswordHasher |
| Login/logout | Sim | JWT/saída local | Sim | Mobile exclusivamente cliente |
| Perfil/edição | Nome/email/telefone; edita nome/telefone | auth/me sem telefone | Saudação | Email somente leitura |
| Senha | Atual/nova/confirmação | Não | Não | Regras MVC + DAO + Seguranca reutilizáveis |
| Catálogo | Sim | Ativos/paginado | Sim/paginado | Sem filtros API |
| Categorias/subcategorias | Sim | SubcategoriaId | Não | Tabelas reais |
| Busca/filtros | Texto/categoria/subcategoria | Não | Não | Modalidade/preço reais |
| Detalhe/profissional | Sim | Serviço sem nome público | Serviço | Descrição profissional não persistida |
| Agenda/agendar/cancelar | Sim | Sim | Sim | Ownership, agenda e endereço reais |
| Conclusão | Sim | Sim | Não | AguardandoCliente → Finalizado |
| Avaliações | Sim | POST existente | Não | Nota 1–5, comentário 500, conclusão obrigatória |
| Notificações | Lista/leitura | Não | Não | DAO por usuário, data/título/mensagem/lida |
| Suporte | Envio | Não | Não | Inserção reutilizável; sem chat |

SQL: Usuarios.Email, Clientes.UsuarioId e Avaliacoes.AgendamentoId únicos. Tabelas ContatoSuporte/Notificacoes/categorias existentes. Nenhuma migração necessária.

## Escopo escolhido

Cadastro automático; perfil nome/telefone e email somente leitura; senha com regras compartilhadas; busca textual/modalidade no servidor mantendo paginação; categoria/subcategoria/nome profissional reais no detalhe; conclusão/avaliação; notificações; suporte. Extrair regras de conta/suporte para Services reutilizados pelo MVC/API. Cadastro cliente transacional.

Adiar seletores de categoria/subcategoria, faixa de preço, nota agregada e perfil profissional expandido. Busca incluirá nomes reais de serviço/profissional/categoria/subcategoria. Descrição profissional não existe na tabela. Revogação imediata de JWT após troca de senha não existe; preservar limite atual. Demais exclusões do planejamento mantidas.

## Resultado — 25/09/2026

Cliente consegue criar conta no Android, entrar automaticamente, editar nome/telefone, alterar senha, pesquisar catálogo, consultar profissional/categoria/subcategoria reais, acompanhar e concluir reserva, avaliar atendimento concluído, consultar/marcar notificações e enviar suporte. Preservados Kotlin/XML, Activity → ViewModel → Repository → Retrofit → API e MVC/API → Services → DAO → SQL. Sem dependência ou migração nova. O login continua restrito a cliente no Android.

Cadastro segue o comportamento Web de autenticar automaticamente. API cria apenas `cliente`, sem aceitar papel ou usuário no corpo, e insere `Usuarios` e `Clientes` na mesma transação. Web cliente passou a usar o mesmo `ContaService`; o cadastro profissional MVC permanece no fluxo legado. Perfil edita nome/telefone; e-mail é somente leitura, como na Web. MVC de perfil e senha também chama o Service compartilhado. `ClienteAtendimentoService` compartilha validação/DAO de suporte com MVC e fornece central de notificações ao cliente.

Catálogo estendido com busca por serviço, profissional, categoria e subcategoria, modalidade Online/Local/Domicilio e paginação SQL preservada. O detalhe recebe nome público do profissional e nomes reais de categoria/subcategoria. Não expõe telefone, e-mail, contato interno, link da reunião, coordenadas ou dados administrativos no catálogo.

## Endpoints

Novos:

| Método e rota | Acesso | Comportamento |
| --- | --- | --- |
| POST `/api/v1/auth/register` | Público, limite de requisições existente | Cria somente cliente; resposta 201 com JWT para login automático |
| GET `/api/v1/perfil` | Cliente JWT | Nome, e-mail e telefone próprios |
| PUT `/api/v1/perfil` | Cliente JWT | Edita nome e telefone; corpo não aceita ID/email |
| PUT `/api/v1/perfil/senha` | Cliente JWT, limite existente | Valida senha atual, tamanho e confirmação; hash Identity existente |
| GET `/api/v1/notificacoes?pagina=&tamanho=` | Cliente JWT | Lista paginada por usuário com data e estado |
| POST `/api/v1/notificacoes/{id}/ler` | Cliente JWT | Atualiza apenas registro do usuário autenticado |
| POST `/api/v1/suporte` | Cliente JWT, limite existente | Grava chamado para identidade JWT |
| GET `/api/v1/agendamentos/{id}/avaliacao` | Cliente participante | Elegibilidade e estado já avaliado |

Reutilizados: `POST /api/v1/auth/login`, `GET /api/v1/auth/me`, `GET /api/v1/servicos`, `GET /api/v1/servicos/{id}`, agenda, criação/lista/detalhe/cancelamento de agendamento, `POST /api/v1/agendamentos/{id}/confirmar-conclusao`, `POST /api/v1/agendamentos/{id}/avaliacao`. A listagem de serviços aceita novos parâmetros opcionais `busca` e `atendimento`; resposta de serviço ganhou `nomeProfissional`, `categoria` e `subcategoria`. Parâmetros existentes continuam válidos.

## Segurança e regras

Ownership deriva do JWT e é conferido com usuário persistido. IDs de usuário em corpos de perfil/cadastro/suporte são rejeitados por `JsonUnmappedMemberHandling.Disallow`. Email único no SQL e conflito tratado como 409; cadastro transacional evita usuário sem linha em Clientes. Senha atual é exigida para alteração, e nenhum contrato retorna hash. Comentário de avaliação permanece limitado a 500 caracteres, nota a 1–5 e índice único por agendamento impede duplicidade. A conclusão respeita transição do DAO. Erros seguem `ErroResponse` existente. O rate limit por IP de 10 chamadas/minuto foi mantido; o runner inicia processo temporário novo entre grupos de testes para não exceder esse limite.

## Telas e UX Android

Criadas: Cadastro, Meu perfil, Alterar senha, Notificações, Ajuda/Suporte e Avaliação. Login ganhou `Criar conta`; Home apresenta acessos à conta, notificações e suporte. Catálogo recebeu campo de busca e modalidade; detalhe do serviço mostra informações públicas reais. Detalhe do agendamento oferece Confirmar conclusão em `AguardandoCliente` e Avaliar em `Finalizado`, com confirmação antes das mutações. Estados de carregamento/erro, botões, labels, campos, rolagem, tema DayNight e recursos da Sprint 8.5 foram reutilizados. Falhas de mutação sem confirmação impedem repetição automática.

## Testes e validação

| Verificação | Resultado |
| --- | --- |
| Novo runner HTTP/API/SQL `Tests/Sprint9/Run.ps1` | 48 verificações aprovadas; fixtures exclusivas removidas |
| Regressão backend Sprint 5 `Tests/Sprint1/Run.ps1 -Sprint5` | 288 verificações aprovadas |
| CI backend `Scripts/Test-CI.ps1` | Restore/build, testes puros, publish Release e 15 verificações Sprint 8 aprovados |
| Integração Kotlin–API–SQL Sprint 6 | 6 verificações e LiveApiTest aprovados; fixtures removidas |
| Android `testDebugUnitTest lintDebug assembleDebug` | BUILD SUCCESSFUL; 30 testes descobertos, 29 aprovados, 1 ignorado; zero falhas/erros |
| Android lint | Zero erros; 11 avisos: 10 sugestões de versões e 1 TextFields preexistente para Número alfanumérico |
| Diff backend | `git diff --check` sem problemas |

Runner Sprint 9 testa cadastro público/validação/duplicidade/proibição de papel, perfil e senha com autenticação e ownership, notificações e leitura própria/alheia, suporte, busca paginada/dados públicos, elegibilidade e avaliação única. `Tests/Sprint9/Run.ps1` usa o SQL Express Development configurado: executar somente contra banco local de testes. Os resultados estão em `artifacts/sprint9-*.log` (ignorados pelo Git).

Smoke no Pixel_6/API 37 com backend HTTP local e SQL Express, APK Debug instalado. Um profissional, um serviço Online e uma conta cliente temporários foram usados, sem alterar registros preexistentes. Observado via UI Android e hierarquia acessível: Cadastro → Home automático → Meu perfil; edição refletida na saudação e no SQL; senha alterada e novo login; catálogo buscou a fixture, exibiu detalhe e horários reais; reserva criada e exibida como Pendente; estado `AguardandoCliente` da fixture permitiu confirmar conclusão; avaliação foi gravada uma vez e a ação ficou desabilitada; notificação passou de Não lida para Lida; suporte gravou chamado para a conta. O estado de conclusão profissional e uma notificação foram preparados diretamente **apenas na reserva/conta temporárias**, porque painel profissional não faz parte desta sprint. Um primeiro preparo da fixture usou `Status='Pendente'` com flags de conclusão e gerou conflito ao confirmar; corrigido para o estado persistido real `AguardandoCliente` antes da validação bem-sucedida.

Capturas reais em `artifacts/sprint9-visual/` incluem Login, Cadastro, Home, Perfil, Catálogo, Detalhe, Agenda, reserva, conclusão, avaliação, notificação e suporte. `cadastro-escuro.png` foi capturada com tema escuro; o modo claro foi restaurado. A hierarquia UI foi inspecionada, mas as PNGs não puderam passar por inspeção visual direta no ambiente de ferramentas desta sessão. A lista de notificações foi revalidada após corrigir largura que ocultava os cards. Conta, serviço, reserva, avaliação, notificação e suporte temporários foram apagados em transação; consulta final encontrou zero usuários da fixture. Sessão invalidada, API e emulador encerrados. O APK final foi regenerado após os últimos ajustes de texto/aviso; esses ajustes finais passaram novamente em testes/lint/build, mas não foram reinstalados para um novo smoke completo.

## Matriz Web × API × Android depois

| Funcionalidade cliente | Web | API | Android |
| --- | --- | --- | --- |
| Cadastro/login/logout | Sim | Sim | Sim |
| Perfil nome/e-mail/telefone | Sim | Sim | Sim |
| Editar nome/telefone | Sim | Sim | Sim |
| Alterar senha | Sim | Sim | Sim |
| Catálogo paginado, busca e modalidade | Sim (filtros próprios) | Sim | Sim |
| Nomes de categoria/subcategoria/profissional | Sim | Sim | Sim no detalhe |
| Agenda/agendar/cancelar/concluir | Sim | Sim | Sim |
| Avaliar atendimento elegível | Sim | Sim | Sim |
| Notificações internas/leitura | Sim | Sim | Sim |
| Enviar suporte | Sim | Sim | Sim |

## Adiados e limites

Filtros específicos por categoria/subcategoria e faixa de preço, avaliação agregada no catálogo e perfil profissional expandido permanecem para etapa futura. Descrição profissional não está persistida; não foi criada informação fictícia. Push, mapas/GPS, pagamentos, chat, upload, refresh token e áreas Profissional/Admin permanecem fora do escopo. A troca de senha não revoga JWT já emitido antes da expiração, limite preexistente. Não houve release assinada nem publicação cloud. Avaliações TalkBack, orientação horizontal, fontes grandes em todas as novas telas e versões Android anteriores à API 37 não foram executadas. O projeto Android ainda não possui repositório Git.

## Inventário

Backend criado: `Services/ContaService.cs`, `Services/ClienteAtendimentoService.cs`, `Api/Contracts/ClienteContracts.cs`, `Controllers/Api/PerfilApiController.cs`, `Controllers/Api/ClienteAtendimentoApiController.cs`, `Tests/Sprint9/Sprint9.csproj`, `Tests/Sprint9/Program.cs`, `Tests/Sprint9/Run.ps1`, este relatório. Backend alterado: `Api/ConfiguracaoApi.cs`, `Api/Contracts/HttpContracts.cs`, `Controllers/Api/AuthController.cs`, `Controllers/Api/AvaliacoesApiController.cs`, `Controllers/Api/ServicosApiController.cs`, `Controllers/SuporteController.cs`, `Controllers/UsuarioController.cs`, `DAO/NotificacaoDAO.cs`, `DAO/ServicoDAO.cs`, `DAO/UsuarioDAO.cs`, `Program.cs`, `Services/AgendamentoService.cs`, `Services/ServicoService.cs`.

Android, caminhos relativos a `C:\Users\Vinicius\AndroidStudioProjects\TRAMPO\app\src`:


### Criados (12)

- `main/java/com/example/trampo/ui/conta/AvaliacaoActivity.kt`
- `main/java/com/example/trampo/ui/conta/ContaActivities.kt`
- `main/java/com/example/trampo/ui/conta/ContaViewModels.kt`
- `main/java/com/example/trampo/ui/conta/NotificacoesActivity.kt`
- `main/res/layout/activity_avaliacao.xml`
- `main/res/layout/activity_cadastro.xml`
- `main/res/layout/activity_notificacoes.xml`
- `main/res/layout/activity_perfil.xml`
- `main/res/layout/activity_senha.xml`
- `main/res/layout/activity_suporte.xml`
- `main/res/layout/item_notificacao.xml`
- `main/res/values/strings_sprint9.xml`

### Alterados (19)

- `main/AndroidManifest.xml`
- `main/java/com/example/trampo/TrampoApplication.kt`
- `main/java/com/example/trampo/data/api/AuthInterceptor.kt`
- `main/java/com/example/trampo/data/api/TrampoApi.kt`
- `main/java/com/example/trampo/data/model/Models.kt`
- `main/java/com/example/trampo/data/repository/Repositories.kt`
- `main/java/com/example/trampo/ui/agendamentos/AgendamentosActivity.kt`
- `main/java/com/example/trampo/ui/agendamentos/AgendamentosViewModel.kt`
- `main/java/com/example/trampo/ui/home/HomeActivity.kt`
- `main/java/com/example/trampo/ui/login/LoginActivity.kt`
- `main/java/com/example/trampo/ui/servicos/ServicosActivity.kt`
- `main/java/com/example/trampo/ui/servicos/ServicosViewModel.kt`
- `main/res/layout/activity_detalhe_agendamento.xml`
- `main/res/layout/activity_detalhe_servico.xml`
- `main/res/layout/activity_home.xml`
- `main/res/layout/activity_login.xml`
- `main/res/layout/activity_servicos.xml`
- `test/java/com/example/trampo/AndroidViewsTest.kt`
- `test/java/com/example/trampo/ApiContractTest.kt`

### Removidos (0)


