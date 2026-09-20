# Arquitetura — Sprint 3

## Registro antes da implementação

View → Controller → DAO → SQL Server.

Controllers acumulam sessão, autorização, validação, regras de negócio, coordenação de DAOs, mensagens e redirecionamentos. Agendamento e Serviço são os primeiros módulos a migrar; os demais continuam funcionando.

## Arquitetura proposta

Presentation (MVC e, futuramente, API) → Application Services → DAOs → SQL Server.

- MVC: binding, sessão, antiforgery, ViewBag/TempData, códigos HTTP e redirecionamentos.
- Services: recebem contexto explícito do usuário e contratos de entrada; validam, autorizam e coordenam casos de uso. Não usam HttpContext, Razor ou IActionResult.
- DAOs: consultas e gravações; as verificações dependentes de concorrência permanecem dentro das transações SQL e bloqueios existentes. Services não substituem a revalidação transacional por checagens fora do bloqueio.
- SQL Server: persistência, constraints e isolamento.

Services concretos serão registrados por DI: sem interfaces espelhadas sem necessidade. Resultados/erros tipados permitirão mapear validação, autenticação, permissão, inexistência e conflito sem interpretar texto.
Contratos mínimos, apenas para operações utilizadas. A migração preserva URLs, formulários, sessões e respostas MVC.
MVC não chamará HTTP para a própria API. A API futura chamará os mesmos Services diretamente.

## Transações

Reservas e serviço/disponibilidade já são atômicos. Transições de agendamento serão coordenadas com notificações em uma única conexão/transação no DAO, evitando estado atualizado sem notificação. Testes provocarão falha real de persistência antes do commit, sem modificar o esquema.
## Estado implementado em 20/09/2026

AgendamentoController e ServicoController utilizam os Services acima por DI. AvaliacaoController também utiliza AgendamentoService para compartilhar autorização e regras de conclusão/avaliação. Os demais Controllers e a infraestrutura de BaseController permanecem legados; a migração é parcial por módulo, conforme planejado.

UsuarioContexto carrega ID e tipo obtidos pelo Controller; os Services conferem esses dados no cadastro persistido. FalhaOperacao possui TipoFalha para decisões HTTP sem interpretação de mensagens. Exceções técnicas SQL não são convertidas genericamente em erro de validação.

As cinco transições de agendamento gravam seus avisos dentro da transação do DAO. O cadastro idempotente de cliente é uma unidade independente da reserva. A decisão de snapshot está em HISTORICO.md e ainda não foi aplicada ao esquema. Evidências e limites de validação constam no RELATORIO.md.
