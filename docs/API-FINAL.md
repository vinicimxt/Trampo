# API final — TRAMPO

Inventário atual: **37 operações REST**, prefixo `/api/v1`. O inventário de 26/09/2026 registrava 32; versões posteriores acrescentaram a consulta de chamados, a lista de serviços próprios, categorias e as duas consultas de avaliações desta estabilização. Fonte: [controllers](../Controllers/Api) e [Services](../Services). Swagger `/swagger/index.html` e OpenAPI `/openapi/v1.json` somente em Development.

## Convenções

`Público` dispensa token; demais operações exigem `Authorization: Bearer` e usuário persistido válido. `C/P contratante` inclui profissional que contrata serviço de outro profissional: o perfil permitido no controller não substitui a autorização de participante no Service. Admin não recebe acesso implícito aos módulos reservados a cliente/profissional.

Erros comuns às rotas protegidas: 401 (token ausente/inválido), 403 (perfil/ownership). Entradas inválidas: 400; recurso inexistente: 404; conflito de estado/duplicidade: 409; JSON incompatível: 415; falha técnica: 500. Tabela mostra os principais status específicos além desses comuns, não uma lista exaustiva. Corpo de erro: `{"erro":{"codigo":"...","mensagem":"..."}}`. API responde com `Cache-Control: no-store`.

| Domínio | Método | Rota após /api/v1 | Autenticação/perfil | Finalidade | Status principais |
| --- | --- | --- | --- | --- | --- |
| Auth | POST | `/auth/login` | Público | Entrar e emitir JWT | 200, 400, 401, 429, 503 |
| Auth | POST | `/auth/register` | Público | Criar cliente e autenticar | 201, 400, 409, 429, 503 |
| Auth | GET | `/auth/me` | JWT, usuário válido | Identidade atual | 200, 401 |
| Perfil | GET | `/perfil` | JWT cliente/profissional | Nome/email/telefone próprios | 200 |
| Perfil | PUT | `/perfil` | JWT cliente/profissional | Editar nome/telefone; email não editável | 204, 400 |
| Perfil | PUT | `/perfil/senha` | JWT cliente/profissional | Senha atual/nova/confirmação | 204, 400, 429 |
| Serviços | GET | `/servicos` | Público | Catálogo ativo paginado, com filtros opcionais | 200, 400 |
| Serviços | GET | `/servicos/me` | JWT profissional | Serviços próprios, inclusive inativos | 200 |
| Serviços | GET | `/servicos/{id}` | Público | Detalhe público | 200, 404 |
| Avaliações | GET | `/servicos/{id}/avaliacoes` | Público | Avaliações do serviço ativo, média e total | 200, 404 |
| Avaliações | GET | `/profissionais/me/avaliacoes` | JWT profissional | Avaliações recebidas pelo profissional do token | 200, 401, 403 |
| Serviços | GET | `/servicos/{id}/agenda?data=AAAA-MM-DD` | JWT C/P | Horários disponíveis | 200, 400, 404 |
| Serviços | POST | `/servicos` | JWT profissional | Criar serviço próprio | 201, 400, 409 |
| Serviços | PUT | `/servicos/{id}` | JWT profissional proprietário | Editar serviço | 204, 400, 404, 409 |
| Serviços | DELETE | `/servicos/{id}` | JWT profissional proprietário | Excluir ou desativar preservando histórico | 200, 404, 409 |
| Categorias | GET | `/categorias` | Público | Categorias e subcategorias disponíveis | 200 |
| Endereços | GET | `/enderecos/cep/{cep}` | JWT, sem restrição adicional de perfil | Consultar CEP | 200, 400, 404, 429, 502, 503, 504 |
| Endereços | GET | `/enderecos/pesquisar?uf=&cidade=&logradouro=` | JWT, sem restrição adicional de perfil | Pesquisar CEP | 200, 400, 429, 502, 503, 504 |
| Locais | GET | `/locais` | JWT profissional | Listar locais próprios | 200 |
| Locais | GET | `/locais/{id}` | JWT profissional proprietário | Consultar local | 200, 404 |
| Locais | POST | `/locais` | JWT profissional | Criar endereço estruturado | 201, 400 |
| Locais | PUT | `/locais/{id}` | JWT profissional proprietário | Editar local livre de vínculo histórico | 204, 400, 404, 409 |
| Locais | DELETE | `/locais/{id}` | JWT profissional proprietário | Remover local sem vínculos | 204, 404, 409 |
| Agendamentos | GET | `/agendamentos?visao=meus` | JWT C/P; recebidos exige profissional | Listar próprios/recebidos | 200, 400 |
| Agendamentos | GET | `/agendamentos/{id}` | JWT C/P participante | Detalhe autorizado | 200, 404 |
| Agendamentos | POST | `/agendamentos` | JWT C/P contratante | Reservar horário de outro profissional | 201, 400, 404, 409 |
| Agendamentos | POST | `/agendamentos/{id}/cancelar` | JWT C/P participante | Cancelar conforme regras | 204, 404, 409 |
| Agendamentos | POST | `/agendamentos/{id}/confirmar` | JWT profissional responsável | Confirmar solicitação | 204, 404, 409 |
| Agendamentos | POST | `/agendamentos/{id}/recusar` | JWT profissional responsável | Recusar solicitação | 204, 404, 409 |
| Agendamentos | POST | `/agendamentos/{id}/finalizar` | JWT profissional responsável | Informar valor final e aguardar cliente | 204, 400, 404, 409 |
| Agendamentos | POST | `/agendamentos/{id}/confirmar-conclusao` | JWT C/P contratante | Confirmar conclusão | 204, 404, 409 |
| Avaliações | GET | `/agendamentos/{id}/avaliacao` | JWT C/P contratante | Elegibilidade/já avaliado | 200, 404 |
| Avaliações | POST | `/agendamentos/{id}/avaliacao` | JWT C/P contratante | Avaliar atendimento concluído | 204, 400, 404, 409 |
| Notificações | GET | `/notificacoes?pagina=1&tamanho=20` | JWT cliente/profissional | Listagem própria paginada | 200, 400 |
| Notificações | POST | `/notificacoes/{id}/ler` | JWT cliente/profissional proprietário | Marcar como lida | 204, 404 |
| Suporte | POST | `/suporte` | JWT cliente/profissional | Gravar chamado próprio | 204, 400, 429 |
| Suporte | GET | `/suporte` | JWT cliente/profissional | Listar chamados do usuário autenticado | 200 |

## Contratos e limites

Catálogo: `pagina`, `tamanho`, `busca`, `atendimento` (Online, Local, Domicilio), `categoriaId`, `subcategoriaId` e `profissional`. Pesquisa textual inclui nome do serviço, profissional, categoria e subcategoria; os três filtros novos são aplicados no SQL antes da paginação. Lista e detalhe incluem `mediaAvaliacoes` (nula quando sem avaliação) e `totalAvaliacoes`, agregados do profissional, sem nomes ou comentários individuais. O detalhe público não expõe contato privado, link de reunião ou coordenadas.

Agendamentos de participante incluem `servicoNome`, `profissionalNome`, `clienteNome` e `atendimento`, além dos campos existentes. O nome vem da associação persistida do pedido com o serviço, inclusive quando a oferta foi desativada. O contrato não apresenta preço atual como valor histórico contratado. `GET /suporte` deriva o proprietário do JWT; a tabela atual contém mensagem e status, mas não contém texto de resposta. As duas consultas de avaliações retornam `media` (nula quando vazia), `total` e `avaliacoes` com nota, comentário, data, nome público do cliente e nome do serviço quando disponível. A consulta pública exige serviço ativo e filtra avaliações desse serviço; a consulta profissional deriva o proprietário do JWT e inclui avaliações históricas de serviços inativos. Não há listagem REST de avaliações enviadas pelo cliente.

Cadastro não aceita papel/ID escolhido pelo cliente. Perfil permite nome e telefone. Senha exige senha atual, nova e confirmação. Endereço estruturado recebe CEP, logradouro, número, complemento opcional, bairro, cidade e UF; coordenadas enviadas pelo cliente são recusadas. CEP aceita oito dígitos ou máscara; consulta indisponível permite preenchimento manual. Avaliação exige nota 1–5, comentário até 500 caracteres e apenas uma por agendamento.

`api-login` limita por IP a 10 chamadas/minuto e é compartilhada por login, cadastro, senha e suporte. Endereços usam política própria. Não repetir mutações automaticamente em caso de resposta incerta. Não existe rota de logout/revogação de JWT: logout Mobile é local.

Endpoint operacional fora do inventário REST: `GET /health`, anônimo, 200/Healthy quando o processo responde. Não testa o banco. Consultas externas podem retornar 502/503/504. Não divulgar tokens em screenshots do Swagger.

Histórico e exemplos adicionais: [API Sprint 4](Sprint4/API.md), [Sprint 9](SPRINT-9-MOBILE-FUNCIONALIDADES.md), [testes finais](TESTES-FINAIS.md).
