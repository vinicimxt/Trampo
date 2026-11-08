# Matriz Web × API × Mobile

> As primeiras seções preservam baselines históricos. Para o estado atual e os limites de validação, consulte a [matriz final de execução](#matriz-final-de-execução--08112026).

Estado inspecionado em 26/09/2026. Android tem foco **Cliente**; presença de endpoint profissional não significa tela profissional no aplicativo. Fontes: [controllers](../Controllers), [Views](../Views), [API final](API-FINAL.md), [Android](https://github.com/vinicimxt/TRAMPO-APP/tree/ba0064bf748b8905a9b38bd7a47f60ae2d50df66/app/src/main).

| Funcionalidade Cliente | Web | API | Android | Limite |
| --- | --- | --- | --- | --- |
| Cadastro | Sim | POST auth/register | Sim | API cria somente cliente |
| Login | Sessão | JWT | Sim | App recusa perfis profissional/admin |
| Logout | Encerra sessão | Sem revogação | Remove JWT local | Token emitido pode durar até expirar |
| Perfil | Sim | GET/PUT perfil | Sim | Nome/telefone editáveis; email somente leitura |
| Senha | Sim | PUT perfil/senha | Sim | Exige senha atual; não revoga JWT antigo |
| Catálogo | Sim | GET servicos | Sim | Serviços ativos |
| Pesquisa/filtros | Texto/categoria/subcategoria | Texto/modalidade/paginação | Texto/modalidade/paginação | Seletores avançados Mobile adiados |
| Detalhe | Serviço/perfil público | Serviço com nomes públicos | Serviço/profissional/categoria/subcategoria | Perfil profissional expandido não implementado no app |
| Agenda | Sim | GET servicos/{id}/agenda | Sim | Horários calculados pelas regras compartilhadas |
| Agendamento | Sim | POST agendamentos | Sim | Online/Local/Domicilio |
| Meus agendamentos | Sim | GET agendamentos | Sim | Dados do participante autenticado |
| Cancelamento | Sim | POST cancelar | Sim | Regras de estado/prazo |
| Conclusão | Sim | POST confirmar-conclusao | Sim | Após finalização pelo profissional |
| Avaliação | Sim | GET/POST avaliacao | Sim | Atendimento elegível, nota 1–5, uma avaliação |
| Notificações | Lista/leitura | GET e POST ler | Lista/leitura | Internas; sem push |
| Suporte | Formulário | POST suporte | Formulário | Grava chamado, não é chat |
| Consulta CEP | Sim | Consulta/pesquisa | Consulta por CEP | Manual disponível; sem mapas/GPS |

## Área Profissional

Web oferece perfil profissional, dashboard, serviços, disponibilidade, locais e gestão de agendamentos recebidos (confirmar, recusar e finalizar). API expõe serviços/locais e transições profissionais, mas **Android não oferece essa área**. Plano Premium/checkout Web é uma simulação persistida; não há gateway ou processamento financeiro real.

## Área Admin

Web possui dashboard protegido por perfil com indicadores e listagens. Não há aplicativo Admin nem conjunto de endpoints REST administrativos. Acesso de smoke só com fixture segura; não usar contas ou dados administrativos reais em captura.

## Evidência versus validação

Esta matriz afirma presença no código. Resultados de execução estão separados em [testes finais](TESTES-FINAIS.md). O smoke histórico da Sprint 9 não deve ser apresentado como nova execução da Sprint 10. Acessibilidade visual/assistiva completa e paridade integral entre as três interfaces não foram certificadas.

## Windows Forms / Desktop — Profissional (atualização de 12/10/2026)

O nome do arquivo permanece por compatibilidade com os links históricos. O Desktop Windows Forms usa a mesma API e os mesmos Application Services do Web e Android. Autentica-se por JWT, mantém o token apenas em memória e não consulta SQL diretamente. A Web continua com Cliente, Profissional e Admin; Android permanece focado no Cliente.

| Função profissional | Web | API | Desktop | Limite |
| --- | --- | --- | --- | --- |
| Login e identificação | Sessão | JWT e `auth/me` | JWT; somente Profissional | Sem persistência local de token |
| Agendamentos recebidos | Sim | Consulta, detalhe e transições | Lista, detalhe, confirmar, recusar e finalizar | Conforme estado e ownership da API |
| Serviços próprios | Sim | CRUD e categorias | Lista, cadastro, edição e remoção/desativação | Regras de validação no backend |
| Locais de atendimento | Sim | CRUD e consulta de CEP | Lista, cadastro, edição e exclusão | CEP é auxiliar; entrada manual disponível |
| Admin | Sim | Sem área REST administrativa equivalente | Não | Papel exclusivo da Web nesta interface |

[Implementação, testes e limites visuais da Sprint 12](SPRINT-12-DESKTOP.md). As quatro capturas enviadas na conversa não estão no repositório e exigem recorte/anonimização antes de publicação.

## Atualização Sprint 13 — Desktop Cliente e Profissional

A seção anterior registra o estado histórico da Sprint 12. O Desktop atual identifica o perfil pelo `auth/me` e abre painéis separados. O Android atual contém telas Cliente e Profissional. As lacunas REST de avaliações, notificações, suporte e perfil profissional registradas na auditoria foram corrigidas na estabilização final; veja o [relatório V2](HOMOLOGACAO-FINAL-2026-V2.md).

| Funcionalidade | Web | Android atual | Desktop atual | API/limite |
| --- | --- | --- | --- | --- |
| Cliente: busca de serviços | Sim | Sim | Sim | Texto, modalidade, categoria, subcategoria e profissional; filtros novos aplicados antes da paginação |
| Cliente: disponibilidade e reserva | Sim | Sim | Sim | Agenda e criação validadas no backend |
| Cliente: acompanhar/cancelar/concluir | Sim | Sim | Sim | Participante e estado validados no Service |
| Cliente: avaliar atendimento próprio | Sim | Sim | Sim | GET/POST por agendamento elegível; sem lista REST de avaliações próprias |
| Cliente: perfil/senha | Sim | Sim | Sim | Nome/telefone e senha pelo JWT |
| Cliente: notificações | Sim | Sim | Sim | Lista e marcação de leitura |
| Cliente: suporte | Formulário | Formulário | Lista e envio | GET próprio acrescido na Sprint 13; banco não possui texto de resposta |
| Profissional: agendamentos recebidos | Sim | Telas presentes | Sim | Confirmar, recusar e finalizar pelo backend |
| Profissional: serviços e locais | Sim | Telas presentes | Sim | CRUD com ownership no backend |
| Comentários de avaliações públicas/recebidas | Web | Telas/chamadas presentes | Não | GET público por serviço ativo e GET de recebidas pelo profissional autenticado |
| Admin | Sim | Não | Não | Acesso Web |

[Auditoria Desktop Cliente](SPRINT-13-DESKTOP-CLIENTE-AUDITORIA.md) · [Implementação Sprint 13](SPRINT-13-DESKTOP-MULTIPERFIL.md).

## Estabilização final — Android Profissional

| Tela existente | API atual | Validação |
| --- | --- | --- |
| Avaliações recebidas | `GET /profissionais/me/avaliacoes` | 200 Profissional, 403 Cliente, 401 anônimo; proprietário derivado do JWT |
| Notificações | `GET /notificacoes`, `POST /notificacoes/{id}/ler` | Lista própria em ambos os perfis; leitura cruzada retorna 404 |
| Suporte | `GET /suporte`, `POST /suporte` | Chamados próprios em ambos os perfis; envio Profissional verificado |
| Meu perfil e senha | `GET/PUT /perfil`, `PUT /perfil/senha` | Contrato existente compartilhado com Cliente; nenhum campo novo |
| Avaliações públicas no detalhe Cliente | `GET /servicos/{id}/avaliacoes` | 200 para serviço ativo, 404 para inexistente; lista não vazia validada no fluxo `[PIM-FINAL]` |

O build e os testes Android passaram. A navegação visual no emulador continua pendente por falta de aceleração de hardware neste ambiente; isso não é validação visual das telas.

## Matriz final de execução — 08/11/2026

Esta seção é a leitura **atual**; as seções acima preservam o histórico. `PARCIAL` significa que há implementação e teste de contrato, runner ou smoke HTTP, mas a tela não foi homologada visualmente nesta execução. `N/A` significa que a função não pertence ao papel/interface. `OK` exigiria execução real da função na interface, incluindo a inspeção visual prevista no plano. Não houve falha funcional reproduzida nas linhas abaixo, portanto nenhum item recebeu `FALHOU`.

| Funcionalidade | Web Cliente | Web Profissional | Android Cliente | Android Profissional | Desktop Cliente | Desktop Profissional |
| --- | --- | --- | --- | --- | --- | --- |
| Login | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL |
| Perfil | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | N/A |
| Busca | PARCIAL | N/A | PARCIAL | N/A | PARCIAL | N/A |
| Serviços | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL |
| Locais | N/A | PARCIAL | N/A | PARCIAL | N/A | PARCIAL |
| Disponibilidade | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL |
| Agendamento | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL |
| Confirmação | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL |
| Recusa | N/A | PARCIAL | N/A | PARCIAL | N/A | PARCIAL |
| Finalização | N/A | PARCIAL | N/A | PARCIAL | N/A | PARCIAL |
| Cancelamento | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL |
| Avaliações | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | N/A |
| Notificações | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | N/A |
| Suporte | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | N/A |
| Logout | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL | PARCIAL |

| Presença no produto atual | Web | Android | Desktop |
| --- | --- | --- | --- |
| Cliente | SIM | SIM | SIM |
| Profissional | SIM | SIM | SIM |
| Admin | SIM | NÃO | NÃO |

As rotas REST de avaliações públicas e recebidas, notificações, suporte e perfil profissional passaram nos testes de contrato. Os smokes Desktop executaram consulta e CRUD profissionais; o fluxo `[PIM-FINAL]` atravessou as contas Cliente e Profissional pela API e teve comentário verificado no HTML Web. A ausência de dispositivo/emulador funcional e a falha da ferramenta visual impedem afirmar execução de interface Android/Web/Desktop nesta rodada. Detalhes e fixtures: [relatório V2](HOMOLOGACAO-FINAL-2026-V2.md).
