# Sprint 10.5 — refinamento visual e responsividade Web

## Baseline antes das alterações — 26/09/2026

- Preservadas alterações/documentação da Sprint 10 que já estavam no working tree. Backend no commit 71f16ce. Nenhum arquivo funcional/API/SQL será alterado nesta sprint.
- Auditados layout/rodapé, CSS/JS e views prioritárias: Home, login/cadastro, catálogo/perfil público, agendamento/lista, dashboard profissional, serviços, locais e Admin.
- CSS compartilha nomes de tokens, mas os redefine em vários arquivos; cores azuis/verdes literais e estilos de cards/formulários diferem entre páginas. CSS global possui regras repetidas e breakpoints de 480 a 1200px.
- Logo original já existe no header, rodapé e autenticação; manter proporções e arquivo original.
- Header fixo com sete links mais ações ocupa espaço em larguras intermediárias. Menu da Home recebe dois handlers (site.js/home.js), com classes diferentes e busca de ícone ausente; precisa de um controlador único com estado acessível.
- Auth usa split-screen e tela inteira sob header fixo; formulários e modais requerem revisão de espaço/rolagem.
- Catálogo já tem filtros, descrição, preço e modalidade reais; manter data-attributes, IDs e ações. Badges de status existentes mantêm texto.
- Home estática contém números e depoimentos sem evidência, promessas de renda/tempo e lembretes automáticos; substituir por apresentação acadêmica fiel às funções existentes.
- Recursos externos de fontes/ícones e VLibras já existentes; nenhum novo asset remoto será introduzido.
- Baseline por inspeção de fontes; ainda sem captura anterior em navegador. Controle de navegador/Windows falhou ao inicializar o processo auxiliar. Será tentada alternativa com navegador local instalado.
- Backup local das Views/CSS/JS em artifacts/sprint105/before (ignorado).

## Resultado

Refinamento implementado em Razor, CSS e JavaScript. Nenhum Controller, Service, DAO, API, schema, autenticação ou fonte Android foi alterado nesta sprint. As alterações preexistentes de README/ScriptBD e os documentos da Sprint 10 foram preservados.

## Identidade e decisões visuais

- `trampo-theme.css` reúne os tokens antes repetidos em 12 folhas de estilo: primária roxa, apoio ciano, fundos claros, texto escuro e cores semânticas. Nomes de variáveis existentes foram mantidos para compatibilidade. Cores legadas literais recorrentes foram substituídas por tokens nos arquivos prioritários; nem todo estilo legado foi reescrito.
- Logo original reutilizada, sem alteração do PNG e com proporção preservada. Fontes possuem fallback local; nenhum pacote, biblioteca ou asset remoto novo.
- Home com hero, ações claras, marca, benefícios, modalidades, etapas e proposta acadêmica. Removidos números/depoimentos fixos e promessas não comprovadas. Perfil apresenta profissional cadastrado, sem alegar verificação inexistente ou confirmação imediata.
- Layout mantém rotas, perfis, campos, antiforgery, IDs e data-attributes. Header mais compacto, rodapé revisado, estados vazios e badges existentes preservados.
- Cards, preços, inputs, ações e cabeçalhos refinados com hierarquia comum. Status/modalidade continuam com texto, não somente cor.

## Telas refinadas

| Área | Alteração |
| --- | --- |
| Home | Composição, logo, CTAs, conteúdo fiel ao projeto e seções responsivas |
| Login/cadastro | Formulário em superfície clara, espaçamento sob header, campos/ações e campos profissionais preservados |
| Catálogo | Filtros responsivos, labels acessíveis, preço destacado e ordem correta de CSS |
| Perfil profissional | Contraste, serviços/CTA, textos de apresentação e CSS no head |
| Novo agendamento | Tema, cabeçalho, campos, slots e layout responsivo existentes |
| Meus agendamentos | Ações sem corte; filtros deixam de ficar fixos no celular |
| Dashboard profissional | Indicadores, gráfico e grid de taxas sem ultrapassar a tela estreita |
| Meus serviços/locais/recebidos | Cards, hierarquia, ações, textos longos e modal rolável |
| Admin | Tema, indicadores/listas e cabeçalho; sem funções novas |

Algumas Views só receberam versionamento de CSS; o refinamento dessas telas está na folha compartilhada e nos tokens. `Recebidos.css` foi alinhado ao nome físico `recebidos.css`, preservando compatibilidade com sistemas de arquivos sensíveis a maiúsculas.

## Responsividade e acessibilidade

Menu compacto até 1180px, layouts de formulário/painel adaptados até 900px, filtros/ações/modais até 600px e indicadores em uma coluna nas larguras menores. A validação considerou a largura útil do documento, descontando a barra de rolagem; não se usou `overflow-x:hidden` para encobrir defeitos.

Adicionados link para pular ao conteúdo, foco visível, rótulo/estado expandido do menu e região anunciável de mensagens. Menu tem um único controlador; fecha por Escape, clique fora e navegação, devolvendo foco no Escape. Controle duplicado em home.js foi removido. Modais mantêm o tratamento de foco existente e ganham altura máxima/rolagem interna. Preferência por movimento reduzido é respeitada. VLibras foi preservado.

## Validação executada

Edge local em modo headless, com perfil de teste separado, backend Development em 127.0.0.1:5185 e SQL Express real. Os conectores de controle visual não iniciaram; foi usado o protocolo de depuração do Edge com scripts Python de biblioteca padrão, somente locais e ignorados no Git. As capturas PNG foram efetivamente inspecionadas; essa inspeção não constitui teste automatizado de comparação visual.

- Build final: `dotnet build BD-TRAMPO.csproj -c Release --no-restore --output artifacts/sprint105/build-final --no-incremental --nologo`: sucesso, zero erros, 109 avisos de nulabilidade existentes. Um comando inicial no nível da solução gerou NETSDK1194; foi corrigido para apontar explicitamente ao projeto.
- Telas públicas e autenticadas exercitadas em 1440, 768, 390 e 320px; perfil profissional também conferido autenticado em desktop/celular. Medidas DOM de overflow registradas em JSON.
- Smoke de interface: login das fixtures cliente/profissional/admin, navegação, busca de serviço, campos dinâmicos de cadastro profissional, menu/Escape, modal de local/Escape e conferência de ações de reserva. Sem envio de nova reserva, edição/exclusão de serviço ou chamado real.
- O perfil público redireciona visitantes para Login por regra preexistente. A primeira tentativa anônima evidenciou esse comportamento; a validação foi refeita autenticada. Não se alterou a autenticação.
- Detectados e corrigidos nesta iteração: corte das ações de reserva, largura do gráfico/grid de taxas, contraste de cabeçalhos e ordem de CSS do catálogo/perfil. Capturas intermediárias permanecem locais.
- `git diff --check -- Views wwwroot`: sem erros. O check global ainda aponta linha vazia final no README preexistente, fora desta sprint.

Nenhuma suíte de regressão backend/Android foi reexecutada como parte da Sprint 10.5: o escopo alterou apresentação. Resultados históricos da Sprint 10 não são apresentados como novos testes. Não foram criados testes visuais automatizados permanentes.

## Evidências e limitações

Capturas e scripts em `artifacts/sprint105/`, ignorados: `browser-audit.json`, `auth-audit.json`, `interactions.json`, `build-final.log` e PNGs desktop/mobile. As imagens Admin têm valores e nomes ocultados no DOM apenas para a captura, sem alteração de dados. Capturas iniciais de catálogo podem conter registros públicos preexistentes e permanecem locais; a evidência selecionada do catálogo usa filtro da fixture.

Validação por viewport no Edge não substitui dispositivo físico, Safari/Firefox, leitor de tela ou certificação WCAG. Não foram percorridas todas as combinações de status, tema de sistema, zoom 200%, CEP externo, formulários de pagamento ou páginas secundárias. Fontes/ícones externos e VLibras já existentes continuam dependendo de rede. Conteúdos longos e componentes dinâmicos merecem revisão humana antes da apresentação.

Nenhum commit, push ou deploy realizado. Não foram alteradas regras de negócio para acomodar o visual.

## Inventário exato desta sprint

Criados: `wwwroot/css/trampo-theme.css` e este relatório. Capturas selecionadas são listadas abaixo após a revisão.

Arquivos alterados (comparação com o backup anterior às edições):


- `Views/Admin/Dashboard.cshtml`
- `Views/Agendamento/Meus.cshtml`
- `Views/Agendamento/Novo.cshtml`
- `Views/Agendamento/Recebidos.cshtml`
- `Views/Home/Index.cshtml`
- `Views/Local/Lista.cshtml`
- `Views/Profissional/Dashboard.cshtml`
- `Views/Profissional/Lista.cshtml`
- `Views/Profissional/MeusServicos.cshtml`
- `Views/Profissional/PerfilPublico.cshtml`
- `Views/Shared/_Footer.cshtml`
- `Views/Shared/_Layout.cshtml`
- `Views/Usuario/Cadastro.cshtml`
- `Views/Usuario/Login.cshtml`
- `wwwroot/css/Admin-Dashboard.css`
- `wwwroot/css/Locais.css`
- `wwwroot/css/MeusAgendamentos.css`
- `wwwroot/css/MeusServicos.css`
- `wwwroot/css/agendamento.css`
- `wwwroot/css/auth.css`
- `wwwroot/css/dashboard.css`
- `wwwroot/css/home.css`
- `wwwroot/css/perfil-publico.css`
- `wwwroot/css/profissionais.css`
- `wwwroot/css/recebidos.css`
- `wwwroot/css/site.css`
- `wwwroot/js/home.js`
- `wwwroot/js/site.js`

## Conferência final e limpeza

A última execução registrou **44 combinações de página/largura sem overflow horizontal detectado** (16 públicas e 28 autenticadas), além de **13 checks de interação aprovados**. O perfil autenticado foi conferido adicionalmente em 1440/390px. São medidas DOM/checks de smoke, não 57 testes unitários nem comparação visual automatizada. A inspeção direta das capturas complementou essas medidas.

Três usuários temporários (cliente, profissional e admin), um serviço, um local, disponibilidades e duas reservas preparados para o smoke foram removidos por identidades exatas em transação. Verificação de ownership impediu limpeza com participante externo. Contagem final: zero usuários/serviço da fixture. Credenciais temporárias locais removidas; logout realizado e instâncias de backend/Edge de teste encerradas. Nenhum registro preexistente foi alterado.

### Capturas selecionadas

Nove PNGs novos em `docs/evidencias/web-sprint105/`. Capturados em 26/09/2026, no build desta sprint ainda sem commit, Edge local, tema claro. Dados de demonstração; Admin com valores ocultados visualmente. Não são prova de dados operacionais reais.

- [home-desktop](evidencias/web-sprint105/home-desktop.png)
- [home-mobile](evidencias/web-sprint105/home-mobile.png)
- [login-mobile](evidencias/web-sprint105/login-mobile.png)
- [cadastro-mobile](evidencias/web-sprint105/cadastro-mobile.png)
- [catalogo-mobile](evidencias/web-sprint105/catalogo-mobile.png)
- [perfil-mobile](evidencias/web-sprint105/perfil-mobile.png)
- [agendamentos-mobile](evidencias/web-sprint105/agendamentos-mobile.png)
- [servicos-desktop](evidencias/web-sprint105/servicos-desktop.png)
- [admin-mobile](evidencias/web-sprint105/admin-mobile.png)

## Revisão solicitada: retorno da Home e navbar anteriores

Por solicitação do usuário, Home/Index e home.css foram restaurados do backup anterior à Sprint 10.5, incluindo conteúdo e composição originais. Navbar recuperou os links, markup visual, dimensões e breakpoints anteriores. site.css original restaurado; tokens da navbar isolados em navbar-original.css nas demais páginas. O tema novo não é carregado na Home. Mantidos o controlador único do menu e seus atributos acessíveis, sem o handler duplicado antigo.

Os refinamentos das demais telas permanecem. As capturas e os resultados acima documentam a versão anterior a esta reversão; em especial, as imagens Home não representam mais a versão atual. O conteúdo promocional estático original da Home também foi restaurado, conforme o pedido.
Validação da reversão: build Release aprovado (zero erros, 109 avisos existentes); Home HTTP 200 sem trampo-theme.css; sete links antigos da navbar presentes; menu móvel abre; Login mantém o tema refinado. Capturas atuais locais: artifacts/sprint105/restored-home-desktop.png e restored-menu-mobile.png. Menu móvel inspecionado visualmente. Instância de teste encerrada ao finalizar.
