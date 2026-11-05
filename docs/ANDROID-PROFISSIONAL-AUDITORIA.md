# Auditoria Android Cliente + Profissional

Data: 04/11/2026. Projetos inspecionados: `C:\Users\Vinicius\Desktop\Trampo` e `C:\Users\Vinicius\AndroidStudioProjects\TRAMPO`.

## Matriz de funcionalidades

| Funcionalidade | Web Profissional | API existente | Android atual | Alteração necessária |
|---|---|---|---|---|
| Autenticação e perfil | `UsuarioController` e `ProfissionalController` usam usuário e perfil persistidos | `POST /api/v1/auth/login`, `GET /api/v1/auth/me`; `UsuarioResponse.Tipo` | `AuthRepository.me()` aceita apenas `cliente` e encerra sessão profissional; `MainActivity`/`LoginActivity` sempre enviam a `HomeActivity` | Aceitar `profissional` e rotear pelo tipo retornado por `/me`; manter autorização no servidor |
| Serviços públicos | `ProfissionalController.Lista` e `PerfilPublico` exibem serviços | `GET /api/v1/servicos` e `/{id}` | Catálogo, detalhe e agendamento de Cliente implementados | Preservar fluxo; acrescentar avaliações reais ao detalhe |
| Serviços próprios | `ProfissionalController.MeusServicos`, `ServicoController` criam, editam e removem via `ServicoService` | `GET /api/v1/servicos/me`, `POST/PUT/DELETE /api/v1/servicos` | Sem tela profissional | Consumir API existente; respeitar validações do serviço (modalidade, preço, local, disponibilidade) |
| Agendamentos do Cliente | `AgendamentoController.Meus` usa `AgendamentoService.Meus` | `GET /api/v1/agendamentos?visao=meus` e `/{id}` | Lista e detalhe exibem `Serviço #ID` | Incluir nomes na projeção REST e apresentar detalhe estruturado |
| Agendamentos recebidos | `AgendamentoController.Recebidos`, confirmar, recusar, finalizar | `GET /api/v1/agendamentos?visao=recebidos`; ações `confirmar`, `recusar`, `finalizar`, `cancelar` | Sem tela profissional | Criar fluxo profissional sobre esses endpoints, sem replicar regras no Android |
| Transições de status | `AgendamentoService` delega atualização condicional ao `AgendamentoDAO`; `Agendamento.StatusAtual()` retorna Pendente, Confirmado, AguardandoCliente, Finalizado e cancelamentos | Mesmas ações via REST, com conflito quando a operação não é mais permitida | Cliente oferece concluir, avaliar e cancelar por status local | Atualizar após mutações e usar estado de avaliação da API; ações são sugestões visuais, servidor valida |
| Disponibilidade e bloqueios | `DisponibilidadeController`, `RegrasAgenda` e `AgendamentoDAO` verificam regras e `BloqueiosAgenda` | `GET /api/v1/servicos/{id}/agenda`; disponibilidade acompanha cadastro de serviço | Cliente consulta agenda ao marcar | Manter cálculo e validações no backend; confirmar se há API de gestão de bloqueios antes de expor edição |
| Locais | `LocalController` e `LocalService` | `GET/POST/PUT/DELETE /api/v1/locais` para profissional | Sem tela profissional | Consumir API existente e seus DTOs |
| Avaliação pelo Cliente | `AvaliacaoController` usa `AgendamentoService`; só permite agendamento concluído, nota 1–5, comentário até 500 e impede duplicata | `GET/POST /api/v1/agendamentos/{id}/avaliacao` | `AvaliacaoActivity` e `AvaliacaoViewModel` já enviam; detalhe mostra ação em Finalizado | Melhorar contexto e estado da tela, preservar a regra existente |
| Avaliações públicas | `ProfissionalController.PerfilPublico` usa `AvaliacaoDAO.ListarPorProfissional` | Não há rota pública de listagem de avaliações | Detalhe de serviço não mostra avaliações | Expor projeção pública mínima do mesmo DAO e renderizar dados reais |
| Perfil profissional | `ProfissionalController.MeuPerfil` e `SalvarContato` | `/api/v1/perfil` é exclusivo de Cliente; `/auth/me` é comum | Perfil Android é de Cliente | Criar API específica apenas se necessária para fluxo profissional, reutilizando serviço/regras reais |
| Notificações e suporte | Controllers MVC próprios | `/api/v1/notificacoes` e `/api/v1/suporte` são exclusivos de Cliente | Telas de Cliente existentes | Preservar Cliente; auditar endpoints profissionais antes de expor esses itens |

## Arquitetura e lacunas verificadas

- Android usa Activities XML, ViewModels com `UiState`, repositórios Retrofit e `SessionManager` com token; não há Fragments. `AuthInterceptor` injeta o token. `BaseActivity` direciona ao login em sessão ausente/401. `UiState.failureOf` mapeia erros HTTP e rede.
- `AgendamentoResponse` REST continha IDs, data, hora, status, descrição, endereço e valor final. O modelo `Agendamento` já possuía `Servico`, `NomeProfissional`, `NomeCliente`, `Atendimento`, `EnderecoLocal`, `TipoPreco` e `PrecoBase`; a consulta de listagem preenchia parte deles, mas `BuscarPorId` ainda não selecionava nem mapeava os nomes. A correção é aditiva no DTO, sem consulta extra no Android.
- `ValorFinal` é o valor persistido na finalização. O contrato atual não expõe um valor estimado contratado; o preço corrente do serviço não deve ser apresentado como valor histórico.
- A API de avaliação já consulta permissão e duplicidade. O Android precisa usar esse estado para ocultar a ação após avaliação e atualizar a tela anterior após envio.
- O perfil Web tem dashboard, serviços, agendamentos, disponibilidade, locais, avaliações públicas e contato. A API já cobre serviços, agendamentos e locais, mas não contém equivalentes completos para dashboard, bloqueios e perfil profissional. Não há base para apresentar controles fictícios desses recursos.
- O texto anexado termina durante a Fase 5. O trabalho deve permanecer restrito aos requisitos efetivamente recebidos.
