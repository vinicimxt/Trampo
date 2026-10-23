# Metodologia ágil e backlogs finais

Consolidação retrospectiva em 26/09/2026. Os relatórios e commits são evidência de evolução incremental; não comprovam reuniões Scrum, velocidade, estimativas, horas ou quadro externo. Não foram inventadas datas de início/fim de sprints.

## Evolução documentada

| Sprint | Entrega predominante | Evidência |
| --- | --- | --- |
| 1 | Segurança/integridade e regressão | [runner histórico](../Tests/Sprint1/Program.cs); sem relatório autônomo localizado |
| 2 | Serviços, agenda e revisão de UI | [relatório](Sprint2/RELATORIO.md) |
| 3 | Application Services e autorização compartilhada | [relatório](Sprint3/RELATORIO.md) |
| 4 | API REST/JWT | [relatório](Sprint4/RELATORIO.md) |
| 5 | Endereços estruturados/snapshots | [relatório](Sprint5/RELATORIO.md) |
| 6 | Mobile inicial e integração real | [relatório](Sprint6/RELATORIO.md) |
| 7 | Git/CI/scripts/secrets | [relatório](SPRINT-7-DEVOPS.md) |
| 8 | Preparação cloud/health/publish | [relatório](SPRINT-8-CLOUD.md) |
| 8.5 | Identidade visual e UX Mobile | [relatório](SPRINT-8-5-MOBILE-UX.md) |
| 9 | Funcionalidades cliente Mobile | [relatório](SPRINT-9-MOBILE-FUNCIONALIDADES.md) |
| Pós-9 | Correção do ID vazio no cadastro de local | Commit backend `71f16ce`; [teste](../Tests/Sprint1/LocalFormRegression.cs) |
| 10 | Auditoria, documentação, regressão e preparação da entrega | [fechamento](SPRINT-10-FECHAMENTO.md) |

## Product Backlog final

Prioridades abaixo organizam a entrega atual; não são estimativas históricas.

| Item | Prioridade | Situação | Sprint |
| --- | --- | --- | --- |
| Autenticação, autorização e integridade de reservas | Alta | Implementado/testado | 1–4 |
| Serviços, disponibilidade e gestão profissional Web | Alta | Implementado | 2–3 |
| API/JWT compartilhando Services | Alta | Implementado | 4 |
| Endereço manual/CEP e snapshots físicos | Alta | Implementado | 5 |
| Mobile cliente: login, catálogo, agenda/reserva | Alta | Implementado | 6 |
| Pipelines e configuração segura | Alta | Implementado; CI Android remoto pendente | 7–8 |
| Publicação Release/health/preparação cloud | Alta | Validado local; deploy não realizado | 8 |
| UX/identidade Mobile | Alta | Implementado; validação assistiva parcial | 8.5 |
| Cadastro, perfil/senha, busca, conclusão/avaliação, avisos/suporte Mobile | Alta | Implementado | 9 |
| Cadastro de local com ID inicial válido | Alta | Corrigido/testado no baseline | Pós-9 |
| Documentação final, MER, matrizes e roteiro | Alta | Consolidado | 10 |
| Capturas revisadas e ensaio completo da apresentação | Alta | Checklist; execução conforme testes finais | 10 |
| Cloud real, TLS, backups e CD | Futura | Proposta; fora desta entrega funcional | Futura |
| Assinatura/publicação Play Store | Futura | Pendente | Futura |
| Mapas/GPS, chat, gateway de pagamento, push | Futura | Não implementados | Futura |
| Refresh token, revogação e observabilidade avançada | Futura | Não implementados | Futura |
| Profissional/Admin Mobile, nota agregada, filtros avançados | Futura | Fora do foco cliente atual | Futura |

## Sprint Backlog 10

| Item | Critério de conclusão | Situação |
| --- | --- | --- |
| Auditar ambos os repositórios | Baseline, status, ignorados, fontes/configuração revisados | Concluído |
| Mapear requisitos | Distinguir implementação/evidência/pendência | Concluído |
| Consolidar arquitetura, banco e API | Fontes reais e limites explícitos | Concluído |
| Documentar paridade, DevOps/cloud e ágil | Histórico preservado e CI remoto verificado | Concluído |
| Executar regressão e publicação | Contagens, falhas e avisos em TESTES-FINAIS | Concluído conforme resultados registrados |
| Auditar segurança | Sem revelar valores; limites e exemplos identificados | Concluído |
| Preparar README, roteiro e checklist de evidências | Material utilizável por avaliador | Concluído |
| Revisar Git/links | Sem gerados no conjunto versionável | Concluído conforme fechamento |
| Smoke visual e capturas completas | Fluxo cliente/profissional com fixtures seguras | Ver alcance e pendências em TESTES-FINAIS |
| Corrigir/validar CI Android remoto | Novo job aprovado após causa comprovada | Pendente; sem push autorizado |

## Kanban de fechamento

| A Fazer | Em andamento | Concluído |
| --- | --- | --- |
| Conferir rubrica oficial do PIM | Nenhum trabalho funcional novo | Auditoria e matrizes |
| Ensaiar demo e completar evidências visuais pendentes | — | Arquitetura/MER/API documentados |
| Diagnosticar log completo do setup Android remoto e revalidar | — | Regressão local/publish e relatório |
| Autorizar commit/push e registrar novo CI | — | README, roteiro e checklist |
| Evoluções futuras priorizadas pelo grupo | — | Preservação da documentação histórica |

Esta tabela representa o fechamento desta execução, não um histórico de movimentação de cards. A sprint não adicionou funcionalidades nem promoveu refatoração de arquitetura.

## Atualização posterior — Sprint 12 Desktop (12/10/2026)

Após o fechamento histórico da Sprint 10, a Sprint 12 acrescentou Windows Forms C# para o Profissional: login JWT, agendamentos, serviços e locais via API. Build, runner não visual e smoke de integração local constam no [fechamento Desktop](SPRINT-12-DESKTOP.md). Quatro das oito janelas têm captura visual enviada pelo usuário; a revisão visual completa e o CI remoto Desktop permanecem pendentes. Este registro complementa o backlog histórico acima, sem transformar as atividades da Sprint 10 em entregas daquela época.
