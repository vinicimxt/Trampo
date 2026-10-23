# Sprint 12 — Fase 3: auditoria de Serviços Desktop

Data: 11/10/2026. Escopo: auditoria da Fase 3 e extensão mínima da API aprovada na Fase 3A. O projeto WinForms permanece sem alterações nesta fase.

## Matriz da API existente

| Operação | Endpoint | Perfil | Contrato/status | Uso Desktop |
|---|---|---|---|---|
| Catálogo público | `GET /api/v1/servicos?pagina=&tamanho=&busca=&atendimento=` | Anônimo | `200 ServicoResponse[]`; paginação até 100 itens; apenas `Ativo=1`; `400` em filtros/paginação inválidos | Não serve como lista própria: não filtra pelo JWT, omite inativos e os dados de edição. |
| Detalhe público | `GET /api/v1/servicos/{id}` | Anônimo | `200 ServicoResponse` ou `404`; apenas ativos | Pode mostrar dados atuais de um serviço ativo, mas não preenche a edição. |
| Criar | `POST /api/v1/servicos` | Profissional | `ServicoRequest`; `201 {id}` e Location | Reutilizável após obter subcategorias do banco. |
| Editar | `PUT /api/v1/servicos/{id}` | Profissional proprietário | `ServicoRequest`; `204` | Reutilizável após obter todos os valores atuais para preencher o formulário. |
| Remover da oferta | `DELETE /api/v1/servicos/{id}` | Profissional proprietário | `200 {id,resultado}`, onde `resultado` é `desativado` ou `excluido` | Reutilizável; a UI deve exibir o resultado real. |
| Locais próprios | `GET /api/v1/locais` | Profissional | `200 LocalResponse[]` | Já permite escolher um local existente quando `atendimento=Local`; não exige criar a tela Locais. |
| Serviços próprios completos | `GET /api/v1/servicos/me` | Profissional com JWT | `200 ServicoProprioResponse[]`; 401/403 | Lista ativos/inativos e preenche a edição sem SQL no Desktop. |
| Categorias/subcategorias | `GET /api/v1/categorias` | Anônimo | `200 CategoriaResponse[]` | Permite seleção no cadastro sem IDs hardcoded. |

A rota MVC pública `/Servico/GetSubcategorias?categoriaId=` depende de conhecer um ID de categoria; o novo catálogo REST fornece a hierarquia completa ao Desktop.

## Contrato e regras de gravação

`ServicoRequest` aceita somente: `subcategoriaId`, `nome`, `descricao`, `atendimento`, `localId`, `linkOnline`, `tipoPreco`, `precoBase`, `diasSemana`, `horaInicio`, `horaFim`. `profissionalId` não pertence ao corpo; o backend o determina pelo JWT e rejeita propriedades JSON extras.

- `nome`: obrigatório, até 100 caracteres. `descricao`: opcional, até 255.
- `atendimento`: `Local`, `Domicilio` ou `Online`. `Local` exige `localId` pertencente ao profissional. `Online` exige `linkOnline` HTTP/HTTPS válido, até 255 caracteres.
- `tipoPreco`: `Fixo` ou `Combinar`. `Fixo` exige `precoBase` decimal maior que zero e até 99.999.999,99; `Combinar` persiste preço nulo.
- `diasSemana`: texto com dias de 0 a 6 separados por vírgula; `horaInicio` e `horaFim` são `TimeSpan` entre 00:00 e antes de 24:00 e devem diferir.
- A subcategoria deve existir. Na edição, `ServicoService.Salvar` mantém a subcategoria original e ignora `subcategoriaId` enviado; a UI deve tratá-la como não editável enquanto essa regra vigorar.
- `ServicoResponse` público contém ID, profissionalId, subcategoriaId, nome, descrição, atendimento, tipoPreco, precoBase, ativo e nomes de categoria/subcategoria. Não contém `localId`, `linkOnline` nem disponibilidade.
- O DELETE usa transação: havendo agendamento, define `Ativo=0` e retorna `desativado`; sem histórico, exclui o serviço e retorna `excluido`. O histórico de agendamentos é preservado.
- As falhas da API usam 400 validação, 401 autenticação, 403 autorização/propriedade, 404 não encontrado, 409 conflito, 429 limite quando aplicável e 5xx indisponibilidade/erro técnico.

## Extensão aprovada e implementada — Fase 3A

A extensão aprovada adiciona somente duas consultas à API. Nenhum Form Desktop, rota de Locais ou operação de gravação foi alterado nesta fase.

### `GET /api/v1/servicos/me`

- Exige JWT Bearer de usuário com papel `profissional`. Sem token retorna 401; Cliente e Admin retornam 403. A identidade vem exclusivamente do token validado e do usuário persistido. `ServicoService.Profissional` resolve o cadastro profissional; `ServicoDAO.ListarPorProfissional` filtra pelo ID obtido no servidor. A rota não recebe `profissionalId` nem `userId`; qualquer parâmetro de query é rejeitado com 400.
- Retorna `200 ServicoProprioResponse[]`, incluindo serviços ativos e inativos; profissional sem serviços recebe `[]`. Campos em camelCase: `id`, `subcategoriaId`, `nome`, `descricao`, `atendimento`, `localId`, `linkOnline`, `tipoPreco`, `precoBase`, `ativo`, `diasSemana`, `horaInicio`, `horaFim`, `categoria`, `subcategoria`. Os horários seguem a serialização `TimeSpan` do ASP.NET, por exemplo `08:00:00`. Não inclui email, telefone, JWT, dados administrativos ou ID do profissional.
- O DAO existente já busca os dados de edição e a disponibilidade. `diasSemana` é a lista de dias persistidos separada por vírgulas. `horaInicio` é o menor início e `horaFim` o maior fim entre as disponibilidades do serviço. Essa representação é fiel ao contrato de gravação atual, que usa o mesmo intervalo para todos os dias; registros legados com intervalos diferentes por dia perderiam esse detalhe na resposta. Serviço sem disponibilidade retorna dias vazios e horários nulos. `linkOnline` só aparece quando a URL persistida é HTTP/HTTPS válida, conforme o mapeamento existente do DAO.

### `GET /api/v1/categorias`

- Catálogo público e somente leitura: retorna `200 CategoriaResponse[]`, cada categoria com `id`, `nome` e `subcategorias: [{id,nome}]`. Sem categorias, retorna `[]`; uma categoria sem subcategorias recebe `subcategorias: []`. Ordenação por ID. O método POST não existe (405).
- A leitura anônima é coerente com o catálogo de serviços público e com a consulta MVC pública de subcategorias. Os valores vêm de `CategoriaDAO.Listar()` e `SubcategoriaDAO.ListarTodas()`, sem IDs fixos ou tabela nova. O mapeamento do `SubcategoriaDAO` passou a preencher o `CategoriaId` que já existe no modelo e no banco; isso permite agrupar corretamente sem consulta por categoria nem duplicação por join.

### PUT e compatibilidade

`ServicoRequest` contém `subcategoriaId`, mas `ServicoService.Salvar` escolhe `original?.SubcategoriaId ?? d.SubcategoriaId`. Um teste HTTP confirmou que o PUT mantém a subcategoria original mesmo quando recebe outro ID. A regra já estava documentada em `docs/Sprint4/API.md`; a intenção histórica não está comprovada, então permanece como comportamento de compatibilidade. O Desktop deverá mostrar categoria e subcategoria como somente leitura na edição até nova análise e aprovação.

Os contratos existentes `GET /api/v1/servicos`, `GET /api/v1/servicos/{id}`, `POST`, `PUT` e `DELETE` continuam sem alteração. O DTO público `ServicoResponse` não ganhou campos privados. Nenhuma alteração de banco foi necessária.

### Validação

- `dotnet build BD-TRAMPO.csproj` em saída isolada: sucesso, 0 erros. A saída normal estava bloqueada por uma instância já aberta do backend; a instância não foi interrompida.
- `Tests/Sprint1 --unit`: 14 verificações passaram, incluindo catálogo vazio e vínculo de subcategorias.
- `Tests/Sprint1 --sprint4`: 247 verificações passaram, incluindo regressão Web/API das Sprints 1–4 e novos cenários de autenticação, perfis, isolamento entre profissionais, ativos/inativos, DTO de edição, profissional sem serviços, catálogo real, unicidade, somente leitura e preservação da subcategoria no PUT. Os registros temporários do teste foram removidos.
