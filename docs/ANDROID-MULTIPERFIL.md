# Android TRAMPO multiperfil

Data: 05/11/2026. Auditoria inicial: [ANDROID-PROFISSIONAL-AUDITORIA.md](ANDROID-PROFISSIONAL-AUDITORIA.md).

## Navegação e arquitetura

`POST /api/v1/auth/login` emite o JWT. Em seguida, `GET /api/v1/auth/me` identifica `cliente` ou `profissional`. O cliente entra na `HomeActivity` existente; o profissional entra em `ProfissionalHomeActivity`. A rota não concede permissão: a API valida perfil e propriedade em cada operação. Perfis diferentes são rejeitados.

As telas seguem `Activity/XML → ViewModel → Repository → Retrofit → API`. O Android não acessa SQL Server nem provedores de CEP diretamente. `SessionManager` mantém JWT e perfil mínimo (`id`, nome e tipo) em preferências privadas, excluídas de backup; não armazena senha. Resposta 401 e logout limpam o armazenamento e levam ao login. A ação Sair está disponível na Home e nas telas de perfil, senha, notificações, suporte e gestão profissional.

## Cliente

Cadastro, login, catálogo, busca, filtros, agenda, solicitação, lista e detalhe de agendamentos, cancelamento, confirmação de conclusão, perfil, senha, notificações e suporte permanecem. O detalhe agora usa `servicoNome`, mostra profissional, data, modalidade, local, observações e valores. O preço anterior à finalização é rotulado como oferta anunciada atualmente, pois o backend não persiste um snapshot do preço contratado. `valorFinal` é exibido quando existe.

A avaliação usa `GET/POST /api/v1/agendamentos/{id}/avaliacao`; o servidor valida participante, status, nota, tamanho do comentário e duplicidade. A tela mostra serviço e profissional e usa nota de 1 a 5 estrelas. O detalhe do serviço consulta avaliações reais via `GET /api/v1/servicos/{servicoId}/avaliacoes`.

## Profissional

| Tela | Operações e API |
| --- | --- |
| Home | Acesso às áreas profissionais e conta; `/auth/me` em cada entrada |
| Agendamentos recebidos e detalhe | `GET /agendamentos?visao=recebidos`, `GET /agendamentos/{id}`, `POST /confirmar`, `/recusar`, `/finalizar`, `/cancelar` |
| Meus serviços e editor | `GET /servicos/me`, `GET /categorias`, `GET /locais`, `POST/PUT/DELETE /servicos` |
| Disponibilidade | Dias e intervalo de cada serviço em `GET /servicos/me` e `POST/PUT /servicos`; edição junto com o serviço |
| Meus locais e editor | `GET/POST/PUT/DELETE /locais`, `GET /locais/{id}`; CEP via `GET /enderecos/cep/{cep}` |
| Minhas avaliações | `GET /profissionais/me/avaliacoes` |
| Meu perfil e senha | `GET/PUT /perfil`, `PUT /perfil/senha` |
| Notificações e suporte | `GET /notificacoes`, `POST /notificacoes/{id}/ler`, `POST /suporte` |

O editor de serviço mantém categoria e subcategoria fixas na edição, seguindo `ServicoService`, que preserva a subcategoria original. Permite editar nome, descrição, modalidade, preço, local/link e horários. O backend valida disponibilidade, preço, vínculo do local e propriedade do serviço. O editor de locais permite preenchimento manual quando a consulta de CEP falha; HTTP 409 por local vinculado recebe mensagem específica.

## Backend e segurança

`AgendamentoResponse` ganhou nomes, modalidade, local e preço corrente da oferta de modo aditivo. `AgendamentoDAO.BuscarPorId` passou a projetar os nomes. A listagem pública de avaliações e a listagem profissional usam `AvaliacaoService` e o `AvaliacaoDAO` existente. A rota profissional exige papel `profissional` e obtém o profissional do JWT; a rota pública só consulta um serviço público ativo. `PerfilApiController`, notificações e suporte agora aceitam ambos os perfis, usando os mesmos serviços de aplicação e o usuário autenticado. Serviços e locais continuam protegidos por papel e verificações de propriedade no backend. IDs de proprietário não são enviados nos corpos Android.

## Experiência e acessibilidade

As novas telas têm rótulos de campo, áreas de toque de pelo menos 48 dp, estados de carregamento, vazios e erro, confirmação antes de excluir, recusar ou cancelar, e descrição falada da nota das avaliações. O detalhe do agendamento usa blocos e badges existentes. A fonte usa `sp` e os layouts permitem rolagem.

## Validação e limites

Foram executados build da API, build Debug, 35 testes unitários Android (34 passaram, 1 integração SQL foi ignorada) e lint do Android, sem erros. O lint ainda aponta um aviso sobre o campo de número do endereço (que aceita letras) e nove recursos não usados. Os testes Android cobrem contratos REST, verbos e corpos de mutação sem ID de proprietário, perfis aceitos, respostas de agendamento e inflação das telas. `Tests/auth-check` executa HTTP contra os controllers reais e verificou 401 sem JWT e 403 com papel Cliente para rotas profissionais; usa autenticação de teste sem conexão ao banco. Testes de propriedade com SQL Server real, testes instrumentados em emulador e revisão manual com TalkBack exigem ambiente de integração e execução visual; não foram afirmados como concluídos.

| Etapa | Entrega | Verificação |
| --- | --- | --- |
| A | Auditoria e nome do serviço | API e Android compilados |
| B | Avaliação do Cliente e avaliações públicas | API e Android compilados; testes de contrato |
| C | Login multiperfil e Home Profissional | Android compilado; teste do perfil profissional |
| D | Agendamentos recebidos e ações | Android compilado; testes de rotas e corpo de finalização |
| E | Serviços e horários semanais | API e Android compilados; testes de mutações |
| F | Locais e CEP pelo backend | Android compilado; testes de rotas e conflito |
| G | Disponibilidade por serviço e perfil | Android compilado; `ServicoService` e `ContaService` reutilizados |
| H | Avaliações, notificações e suporte | API e Android compilados; testes de contrato e 401/403 |
| I | UX, acessibilidade e documentação | Lint, testes unitários e relatórios revisados |

Não há API nem fluxo Web consolidado para gerir bloqueios de agenda no Android. A disponibilidade oferecida aqui é a programação semanal por serviço. A edição de contato público profissional separada do perfil não foi exposta; nome e telefone usam `/perfil`. Não há alteração de plano, dashboard financeiro ou administração no Android.
