# Sprint 13 — auditoria Desktop Cliente

Auditoria anterior à implementação. Repositórios examinados: Web/API, Android (contrato compartilhado) e Desktop WinForms. O Desktop atual usa `HttpClient` assíncrono e JWT apenas em memória; o login valida `/auth/me`, mas aceita somente `profissional`. Os Forms profissionais são modais e fecham após 401; 403 mostra mensagem sem limpar sessão. O runner existente usa respostas HTTP simuladas.

| Funcionalidade | Web Cliente | API existente | Desktop atual | Alteração necessária |
| --- | --- | --- | --- | --- |
| Login/perfil | Login e área Cliente | `POST /auth/login`, `GET /auth/me` | Rejeita Cliente | Encaminhar por perfil; Admin só Web |
| Busca e filtros | Catálogo público por nome, localização e categoria | No início da auditoria: `GET /servicos` com `busca`, `atendimento` e paginação; `GET /categorias` | Só serviços próprios do Profissional | Ampliar filtros opcionais no servidor antes da paginação, depois ligar a busca Desktop |
| Detalhe e profissional | Perfil público, serviço, avaliações Web | `GET /servicos/{id}` inclui nome profissional e categorias; sem perfil público REST nem avaliações públicas | Sem tela Cliente | Detalhe com dados reais; registrar avaliação/perfil público como lacuna da API |
| Disponibilidade | Tela Novo usa `AgendamentoService.ConsultarAgenda` | `GET /servicos/{id}/agenda?data=` | Não consulta agenda Cliente | Consultar horários da API, sem cálculo local |
| Criar agendamento | `AgendamentoService.Criar` | `POST /agendamentos` deriva cliente do JWT | Ausente | Resumo, endereço domiciliar e criação; nenhum `clienteId` enviado |
| Meus agendamentos | `service.Meus`, detalhes e ações | `GET /agendamentos?visao=meus`, `GET /agendamentos/{id}` | Só recebidos | Lista e detalhe Cliente; DTO já ampliado nesta árvore com `servicoNome`, pessoas e modalidade |
| Cancelar/concluir | `Cancelar`, `ConfirmarConclusao` | `POST /agendamentos/{id}/cancelar`, `/confirmar-conclusao` | Ações do Profissional | Habilitar conforme estado e atualizar após resposta |
| Avaliar | Web após finalização | `GET/POST /agendamentos/{id}/avaliacao`; sem listagem de avaliações próprias | Ausente | Avaliar quando API permitir; listagem própria exige endpoint mínimo se incluída |
| Perfil/senha | Minha Conta | `GET/PUT /perfil`, `PUT /perfil/senha` | Ausente | Form Cliente, sem editar campos protegidos |
| Notificações | Lista e marcar lida | `GET /notificacoes`, `POST /notificacoes/{id}/ler` | Ausente | Lista e leitura do usuário autenticado |
| Suporte | Envio Web; DAO possui lista por usuário | No início da auditoria: `POST /suporte` apenas | Ausente | GET mínimo com ownership do JWT; consertar leitura do DAO que abre conexão já aberta |
| Locais/endereço | Locais do Profissional; endereço do Cliente no pedido | `GET /enderecos/cep/{cep}` para usuário autenticado; `/locais` só Profissional | Profissional gerencia locais | Cliente consulta CEP para domicílio; não acessar `/locais` |
| Profissional | Painel, agendamentos, serviços, locais | Rotas de propriedade e papel existentes | Implementado | Preservar Forms, rotas, testes e 401/403 |

Limites verificados antes da implementação: o catálogo REST filtrava no servidor somente texto e modalidade; não havia rota REST de perfil público, avaliações públicas ou resposta de suporte. Não inventar dados, médias ou respostas. A API não deve confiar em IDs de proprietário do Desktop. O histórico da Sprint 12 permanece em seu documento. O código Android atual contém área Profissional e chamadas para avaliações públicas/recebidas ainda sem rotas correspondentes neste backend.
