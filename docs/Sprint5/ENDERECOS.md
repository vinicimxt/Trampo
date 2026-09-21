# Modelo e decisão anterior à implementação

Revisados Models/Local.cs, Models/Agendamento.cs, LocalDAO, AgendamentoDAO, LocalController, AgendamentoController, AgendamentoService e ScriptBD.txt. Inventário em usos-endereco.txt. Locais.Endereco é NVARCHAR(255) obrigatório; Agendamentos.EnderecoCliente é texto opcional. LocalId vincula serviços/reservas ao local profissional. LocalDAO já impede alterar/excluir endereço usado no histórico, sob bloqueio por profissional.

Views afetadas: Local/Lista cadastra e lista; Servico/Criar e Profissional/MeusServicos selecionam locais; Agendamento/Novo recebe domicílio; Agendamento/Meus e Recebidos exibem o endereço. A busca textual de serviços usa Locais.Endereco. A API controla a saída por DTOs. ProfissionalDAO.AtualizarEndereco é legado separado, não será usado como fonte da contratação.

Decisão: adicionar componentes opcionais e EnderecoEstruturado (default 0) em Locais, preservando Endereco. Criar AgendamentoEnderecos 1:1 com PK/FK AgendamentoId. A tabela será snapshot imutável de endereço, sem referência a perfil mutável. Preferida a ampliar Agendamentos porque somente modalidades físicas precisam desses dados. Capturar Domicilio e também Local; Online não cria snapshot. Para Local, copiar dentro do bloqueio SQL, nunca de uma consulta anterior à transação.

Legado: não decompor textos. Registros anteriores permanecem sem snapshot e com texto original. Novas chamadas do contrato antigo de domicílio continuam aceitas como legado explícito; novos formulários/contratos estruturados são validados integralmente. Campos opcionais não devem ser preenchidos por inferência. EnderecoEstruturado identifica a origem, sem depender de testar se um campo isolado é NULL.

Coordenadas: DECIMAL(9,6), par completo ou ambas NULL, latitude -90..90, longitude -180..180. Requests não contêm coordenadas; somente provider de geocodificação registrado no backend pode fornecê-las, após validar limites. Sem provider, salvar normalmente com NULL. Não geocodificar domicílios nem enviar número/complemento residencial ao ViaCEP.

Histórico: manter a restrição de edição de local já usado, inclusive componentes novos. Não permitir uma edição que altere metadados enquanto conserva o texto formatado. Snapshot+reserva+notificações na mesma transação. A evolução trata endereço, não snapshot de preço/nome da oferta, que continua pendente da Sprint 3.

Índices: PK em AgendamentoEnderecos atende consultas por AgendamentoId e garante 1:1. Sem índice especulativo em CEP/Cidade/UF: nesta Sprint não existem consultas SQL novas filtrando essas colunas. Busca por proximidade e recursos espaciais ficam adiados até haver coordenadas confiáveis.

## Integração e contratos implementados

ViaCepClient usa IHttpClientFactory, base HTTPS fixa, timeout de quatro segundos, limite de resposta de 128 KiB e JSON com profundidade limitada. Não há retry automático. Respostas próprias do TRAMPO contêm somente cep, logradouro, bairro, cidade e uf. Normalização aceita oito dígitos com hífen opcional; UF é validada contra as 27 siglas. Cidade/logradouro de pesquisa exigem pelo menos três caracteres e no máximo 80/100; até 20 resultados são devolvidos.

Cache separado e limitado a 500 entradas: CEP por 24 horas, pesquisa por 15 minutos. Falhas não ficam em cache. Requisições concorrentes ainda podem consultar o mesmo CEP antes de o cache ser preenchido; não há coordenação distribuída. Os limites de consulta reduzem abuso.

A consulta é opcional. CEP geral pode não preencher logradouro/bairro: o usuário completa manualmente. Não é feita validação obrigatória on-line ao salvar; o backend valida formato/campos sem exigir disponibilidade do fornecedor. O texto formatado tem até 255 caracteres para manter compatibilidade.

## Endpoints novos

| Método | Endpoint | Auth | Finalidade |
|---|---|---|---|
| GET | /api/v1/enderecos/cep/{cep} | JWT | Consulta assistida de CEP |
| GET | /api/v1/enderecos/pesquisar?uf=SP&cidade=...&logradouro=... | JWT | Pesquisa assistida |
| GET | /api/v1/locais | JWT, profissional | Listar próprios locais |
| GET | /api/v1/locais/{id} | JWT, proprietário | Detalhar local |
| POST | /api/v1/locais | JWT, profissional | Criar local estruturado; 201 + Location |
| PUT | /api/v1/locais/{id} | JWT, proprietário | Editar local não histórico; 204 |
| DELETE | /api/v1/locais/{id} | JWT, proprietário | Remover local sem vínculos; 204 |

API total: 24 operações. Consultas de endereço usam limite compartilhado de 30 requisições/minuto/IP entre MVC e API, inclusive hits de cache; 429 com Retry-After. Não é proxy público irrestrito. O limite é local ao processo, como o login da Sprint 4.

Erros de integração mantêm o envelope erro/codigo/mensagem: CEP inexistente 404 CEP_NAO_ENCONTRADO; timeout 504 ENDERECO_TIMEOUT; indisponibilidade 503 ENDERECO_INDISPONIVEL; resposta inválida 502 ENDERECO_RESPOSTA_INVALIDA. Parâmetros inválidos geram 400 antes da chamada. Sem detalhes SQL/HTTP internos no cliente.

POST/PUT de local recebem nome e dadosEndereco (cep, logradouro, numero, complemento opcional, bairro, cidade, uf), sem IDs de proprietário ou coordenadas. Requests incompletos são recusados. Conflito histórico retorna 409; propriedade alheia 403.

CriarAgendamentoRequest recebeu endereco opcional com os mesmos componentes. Em Domicilio, um endereco presente é validado integralmente e vira snapshot; o contrato antigo rua/numero/bairro/cidade continua aceito como legado explícito quando endereco está ausente. Online não exige endereço; Local copia o local escolhido sob bloqueio SQL. O DTO de agendamento ganhou enderecoAtendimento, sem coordenadas; reservas antigas podem retornar null e continuam com texto legado.

O MVC consulta /Endereco/Cep e /Endereco/Pesquisar usando sessão e o mesmo EnderecoService. Não há chamadas do MVC à própria API nem chamadas diretas do JavaScript ao ViaCEP. O formulário comum inclui consulta, pesquisa, loading, status aria-live, seleção por botão, edição manual, foco no número e resumo; Local/Lista permite manter texto antigo ou confirmar componentes.

Referência do provedor: [documentação ViaCEP](https://viacep.com.br/). A consulta real do fornecedor é separada da regressão simulada; não se realizou geocodificação em massa.
