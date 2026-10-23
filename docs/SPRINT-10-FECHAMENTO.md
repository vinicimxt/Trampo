# Sprint 10 — fechamento final do PIM

Fechamento em 26/09/2026. Entrega documental e validação do projeto existente, sem funcionalidades novas, dependências, alteração de schema ou refatoração de arquitetura. As pendências de apresentação e CI abaixo permanecem explícitas.

## 1. Baseline e retomada

Backend `71f16ceefbebd21b7097f29ad1f43dbe9ad6eaae`; Android `ba0064bf748b8905a9b38bd7a47f60ae2d50df66`. Ambos já possuem Git. O estado sem Git do Android descrito na Sprint 8.5 é histórico.

Na retomada, README e 11 documentos finais, além de três capturas, já estavam preparados; os logs de regressão e smoke de 26/09 estavam disponíveis. Foram conferidos antes de consolidar este relatório. Não se apresenta essa conferência como nova execução das suítes. Relatórios anteriores foram preservados no lugar, sem mover ou apagar documentação.

## 2. Requisitos cobertos

A [matriz PIM](PIM-REQUISITOS.md) relaciona implementação, evidência e situação para organização/proposta, responsabilidade social, diversidade, acessibilidade, Web, Mobile, integração, API, orientação a objetos, arquitetura, banco/MER, SQL, cloud, DevOps/CI, ágil, backlogs, Kanban, testes e segurança.

A rubrica oficial não foi fornecida. Caracterização empresarial, impacto social e pesquisa de mercado não foram comprovados. Procedures/triggers de negócio não existem; confirmar se são exigência acadêmica. Recursos de acessibilidade não representam certificação completa.

## 3. Arquitetura

[Arquitetura final](ARQUITETURA-FINAL.md): MVC/sessão e Android Activity → ViewModel → Repository → Retrofit → API/JWT convergem nos Services/DAOs/SQL. Alguns módulos MVC legados ainda acessam DAO diretamente. A proposta cloud está separada da arquitetura efetivamente executada.

## 4. Banco

[Banco e MER](BANCO-DE-DADOS.md): 15 tabelas, 20 FKs, 16 CHECKs e 20 índices incluindo PK/UNIQUE, conforme metadados inspecionados. ScriptBD + migração 001 representam o schema final; ScriptBD sozinho não basta. Não foi recriado banco vazio nem reaplicada migração nesta sprint.

Na conferência final, foi removida uma instrução de exemplo com senha literal do bloco comentado de administração em ScriptBD.txt, substituída por orientação de cadastro com hash. Nenhum SQL executável/schema foi alterado por essa edição.

## 5. API

[Inventário](API-FINAL.md): 32 operações REST por domínio, método/rota, autenticação, perfil, finalidade e status. Inclui Auth, Perfil, Serviços, Endereços/Locais, Agendamentos, Avaliações, Notificações e Suporte. Health é operacional; Swagger/OpenAPI somente Development. Nenhuma rota nova nesta sprint.

## 6. Web

Cliente, Profissional e Admin conforme [matriz de paridade](MATRIZ-WEB-API-MOBILE.md). Smoke HTTP/HTML registrou login profissional, Home, catálogo, dashboard, serviços, locais, Swagger e cadastro de local com CEP normalizado. Não equivale a revisão visual completa em navegador. Admin tem regressão automatizada, sem nova captura nesta sprint.

## 7. Mobile

Foco Cliente preservado. Smoke com APK gerado nesta sprint percorreu cadastro, Home, perfil, busca, detalhe, agenda, reserva, lista, conclusão, avaliação, notificações, suporte e logout/login. Conclusão usou somente reserva temporária preparada no SQL para ficar elegível; não demonstra finalização profissional pela interface. Cancelamento foi exercitado pela integração, sem repetição visual nesta sprint.

## 8. DevOps

[Consolidação](DEVOPS-E-CLOUD.md): Git, workflows, scripts, secrets por ambiente, publish e health. Backend e Android mantêm repositórios separados. Sem commit, push, dispatch remoto ou reescrita de histórico nesta execução. Artefatos, backups, logs e APK permanecem locais/ignorados.

## 9. Cloud

Proposta documentada: HTTPS → ASP.NET Core → SQL Server gerenciado, com secrets, logs/health e backups. Release backend publicado localmente; nenhum deploy real. Android Release exige endpoint HTTPS e assinatura futura; não há publicação Play Store.

## 10. Testes e resultados

Logs existentes desta sprint conferidos; detalhes/reprodução em [TESTES-FINAIS](TESTES-FINAIS.md).

| Verificação | Resultado |
| --- | --- |
| Restore/build Release/analisadores/publish | Concluídos |
| Testes puros dentro do CI | 12 verificações aprovadas, também incluídas na histórica |
| Sprint 8 | 15 verificações aprovadas |
| Regressão histórica | 296 verificações aprovadas |
| Sprint 9 | 48 verificações aprovadas |
| Integração Mobile/API/SQL | 6 verificações do runner e LiveApiTest Kotlin aprovados |
| Android JUnit comum | 30 descobertos, 29 aprovados, 1 ignorado; zero falhas/erros |
| Android lint/build Debug | Zero erros, APK gerado |

Não somar contagens heterogêneas nem o LiveApiTest novamente como teste novo. Backend: 109 avisos únicos de nulabilidade registrados. Android: 11 warnings de lint (dez sugestões de atualização e um TextFields), mais aviso JVM de acesso nativo. Nenhuma supressão/atualização de dependência feita para ocultá-los.

## 11. CI remoto

Resultados consultados nesta sprint e vinculados em [DevOps](DEVOPS-E-CLOUD.md): backend aprovado no baseline; Android falhou em setup-android antes de testes/lint/build. A causa exata não foi comprovada. Não afirmar CI Android remoto aprovado com base no build local. Documentação atual ainda não enviada ao remoto.

## 12. Segurança e limpeza

[Auditoria](SEGURANCA-FINAL.md) distingue revisão por padrões de uma auditoria exaustiva. User Secrets não está versionado; configuração SQL de desenvolvimento usa autenticação integrada. Release Android bloqueia HTTP. Nenhum segredo foi incluído neste relatório.

Na retomada final ainda existiam os dois usuários e o serviço da fixture registrada em artifacts/sprint10/fixture.json. Foram removidos em transação, por identidades exatas, com verificação de ownership e bloqueio caso houvesse participante externo. Foram removidos também seus registros dependentes de teste. Consulta final confirmou **zero usuários da fixture e zero serviço da fixture**; evidência local em artifacts/sprint10/cleanup-final.txt. Nenhum registro preexistente foi alvo da limpeza.

O PID salvo do backend de smoke já não estava ativo; ADB não mostrou emulador conectado. Nenhum processo alheio foi encerrado. Arquivos de configuração/fixture locais continuam ignorados; não entram na entrega versionável. Nenhum arquivo rastreado corresponde ao .gitignore nos dois repositórios.

## 13. Documentação e inventário da entrega

Arquivos criados, ainda não rastreados no fechamento:

- docs/PIM-REQUISITOS.md
- docs/ARQUITETURA-FINAL.md
- docs/BANCO-DE-DADOS.md
- docs/API-FINAL.md
- docs/MATRIZ-WEB-API-MOBILE.md
- docs/DEVOPS-E-CLOUD.md
- docs/AGIL.md
- docs/ROTEIRO-APRESENTACAO.md
- docs/EVIDENCIAS-CHECKLIST.md
- docs/SEGURANCA-FINAL.md
- docs/TESTES-FINAIS.md
- docs/SPRINT-10-FECHAMENTO.md
- docs/evidencias/mobile-home.png
- docs/evidencias/mobile-notificacoes.png
- docs/evidencias/mobile-suporte.png

Arquivos rastreados alterados: README.md (entrada da entrega) e ScriptBD.txt (somente exemplo comentado). Android sem alteração de fontes nesta sprint. Scripts auxiliares/logs locais em artifacts/sprint10 não fazem parte desse inventário versionável.

## 14. Evidências

[Checklist](EVIDENCIAS-CHECKLIST.md) registra capturas pendentes e três imagens selecionadas de Home, Notificações e Suporte. Logs locais: backend-ci.log, historica.log, sprint9.log, mobile-api-sql.log e android.log em artifacts/sprint10. Resultados JUnit comuns foram preservados antes da integração filtrada.

Links relativos da documentação final e diff de whitespace foram conferidos. As imagens selecionadas foram revisadas na execução registrada em TESTES-FINAIS; não se alega nova inspeção visual nesta retomada.

## 15. Limitações e riscos para apresentação

- Completar seleção/revisão das demais capturas e ensaiar a demo; Web teve validação HTTP/HTML, sem revisão visual completa.
- CI Android remoto permanece pendente. Consultar log integral e corrigir somente a causa comprovada antes de revalidar.
- Texto cosmético “Conclus?o confirmada.” observado no smoke; preservado como pendência, sem alteração funcional no fechamento.
- TalkBack, fonte 200%, orientação horizontal e outros Android não foram revalidados.
- Demo depende de SQL/backend/emulador locais e fixture nova; a fixture desta sprint foi removida. Preparar horários futuros e evitar mutações repetidas/rate limit.
- Cloud real e assinatura/publicação Android não disponíveis; usar os diagramas rotulados como proposta.
- Rubrica acadêmica pode exigir evidências adicionais; não declarar requisitos pendentes como implementados.

## 16. Próximos passos

Usar o [roteiro de 10–15 minutos](ROTEIRO-APRESENTACAO.md), conferir a rubrica e ensaiar com contas temporárias novas. Completar capturas, examinar o CI Android e revisar o conjunto de arquivos antes de autorizar commit/push. Não foi executado push automático.

Evoluções fora desta entrega: deploy cloud real, assinatura/Play Store, mapas/GPS, chat, gateway de pagamento, push notification, refresh token/revogação, Profissional/Admin Mobile, avaliação agregada, filtros avançados e observabilidade avançada. São propostas futuras, não funcionalidades entregues.
