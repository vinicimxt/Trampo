# Relatório — Sprint 3

## Resultado em 20/09/2026

A camada de aplicação está integrada ao MVC de Agendamento, Serviço e Avaliação. A regressão preserva as 91 verificações anteriores e acrescenta 34: total de 125 aprovadas, zero falhas. A validação visual continua pendente por falha de inicialização do navegador no ambiente; os passos estão em CHECKLIST-MANUAL.md. Não se afirma aceite visual completo.

Esta retomada conferiu a implementação já existente, repetiu build/testes, corrigiu os quatro warnings restantes nos DAOs prioritários e a nulabilidade dos testes, e completou a documentação. Não foi iniciado trabalho da Sprint 4.

## Arquivos criados na Sprint

- Services/AgendamentoService.cs e Services/ServicoService.cs.
- Contracts/Operacao.cs e Contracts/Solicitacoes.cs.
- docs/Sprint3/ARQUITETURA.md, HISTORICO.md, CHECKLIST-MANUAL.md e RELATORIO.md.
- Evidências de build/testes e warnings.csv em docs/Sprint3. Os logs anteriores foram preservados; testes-final.log identifica a última regressão e validacao-final.log contém a compilação da aplicação com 120 warnings.

## Arquivos alterados na Sprint

| Arquivo | Resultado |
|---|---|
| Controllers/AgendamentoController.cs | Recebe Service por DI; binding, sessão, apresentação e redirecionamento permanecem no MVC |
| Controllers/ServicoController.cs | Criação, edição, propriedade e remoção delegadas ao Service; alias Novo aponta para Criar |
| Controllers/AvaliacaoController.cs | Usa AgendamentoService para propriedade, conclusão e avaliação |
| Controllers/BaseController.cs | Mapeia FalhaOperacao para códigos HTTP |
| DAO/AgendamentoDAO.cs | Erros de reserva tipados, transições com notificações na mesma transação e leitura SQL com nulabilidade explícita |
| DAO/ServicoDAO.cs | Mantém transação serviço/disponibilidade; retorno de busca anulável e leituras SQL ajustadas |
| DAO/ClienteDAO.cs | ObterOuCriar serializado para evitar duplicação concorrente do cliente |
| Models/Agendamento.cs e Models/Servico.cs | Campos opcionais ajustados, sem desabilitar nulabilidade |
| Program.cs | Registra os dois Services e seus DAOs no container |
| Views/Profissional/PerfilPublico.cshtml | Evita desreferência de contato/telefone ausente |
| Tests/Sprint1/Program.cs | Preserva regressão e acrescenta 34 verificações da camada de aplicação; valida ausência de fixtures explicitamente |
| Tests/Sprint1/Run.ps1 | Opção -Sprint3 inclui as três Sprints |

bin/obj e Tests/Sprint1/app são artefatos gerados; já existem arquivos versionados nessas pastas. Não foram editados manualmente nem removidos. conexao.cs e o esquema do banco não foram alterados. Nenhum commit foi criado.

## Arquitetura antes e depois

Antes: View → MVC Controller → DAO → SQL Server.

Depois, nos módulos migrados: View → MVC Controller → Application Service → DAO → SQL Server.

MVC e futuros Controllers de API compartilham Services diretamente; MVC não faz HTTP para a própria API. Não foram criados endpoints REST, Mobile, EF, Identity, Docker ou nova solução.

AgendamentoController, ServicoController e AvaliacaoController usam Services. AdminController, Disponibilidade, HomeController, LocalController, NotificacaoController, PagamentoController, ProfissionalController, SuporteController e UsuarioController permanecem no padrão legado. BaseController mantém a infraestrutura compartilhada de sessão/perfil/notificações e consultas legadas. A migração não cobre todas as leituras de serviço no sistema nem o controller legado de disponibilidade.

## Regras e contratos

AgendamentoService autentica o contexto pelo usuário persistido, valida participante/perfil, oferta ativa, autoagendamento, data/hora e endereço; coordena criação, consulta de agenda, cancelamento, confirmação, recusa, finalização, conclusão e avaliação. Calcula valor/taxa conforme regras existentes. Disponibilidade, bloqueios, conflitos e limites são revalidados pelo DAO sob bloqueios SQL: retirar essas verificações da transação abriria condições de corrida.

ServicoService valida propriedade, nome, modalidade, preço, URL, local, subcategoria e disponibilidade. Coordena gravação e remoção preservando histórico. Desativar mantém a semântica vigente de retirar a oferta: serviço sem histórico pode ser excluído; com histórico é desativado. Não foi criada ação de reativação sem fluxo MVC existente.

Contratos utilizados: UsuarioContexto, CriarAgendamentoRequest, SalvarServicoRequest, AvaliarAgendamentoRequest e AgendaDisponivel. TipoFalha/FalhaOperacao distinguem validação, autenticação, permissão, inexistência e conflito; o MVC mapeia para 400/401/403/404/409. Sucesso segue respostas MVC existentes. Services não dependem de HttpContext, Razor, ViewBag, TempData ou IActionResult.

Os Services concretos usam DI. Não foram criadas interfaces espelhadas: os testes exercitam a implementação real com SQL Server, sem mocks que substituam as garantias transacionais.

## Transações revisadas

- Criação de reserva: reserva e dois avisos na mesma transação SERIALIZABLE, preservando bloqueios por cliente/profissional.
- Serviço e disponibilidade: uma transação e o mesmo bloqueio por profissional utilizado nas reservas; exclusão/desativação preserva referências históricas.
- Confirmar, recusar, cancelar, finalizar e confirmar conclusão: UPDATE condicional e todos os avisos compartilham conexão/transação. Falha antes do commit desfaz atualização e avisos. Estado incompatível resulta em conflito.
- ObterOuCriar de cliente é uma unidade independente e idempotente; uma reserva rejeitada pode manter o cadastro de cliente criado, sem pedido ou notificação parcial. Cadastro do perfil não é tratado como parte do contrato da reserva.

Os testes de falha das cinco transições inserem primeiro um aviso válido e depois um aviso com FK inválida: comprovam rollback do status e dos avisos, incluindo DataCancelamento e valores financeiros. Os testes existentes de serviço provocam FK inválida e verificam serviço/regras preservados; não simulam queda de rede após cada inserção de disponibilidade.

## Warnings antes e depois

Aplicação: 205 na Sprint 2 → 124 no rebuild inicial desta retomada → 120 após quatro correções nos DAOs. Redução total: 85, sem supressão de warnings ou uso indiscriminado de !. Os DAOs prioritários, Controllers migrados e PerfilPublico não apresentam warnings na compilação final da aplicação.

A mudança de BuscarPorId para retorno anulável expôs cinco warnings nos testes; eles foram tratados por validações explícitas e comparação anulável que falha quando o registro não existe. Build final dos testes: zero warnings. Os 120 restantes da aplicação estão catalogados em warnings.csv; não representam build livre de problemas de nulabilidade no restante legado.

## Testes e reprodução

| Grupo | Aprovadas |
|---|---:|
| Sprint 1 | 66 |
| Sprint 2 | 25 |
| Sprint 3 | 34 |
| Total | 125 |
| Falhas | 0 |

Na raiz: `powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint3`.

Bypass é limitado ao processo e não altera a política do Windows. O runner compila em saída isolada, inicia a aplicação para testes HTTP e usa SQL Server local configurado. Cria registros com identificador único e remove os registros da execução ao final; IDs identity podem avançar. Build da aplicação e dos testes sem erros.

## Banco e pendências

Nenhuma alteração estrutural, nenhum DROP/recriação e nenhum script de migração. HISTORICO.md contém somente a decisão e a estratégia incremental de snapshot, com compatibilidade explícita para legado.

Pendências: executar checklist visual; implementar snapshot em evolução posterior; continuar nulabilidade do legado. Erros inesperados de infraestrutura SQL continuam exceções técnicas, não mensagens de validação. A revalidação SQL de serviço ainda usa THROW 50001 para corrida de local/serviço e timeout do bloqueio; seu mapeamento refinado por código deve ser tratado antes de publicar uma API, sem interpretar texto. Não há garantia de cobertura de todas as corridas concorrentes ou falhas de rede.

A limitação de histórico inclui preço fixo usado na finalização a partir da oferta atual. Pagamentos continuam simulados e arquivos bin/obj continuam versionados, como nas Sprints anteriores.

## Impacto no PIM IV e próxima etapa

Programação Aplicada em .NET: DI, contratos explícitos, exceções tipadas e testes de integração. Arquitetura de Software: separação incremental entre apresentação, aplicação e persistência. Desenvolvimento Web: manutenção de rotas, sessão e antiforgery no MVC. Banco de Dados: atomicidade de transições e decisões documentadas de preservação histórica. Integração Web/Mobile: casos de uso compartilháveis sem dependência da sessão MVC.

Próxima etapa recomendada: Sprint 4 — REST API, reutilizando os Services e definindo autenticação e respostas HTTP próprias, após revisar as pendências registradas. Essa etapa não foi iniciada.
