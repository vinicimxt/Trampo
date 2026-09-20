# Sprint 2 — estabilização dos fluxos

Implementação incremental sobre MVC + DAO. Nenhuma alteração estrutural no SQL Server, API, EF, Mobile ou infraestrutura. Nenhum commit criado.

## Arquivos alterados

| Arquivo | Mudança |
|---|---|
| Controllers/ServicoController.cs | Criação/edição delegam à transação única; remoção é decidida pelo DAO sob bloqueio; mensagens de preservação do histórico. |
| DAO/ServicoDAO.cs | Conexão/transação explícitas reutilizando comandos; grava serviço e regras juntos, valida proprietário/local e elimina dias duplicados. |
| Controllers/LocalController.cs | Mensagens de recusa e parâmetro abrir na lista. |
| DAO/LocalDAO.cs | Recusa exclusão vinculada; impede mudança de endereço/nome já usado no histórico; aceita nome opcional. |
| Controllers/AgendamentoController.cs | Filtra bloqueios e sobreposições na tela de reserva. |
| DAO/AgendamentoDAO.cs | Consulta intervalos indisponíveis; mantém a validação transacional da Sprint 1. |
| Views/Usuario/Cadastro.cshtml | ID formCadastro e indicação de senha mínima de oito caracteres. |
| wwwroot/js/cadastro.js | Toggle sem variável inexistente; mínimo de oito caracteres; rótulo acessível dinâmico. |
| wwwroot/js/login.js | Toggle sem variável inexistente e rótulo acessível dinâmico. |
| Views/Home/Index.cshtml | Links internos absolutos funcionam também em /Home/Index. |
| Views/Servico/Criar.cshtml | Link para /Local/Lista?abrir=true. |
| Views/Local/Lista.cshtml | Abertura do formulário pelo parâmetro; remove aria-label duplicado. |
| Views/Agendamento/Novo.cshtml | Horários selecionáveis são botões nativos. |
| Views/Agendamento/Meus.cshtml | Filtro reconhece status legado Cancelado. |
| Views/Agendamento/Recebidos.cshtml | Filtro reconhece status legado Cancelado. |
| Views/Profissional/MeusServicos.cshtml | Limpa local anterior ao abrir outra edição. |
| Views/Shared/_Layout.cshtml | Semântica/teclado das notificações; guarda para falha no carregamento do VLibras. |
| wwwroot/js/site.js | Atualização e tratamento de falha das notificações; contador coerente; toasts acessíveis; foco, Tab, Escape e restauração do scroll dos modais. |
| Tests/Sprint1/Program.cs | Preserva 66 verificações; adiciona 25 opcionais com --sprint2 e caminho de DLL isolada. |
| Tests/Sprint1/Run.ps1 | Compila/testa sem disputar o executável aberto no IDE. |
| docs/Sprint2/RELATORIO.md | Este relatório. |
| docs/Sprint2/rotas.csv | Inventário das ações MVC. |
| docs/Sprint2/views.csv | Inventário das Views. |
| docs/Sprint2/warnings.csv | Cada aviso com arquivo, linha, coluna, código e causa. |
| docs/Sprint2/testes.log | Build sem supressão e testes. |
| docs/Sprint2/arquivos-gerados.txt | Relação separada dos artefatos de compilação. |

bin/obj já são versionados no projeto e foram atualizados pela compilação. Tests/Sprint1/app é saída gerada. Não são alterações manuais de funcionalidade. conexao.cs não foi alterado nesta Sprint.

## Rotas e Views

Examinadas todas as Controllers, Views, formulários, links internos e fetch dos scripts próprios. Nenhuma View de página ficou sem ação correspondente; layouts, partials, Error e _View* são composição e não requerem ação própria.

Classificações: 1 funcional necessária; 2 funcional incompleta; 3 legado/compatibilidade sem uso na navegação atual; 4 navegação quebrada.

| Caso | Classe | Evidência e decisão |
|---|---|---|
| Links relativos da Home para cadastro/lista | 4 → 1 | Quebravam em /Home/Index; corrigidos para caminhos absolutos. |
| /Local?abrir=true na criação de serviço | 4 → 1 | A rota existia, mas ignorava abrir; destino e abertura corrigidos. |
| Usuario/AreaRestrita e MinhaConta | 3 | Views ausentes e nenhum caller atual; edição vigente é Perfil → SalvarConta. |
| Servico/Novo | 3 | View ausente; criação vigente é Criar → Salvar. |
| Profissional/Contato e SalvarContato | 3 | View Contato ausente; edição atual de contato usa Usuario/Perfil. |
| Profissional/Premium | 3 | View ausente; fluxo atual usa Pagamento/CheckoutPremium. |
| Home/Privacy | 3 | Template existente; navegação usa Home/Privacidade. |
| Servico/Editar GET | 3 | Redirecionamento de compatibilidade; POST do modal é classe 1. |
| Disponibilidade/Index e Criar | 3 | Redirecionam; gestão atual ocorre junto ao serviço. Index ainda faz consulta redundante com identificador inadequado, sem efeito no destino. |
| Disponibilidade/Salvar e Desativar | 3 | Protegidos, sem caller na interface atual; não se criou interface especulativa. |
| Agendamento/DetalhesModal e modalGlobal | 3 | Nenhum fetch/caller encontrado; rota protegida redireciona à lista; detalhes atuais aparecem nos cards. |
| Agendamento/FinalizarProfissional | 3 | Compatibilidade; interface usa Finalizar. |
| Avaliacao/Avaliar | 3 | Redireciona para Meus; avaliação atual é modal com POST Salvar. |
| Servico/Subcategorias | 3 | Alias não utilizado; fetch vigente usa GetSubcategorias. |
| Pagamento/CheckoutPremium e ConfirmarPremium | 2 | Fluxo simulado, sem cobrança real. |

Não se criaram Views para rotas legadas. Não foram encontrados formulários atuais ou chamadas AJAX para ações inexistentes. Links de mutação continuam convertidos em POST com antiforgery pelo script global.

## Consistência e histórico

- Serviço + disponibilidade usam uma conexão e transação SERIALIZABLE, com o mesmo bloqueio TRAMPO:Profissional:{id} usado nas reservas. Proprietário/local são revalidados dentro da transação.
- Edição substitui regras como unidade; dias repetidos não duplicam registros. Exceção antes do Commit descarta a transação.
- Remover serviço com agendamentos o desativa; sem histórico, serviço e regras são excluídos juntos.
- Locais não têm coluna Ativo. Para preservar o esquema, exclusão vinculada é recusada com orientação de cadastrar outro local.
- Mudança de endereço/nome usado em agendamento também é recusada, pois a apresentação histórica consulta esse registro.
- Cancelamentos deixam de bloquear. Inatividade, passado, disponibilidade, bloqueios e concorrência continuam validados no backend. A tela também omite sobreposições parciais e intervalos que cruzam a meia-noite.

## Revisão de frontend

Corrigidos: formCadastro, variável input inexistente nos toggles, mínimo de senha divergente, seleção residual de local, links relativos, badge removido sem leitura persistida, cache permanente do dropdown, falta de teclado na seleção de horários/notificações e falta de gestão do foco e scroll dos modais.

A revisão de IDs encontrou calcularResumoFinanceiro em Recebidos referenciando elementos ausentes; não existe caller, portanto foi classificado como legado. Referências globais opcionais a topbar/campos de cadastro são dependentes do contexto. Não se criaram elementos para código sem uso.

Há media queries em autenticação, reserva e locais. Não houve validação visual em navegador, leitor de tela ou ensaio completo dos estilos dos modais. Os testes de sintaxe e DOM simulado não substituem essa validação.

## Build e testes

- Build final da aplicação: 0 erros e 205 warnings C#, sem supressão.
- Build dos testes: 0 erros e 0 warnings.
- Sprint 1: 66 verificações preservadas e aprovadas.
- Sprint 2: 25 adicionais aprovadas.
- Total: 91 aprovadas, 0 reprovadas na execução final.
- Sintaxe válida em site.js, cadastro.js, login.js, servico.js, PerfilUsuario.js, profissionais.js e home.js.
- Mostrar/ocultar senha e rótulos de cadastro.js/login.js passaram em DOM simulado.
- Nenhuma outra suíte independente foi encontrada.

Os testes usam SQL Server configurado e usuários temporários com GUID; limpam somente registros da execução. Cobrem autorização, antiforgery, concorrência, cancelamento, conclusão, avaliação, falha SQL, substituição de regras, dias duplicados, preservação de local/histórico, inatividade, filtragem de horários e dez rotas atuais.

A falha SQL foi provocada por chave estrangeira inválida na gravação do serviço, verificando ausência de criação parcial e manutenção das regras anteriores. Não foi injetada falha de rede depois da primeira inserção de disponibilidade.

O primeiro dotnet build falhou ao substituir o executável aberto; a compilação final usa saída isolada. Uma tentativa de teste encontrou DLL antiga; o runner agora copia explicitamente a DLL validada. Esses problemas foram resolvidos antes da execução final.

Na raiz do projeto:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint2
~~~

Bypass vale só para esse processo, sem mudar a política do Windows. Sem -Sprint2 executa as 66 verificações originais.

## Warnings

| Código | Quantidade | Causa provável |
|---|---:|---|
| CS8601 | 105 | Atribuição possivelmente nula, principalmente leitura SQL. |
| CS8618 | 45 | Propriedades não anuláveis sem inicialização. |
| CS8604 | 27 | Argumentos possivelmente nulos, incluindo sessão. |
| CS8603 | 12 | Retorno null em método declarado não anulável. |
| CS8600 | 11 | Conversão/atribuição de valor possivelmente nulo. |
| CS8602 | 3 | Possível desreferência nula no perfil público. |
| CS8625 | 2 | Literal null em contexto não anulável. |

Arquivo, linha e mensagem de cada ocorrência constam em warnings.csv. Não houve refatoração geral de nulabilidade. Os contratos legados precisam de revisão gradual.

## Identificados, mas não corrigidos

- Pagamento/assinatura continuam simulados.
- Parte da apresentação histórica lê nome, modalidade, link e preço atuais do serviço. Não existe snapshot completo do contrato; preservar registros e endereços não equivale a versionar todos os atributos.
- Finalização/cancelamento e notificações posteriores merecem revisão transacional conjunta; criação de reserva já é transacional.
- Não existe tela atual para criar bloqueios; bloqueios no banco são respeitados.
- Edição de disponibilidade usa intervalo comum aos dias escolhidos. Regras heterogêneas inseridas fora do fluxo não ganham editor avançado nesta Sprint.
- Rotas e função financeira legadas permanecem, conforme classificação.
- Sem JavaScript, links convertidos em POST recebem recusa de GET; não há alteração indevida de dados.
- Falta validação visual/responsiva e com tecnologias assistivas.
- bin/obj continuam versionados.

## Banco e impacto no PIM IV

Nenhuma tabela, coluna, índice ou constraint foi criada/alterada. Dados de testes foram removidos; sequências IDENTITY podem avançar.

Arquitetura: preservação de MVC + DAO e unidade de gravação no DAO existente.
Segurança: manutenção de perfil, propriedade e antiforgery.
Banco: atomicidade e referências históricas preservadas.
Qualidade: regressão automatizada e inventários rastreáveis.
Experiência: cadastro, notificações, horários e teclado coerentes com as regras.

## Próxima Sprint sugerida

Preparar gradualmente contratos e regras para futura API reutilizável, decidir snapshots históricos e separar responsabilidades pontualmente com testes. Completar a validação visual e tratar as operações transacionais pendentes antes de publicar a API. Nenhuma etapa futura foi implementada.