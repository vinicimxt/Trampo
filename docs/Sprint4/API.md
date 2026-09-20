# REST API V1 — TRAMPO

## Convenções

Base: `/api/v1`. MVC e API chamam os mesmos Services, sem chamadas HTTP internas. JSON usa camelCase, números decimais para valores monetários, strings para status/modalidade, e preserva nulls. Requests desconhecidos em login, serviço, reserva, finalização e avaliação são recusados com 400; o cliente não pode acrescentar IDs de dono, preço ou status à reserva.

Datas de resposta usam `yyyy-MM-dd`; a entrada de reserva reutiliza DateTime do contrato de aplicação (enviar `yyyy-MM-dd`, sem fuso; a regra considera o dia). Hora/horários usam TimeSpan em `HH:mm:ss`. A agenda recebe DateOnly na query. Datas de expiração JWT são UTC em ISO 8601. A agenda de negócio continua usando o relógio local do servidor, conforme MVC: padronizar o fuso será necessário antes de hospedar em outro ambiente.

Autenticação: `Authorization: Bearer <accessToken>`. Apenas catálogo e login são públicos. Cookies e sessão MVC não autenticam a API. Perfis são `cliente`, `profissional`, `admin`. Admin pode consultar sua identidade; não ganha propriedade sobre serviços/agendamentos.

Swagger UI: `/swagger/index.html`; documento: `/openapi/v1.json`. Ambos existem somente em Development. O botão Authorize aceita o accessToken; a interface não contorna a autorização. O documento gerado contém apenas as operações da API V1, schemas, parâmetros, respostas e esquema Bearer.

## Contratos

- **LoginRequest**: `{ "email": "usuario@example.invalid", "senha": "senha-do-usuario" }`.
- **LoginResponse**: `accessToken`, `tokenType` (`Bearer`), `expiresAt` (UTC). Token não é armazenado pelo servidor.
- **UsuarioResponse**: `id`, `nome`, `email`, `tipo`; sem senha/hash/telefone/documento.
- **ServicoRequest**: `subcategoriaId`, `nome`, `descricao?`, `atendimento` (`Local`, `Domicilio`, `Online`), `localId?`, `linkOnline?`, `tipoPreco` (`Fixo`, `Combinar`), `precoBase?`, `diasSemana` (por exemplo `1,2,3`), `horaInicio`, `horaFim`. Os horários usam `08:00:00`. A edição mantém a subcategoria original conforme regra existente. O ID da rota é a única fonte de identidade do recurso.
- **ServicoResponse**: `id`, `profissionalId`, `subcategoriaId`, `nome`, `descricao`, `atendimento`, `tipoPreco`, `precoBase`, `ativo`. Catálogo público não expõe link de reunião ou endereço. Lista apenas ofertas ativas.
- **CriarAgendamentoRequest**: `servicoId`, `data`, `hora`, `descricao?`, `rua?`, `numero?`, `bairro?`, `cidade?`. Endereço é necessário para Domicilio. Não aceita `clienteId`, `usuarioId`, `profissionalId`, `status`, `preco` ou `taxa`.
- **AgendamentoResponse**: `id`, `servicoId`, `profissionalId`, `data`, `hora`, `status`, `descricao`, `enderecoCliente`, `valorFinal`, `origemValorFinal`. Somente participantes têm acesso. Sem preço atual apresentado como preço contratado, taxa ou dados internos do cliente.
- **AgendaResponse**: `servicoId`, `data`, `horarios` (array de horários disponíveis). Consultar não reserva o horário; POST revalida tudo sob bloqueios SQL.
- **FinalizarRequest**: `{ "valorFinal": 100.00 }`. Para preço fixo, prevalece o cadastro do servidor; a limitação histórica abaixo continua aplicável.
- **AvaliacaoRequest**: `{ "nota": 5, "comentario": "Atendimento realizado" }`. Profissional e autor são derivados no Service.
- **RecursoCriadoResponse**: `{ "id": 123 }`, com HTTP 201 e cabeçalho Location.
- **RemocaoResponse**: `{ "id": 123, "resultado": "desativado" }` ou `excluido`. O resultado vem da mesma transação que executa a remoção.

## Endpoints

Nos endpoints protegidos, 401 significa Bearer ausente/inválido; 403 significa perfil ou propriedade inadequados. Todas as rotas podem responder 500 genérico em falha técnica. Bodies de criação/edição requerem application/json (415 caso contrário).

| Método | Rota | Auth | Perfil | Request / query | Response de sucesso | Outros status previstos |
|---|---|---|---|---|---|---|
| POST | /api/v1/auth/login | Pública | Qualquer conta válida | LoginRequest | 200 LoginResponse | 400, 401, 429, 503 |
| GET | /api/v1/auth/me | Bearer | cliente, profissional, admin | Sem body | 200 UsuarioResponse | 401 |
| GET | /api/v1/servicos | Pública | Todos | pagina=1, tamanho=20 (máximo 100) | 200 ServicoResponse[] | 400 |
| GET | /api/v1/servicos/{id} | Pública | Todos | ID na rota | 200 ServicoResponse | 404 |
| POST | /api/v1/servicos | Bearer | profissional | ServicoRequest | 201 RecursoCriadoResponse + Location | 400, 401, 403, 404, 409 |
| PUT | /api/v1/servicos/{id} | Bearer | profissional proprietário | ServicoRequest | 204 sem body | 400, 401, 403, 404, 409 |
| DELETE | /api/v1/servicos/{id} | Bearer | profissional proprietário | Sem body | 200 RemocaoResponse | 401, 403, 404, 409 |
| GET | /api/v1/servicos/{id}/agenda | Bearer | cliente, profissional | data=2026-09-25 | 200 AgendaResponse | 400, 401, 403, 404 |
| GET | /api/v1/agendamentos | Bearer | cliente, profissional | visao=meus; recebidos exige profissional | 200 AgendamentoResponse[] | 400, 401, 403 |
| GET | /api/v1/agendamentos/{id} | Bearer | Participante | ID na rota | 200 AgendamentoResponse | 401, 403, 404 |
| POST | /api/v1/agendamentos | Bearer | cliente, profissional contratante | CriarAgendamentoRequest | 201 RecursoCriadoResponse + Location | 400, 401, 403, 404, 409 |
| POST | /api/v1/agendamentos/{id}/cancelar | Bearer | Participante | Sem body | 204 | 401, 403, 404, 409 |
| POST | /api/v1/agendamentos/{id}/confirmar | Bearer | profissional responsável | Sem body | 204 | 401, 403, 404, 409 |
| POST | /api/v1/agendamentos/{id}/recusar | Bearer | profissional responsável | Sem body | 204 | 401, 403, 404, 409 |
| POST | /api/v1/agendamentos/{id}/finalizar | Bearer | profissional responsável | FinalizarRequest | 204 | 400, 401, 403, 404, 409 |
| POST | /api/v1/agendamentos/{id}/confirmar-conclusao | Bearer | Contratante | Sem body | 204 | 401, 403, 404, 409 |
| POST | /api/v1/agendamentos/{id}/avaliacao | Bearer | Contratante | AvaliacaoRequest | 204 | 400, 401, 403, 404, 409 |

`visao=meus` inclui os pedidos feitos pelo usuário, inclusive quando ele tem perfil profissional. `visao=recebidos` inclui os pedidos do profissional autenticado. Não há edição genérica de status nem consulta arbitrária por usuário. A listagem de agendamentos ainda não tem paginação; a de serviços permite páginas de 1 a 1.000.000, com até 100 itens, ordenados por ID.

## Erros

Formato estável: `{ "erro": { "codigo": "CONFLITO", "mensagem": "..." } }`.

| HTTP | Código |
|---|---|
| 400 | VALIDACAO |
| 401 | NAO_AUTENTICADO |
| 403 | SEM_PERMISSAO |
| 404 | NAO_ENCONTRADO |
| 405 | METODO_NAO_PERMITIDO |
| 409 | CONFLITO |
| 415 | FORMATO_INVALIDO |
| 429 | LIMITE_REQUISICOES |
| 500 | ERRO_INTERNO |
| 503 | API_INDISPONIVEL |

O cliente deve decidir pelo status/código, nunca pelo texto. Erros de binding/JSON têm mensagem genérica, sem ecoar o valor recebido. Exceptions SQL, connection strings e stack traces não são enviados ao cliente. Falhas técnicas são registradas no log do servidor; acesso a esses logs deve ser restrito.

As revalidações SQL usam códigos exclusivos: 51001 = conflito no bloqueio; 51002 = serviço não encontrado; 51003 = local inválido. Nenhuma decisão depende de examinar o texto da exception. Outros erros SQL continuam técnicos e resultam em 500.

## Histórico e preço

Decisão original: ../Sprint3/HISTORICO.md. Não houve migração de snapshot. `valorFinal` é o valor persistido da finalização e fica null antes disso. `origemValorFinal=finalizacao_sem_snapshot_da_oferta` comunica que não há snapshot do contrato original.

A finalização de oferta Fixo continua usando o preço atual do cadastro; uma edição entre reserva e conclusão pode mudar esse valor. A API V1 não apresenta preço atual como preço histórico e não inclui endpoints de relatório financeiro. O snapshot foi adiado para evolução incremental com política explícita para reservas legadas; deve preceder uso comercial que exija preservar o preço contratado. Não existem garantias de cobrança real nesta versão acadêmica.

## Exemplo de reserva

```json
{
  "servicoId": 123,
  "data": "2026-09-25",
  "hora": "10:00:00",
  "descricao": "Solicitação de atendimento"
}
```

A data é ilustrativa: escolher horário futuro disponível. Mesmo após consulta da agenda, tratar 409 ao reservar.
