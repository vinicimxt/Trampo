# Erros e feedback no MVC Web

## Auditoria dos fluxos

| Actions MVC | Service/DAO | Falha esperada | Antes | Agora |
| --- | --- | --- | --- | --- |
| Agendamento/Salvar | AgendamentoService, AgendamentoDAO | validação, agenda ocupada, limite | texto HTTP via BaseController | formulário com ModelState ou redirect para Novo com feedback |
| Agendamento/Confirmar, Recusar, Cancelar, Finalizar, ConfirmarCliente | AgendamentoService | status incompatível, horário ainda não iniciado | texto HTTP via BaseController | redirect para Recebidos/Meus com feedback |
| Avaliacao/Salvar | AgendamentoService, AvaliacaoDAO | ainda não permitida, duplicada | texto HTTP via BaseController | redirect para Meus com feedback |
| Servico/Salvar, Editar, Excluir, Desativar | ServicoService | dados inválidos, recurso indisponível | texto HTTP ou feedback parcial | ModelState ou redirect com feedback |
| Local/Salvar, Excluir | LocalService | local histórico ou vinculado, dados inválidos | alguns casos com BadRequest/texto | ModelState ou redirect com feedback |
| Disponibilidade/Salvar, Desativar | DisponibilidadeDAO | intervalo inválido, horário ausente | BadRequest/404 sem contexto | redirect com feedback |
| Usuario/Cadastrar, Logar, SalvarConta | ContaService e DAO | dados e senha inválidos | alguns BadRequest crus | redirect com feedback |
| Suporte/Enviar | ClienteAtendimentoService | solicitação inválida | BadRequest com mensagem | redirect com feedback |
| Profissional/MeusServicos, PerfilPublico | DAOs | profissional ausente | Content com texto | página com layout e status 403/404 |

As regras continuam nos Services e DAOs. O MVC converte `FalhaOperacao` esperada em mensagem no toast compartilhado do layout, usando `TempData` após POST. Campos inválidos em formulários de agendamento, serviço e local usam `ModelState` e resumo de validação. O middleware da API continua responsável pelos status e pelo JSON dos endpoints `/api`.

Falhas técnicas de POST tratadas por `ExecutarPost` são registradas com `ILogger` e ID da requisição; o usuário recebe texto genérico. As demais falhas técnicas Web usam o handler de produção e a página `Error` sem detalhes internos. As páginas Web 403/404 têm layout próprio. Endpoints JSON de endereço e notificação mantêm seus retornos HTTP.
