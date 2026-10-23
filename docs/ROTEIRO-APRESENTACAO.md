# Roteiro da apresentação — TRAMPO / PIM IV

Duração sugerida: **13 minutos** (intervalos são planejamento de fala, não horas históricas de trabalho). Adaptar à rubrica da banca. Separar demonstração do que existe de propostas de evolução.

| Tempo | Tema | Fala/artefato |
| --- | --- | --- |
| 0:00–1:00 | Problema | Encontrar profissionais e organizar solicitações/horários sem um fluxo integrado |
| 1:00–2:00 | Proposta TRAMPO | Cliente encontra e agenda; profissional organiza serviços/atendimentos; Admin acompanha indicadores. Protótipo acadêmico, sem alegar operação comercial |
| 2:00–3:00 | Arquitetura | Web MVC, Android Cliente e Windows Forms Profissional convergem na API/Services/DAO/SQL; reconhecer módulos MVC legados |
| 3:00–4:00 | Web | Catálogo e área profissional; local, serviço e disponibilidade preparados |
| 4:00–4:45 | Desktop/demo | Login profissional, painel e listagens de Agendamentos e Serviços com fixture; quatro telas restantes ainda sem captura visual |
| 4:45–6:30 | Mobile/demo | Cliente: entrar, pesquisar fixture, detalhe, agenda, reserva e consulta; mostrar perfil/avisos/suporte |
| 6:30–7:15 | API | Swagger com rotas e JWT, sem exibir token; regras compartilhadas entre Web, Android e Desktop |
| 7:15–8:15 | Banco | MER, chaves/constraints, snapshot de endereço; sem procedures/triggers de negócio |
| 8:15–9:15 | Segurança | Hash, sessão/CSRF, JWT/ownership e secrets externos; JWT sem refresh/revogação imediata |
| 9:15–10:15 | DevOps/Cloud | CI local/remoto separado; Android remoto falhou no setup; cloud é proposta, publish é local |
| 10:15–11:15 | Resultados | Contagens por suíte e limitações de smoke/acessibilidade; não somar testes repetidos |
| 11:15–12:15 | Conclusão/avaliação | Usar reserva temporária já elegível, se preparada; não alterar reserva real nem relógio para forçar estado |
| 12:15–13:00 | Evoluções | Cloud real, assinatura, observabilidade, refresh, mapas/chat/push e áreas Mobile futuras |

## Ordem recomendada da demo ao vivo

1. Antes da banca, compilar APK/backend e iniciar SQL/backend/emulador; testar `/health` **e** catálogo/agenda, pois health sozinho não verifica SQL.
2. Preparar profissional, local, serviço e disponibilidade temporários identificados. Conta cliente dedicada; nenhuma conta real. Aguardar prontidão antes de abrir app.
3. Abrir Home/catálogo Web. Entrar na conta profissional de demo e mostrar serviço/local já existentes; evitar longa digitação.
4. Desktop: entrar com fixture profissional, mostrar painel e listagens já validadas; não exibir as capturas brutas da conversa com outras aplicações ou dados do ambiente.
5. Mobile: login → Home → busca por nome único → detalhe → agenda com data futura válida → uma reserva → Meus agendamentos. Mostrar confirmação antes de repetir uma mutação.
6. Cancelar somente essa reserva, se o roteiro escolher cancelamento. Para conclusão/avaliação, usar outra fixture cujo profissional já finalizou; explicar como foi preparada.
7. Mostrar perfil, notificação fictícia e suporte com texto de teste. Não enviar dados pessoais.
8. Mostrar Swagger sem token visível, MER e links reais de CI. Se a rede falhar, usar documentação/capturas previamente revisadas, explicando que são evidências gravadas.
9. Logout ao fim; remover somente IDs das fixtures e verificar contagem zero. Encerrar apenas processos de demo iniciados para a apresentação.

## Preparação e contingência

- CEP externo pode indisponibilizar: usar endereço preenchido manualmente ou modalidade Online já preparada. Não depender de ViaCEP para demonstrar reserva.
- Não recompilar a aplicação sobre executável aberto; usar saída isolada dos runners. Não executar múltiplos builds no mesmo obj.
- Login/cadastro/senha/suporte compartilham rate limit: evitar tentativas rápidas repetidas. Planejar login válido e não renovar chave durante a demo.
- Preparar rede local e Debug no emulador `10.0.2.2:5165`; endpoint Release real não existe. Não afirmar que HTTP Debug é configuração de produção.
- Não demonstrar cobrança: Premium Web registra uma simulação, não transação financeira real.
- Não depender de CI Android remoto aprovado ou cloud pública inexistente. Abrir [resultados reais](DEVOPS-E-CLOUD.md).
- Capturas de reserva, perfil e admin devem passar pelo [checklist de privacidade](EVIDENCIAS-CHECKLIST.md).

## Respostas defensáveis

**“Está na nuvem?”** Preparado/documentado, com publicação Release validada localmente; nenhum deploy real comprovado.

**“Mobile atende todos os papéis?”** Não. Android tem foco Cliente; Windows Forms atende Profissional; Web atende Cliente, Profissional e Admin.

**“Tudo passou?”** Consultar [TESTES-FINAIS](TESTES-FINAIS.md): testes locais aprovados, warnings conhecidos e CI Android remoto com falha de setup. Não transformar validação parcial em certificação total.

**“Qual o impacto social?”** Potencial de acesso a serviços e oportunidades; não houve estudo de impacto ou validação de mercado.

**“O que falta?”** Itens de [evolução futura](SPRINT-10-FECHAMENTO.md), além da rubrica acadêmica, evidências visuais pendentes e correção comprovada do CI Android remoto.
