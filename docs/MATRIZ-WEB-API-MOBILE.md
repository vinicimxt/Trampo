# Matriz Web × API × Mobile

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
