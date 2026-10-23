# Evidências e checklist de capturas

Sprint10, 26/09/2026. Checklist de preparação; caixa vazia significa **não concluída/revisada para entrega**. Não confundir capturas históricas, testes automatizados e demonstração visual atual.

## Regras de captura

Usar fixture `sprint10-...`/contas de demonstração identificáveis; jamais dados de cliente real. Não capturar senha preenchida/visível, JWT, conexão, User Secrets, painel de credenciais ou terminal com valores. Ocultar notificações do desktop. Revisar cada imagem antes de versionar. Não mover automaticamente imagens históricas não revisadas para docs.

Pasta local de trabalho: `artifacts/sprint10/` (ignorada). Capturas Sprint9 existentes em `artifacts/sprint9-visual/` são evidência histórica local e não estão disponíveis ao clonar o Git. Selecionar arquivos aprovados e copiar deliberadamente para uma pasta de evidências versionável somente após revisão.

## Mobile — checklist visual para apresentação

| Captura | Mostrar | Situação para entrega |
| --- | --- | --- |
| Login | Marca e formulário vazio | [ ] Revisar captura final |
| Cadastro | Campos sem senha/dados reais | [ ] Revisar captura final |
| Home | Conta de demonstração e navegação | [x] [Captura revisada](evidencias/mobile-home.png) |
| Serviços | Pesquisa/modalidade da fixture | [ ] Revisar captura final |
| Detalhe | Nome/preço/modalidade públicos | [ ] Revisar captura final |
| Agenda | Data e horários disponíveis | [ ] Revisar captura final |
| Agendamento | Reserva temporária e estado | [ ] Revisar captura final |
| Perfil | Nome/telefone fictícios; email de teste | [ ] Revisar captura final |
| Avaliação | Reserva elegível e nota | [ ] Revisar captura final |
| Notificações | Aviso fictício e estado de leitura | [x] [Captura revisada](evidencias/mobile-notificacoes.png) |
| Suporte | Texto fictício, sem dados pessoais | [x] [Captura revisada](evidencias/mobile-suporte.png) |

## Web, API, CI e cloud

| Captura | Critério | Situação |
| --- | --- | --- |
| Home Web | Tela pública, sem sessão real | [ ] |
| Login Web | Campos vazios | [ ] |
| Catálogo | Serviços de demo ou conteúdo público revisado | [ ] |
| Perfil profissional | Fixture, sem contato real | [ ] |
| Agendamento Web | Somente reserva temporária | [ ] |
| Área profissional | Serviços/locais/recebidos da fixture | [ ] |
| Admin | Somente se fixture segura e sem indicadores privados visíveis | [ ] Opcional |
| Swagger | Rotas; botão Authorize fechado, sem token/corpos sensíveis | [ ] |
| Actions backend | Nome, SHA, conclusão e link reais | [ ] Resultado verificado em DEVOPS-E-CLOUD |
| Actions Android | Falha real de setup, sem apresentar como aprovação | [ ] Resultado verificado em DEVOPS-E-CLOUD |
| Cloud | Diagrama rotulado “proposta / não implantada” | [x] Diagrama textual em ARQUITETURA-FINAL |

## Rastreabilidade mínima de cada arquivo

Registrar nome, data de captura, versão/commit, tela, fixture utilizada, resultado esperado/observado e revisão de privacidade. O [relatório de testes](TESTES-FINAIS.md) informa o alcance real do smoke atual. Imagens externas do README antigo permanecem no histórico Git, mas não são usadas como prova atual sem revisão.

## Capturas selecionadas nesta sprint

As três imagens em docs/evidencias foram capturadas em 26/09/2026, Android ba0064b, backend 71f16ce, Pixel_6/API 37, tema claro. Usam somente Cliente Demo Sprint10 e registros temporários. Home mostra autenticação após cadastro; Notificações mostra leitura confirmada; Suporte mostra chamado enviado. Inspeção visual direta verificou legibilidade e ausência de senha/token/dados reais. Não são evidência de testes assistivos completos.

## Revisão pré-Git de imagens existentes — 12/10/2026

As três capturas Android acima e as nove imagens em `evidencias/web-sprint105/` foram examinadas visualmente e quanto a metadados PNG: sem EXIF, texto embutido, senha, token ou dado pessoal aparente. Todas mostram campos vazios ou dados de demonstração e estão **aptas para publicação quanto à privacidade observável**. Isto não completa as capturas de jornadas ainda vazias no checklist nem substitui a confirmação da origem das fixtures pelo grupo.

As quatro capturas Desktop enviadas na conversa não estão no repositório. Elas incluem aplicativos externos, caminho/ambiente de desenvolvimento e nome exibido no painel. **Não publicar as versões brutas**; recortar ou anonimizar e revisar novamente. Continuam faltando evidências visuais de DetalheAgendamentoForm, ServicoForm, LocaisForm, LocalForm e dos estados de redimensionamento/mensagens descritos em [Sprint 12 Desktop](SPRINT-12-DESKTOP.md).
