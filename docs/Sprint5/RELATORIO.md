# Relatório — Sprint 5: Endereços e Geolocalização

## Resultado

Endereços estruturados implementados para locais profissionais e reservas físicas, preservando textos legados. Migração incremental aplicada e reexecutada no banco Xamou, sem recriar tabelas/banco. Integração ViaCEP isolada, cache, timeout, fallback manual e sete endpoints novos: API passa de 17 para 24 operações.

Regressão final: 288 aprovados (218 anteriores + 70 novos), zero falhas. Build completo: 109 warnings, zero erros; eram 120 na Sprint 4. A validação visual permanece pendente e tem checklist. Geocoder real, proximidade e coleta de localização do navegador foram avaliados e adiados; coordenadas permanecem NULL sem fornecedor confiável.

## Arquivos criados

| Grupo | Arquivos |
|---|---|
| Banco | ScriptsSQL/001_enderecos_geolocalizacao.sql |
| Contratos | Contracts/Enderecos.cs |
| Integração | Integrations/Enderecos/Providers.cs, ViaCepClient.cs, CacheEnderecos.cs |
| Aplicação | Services/EnderecoService.cs, LocalService.cs |
| Persistência | DAO/EnderecoSql.cs |
| Configuração/erros HTTP | Api/ConfiguracaoEnderecos.cs, FalhasEnderecoHttp.cs |
| API | Controllers/Api/EnderecosApiController.cs, LocaisApiController.cs |
| MVC | Controllers/EnderecoController.cs |
| Formulários | Views/Shared/_EnderecoCampos.cshtml, wwwroot/js/enderecos.js, wwwroot/css/enderecos.css |
| Testes | Tests/Sprint1/EnderecoFakes.cs |
| Documentação | docs/Sprint5/RELATORIO.md, ENDERECOS.md, GEOLOCALIZACAO.md, MIGRACAO-BANCO.md, PRIVACIDADE.md, TESTES.md, CHECKLIST-MANUAL.md, inventário, warnings.csv e logs |

## Arquivos alterados

| Arquivo | Alteração |
|---|---|
| Models/Local.cs | Componentes em DadosEndereco e inicialização explícita |
| Models/Agendamento.cs | EnderecoAtendimento para snapshot |
| DAO/LocalDAO.cs | Persistência estruturada, leitura legada e proteção transacional do histórico |
| DAO/AgendamentoDAO.cs | Snapshot na transação da reserva e leitura do endereço/modalidade históricos |
| Services/AgendamentoService.cs | Validação do contrato estruturado de domicílio, mantendo entrada antiga |
| Contracts/Solicitacoes.cs | Endereco opcional no request de reserva |
| Controllers/LocalController.cs | Delega cadastro e propriedade ao LocalService |
| Api/Contracts/HttpContracts.cs | Snapshot de endereço no DTO do participante, sem coordenadas |
| Api/ErrosApiMiddleware.cs | Mapeia falhas de integração por tipo |
| Program.cs | Registra endereço/cache/HttpClient/provider e rate limiting |
| Views/Local/Lista.cshtml | Formulário assistido, modo legado e edição dos componentes |
| Views/Agendamento/Novo.cshtml | Endereço estruturado apenas para Domicilio |
| wwwroot/js/agendamento.js | Seletores do formulário atualizado |
| Tests/Sprint1/Program.cs e Run.ps1 | 70 verificações novas e opção -Sprint5 |
| README.md | Migração, documentação e comando de teste |

Artefatos de build em bin/obj/Tests/Sprint1/app foram atualizados automaticamente, como nas Sprints anteriores; não são alterações manuais de funcionalidade. O whitespace apontado pelo Git em obj/Debug/net10.0/BD-TRAMPO.GeneratedMSBuildEditorConfig.editorconfig é gerado pelo MSBuild. Nenhum commit foi criado nesta retomada. conexao.cs e ScriptBD.txt não foram modificados para aplicar a migração.

## Banco e execução

Locais recebeu CEP, Logradouro, Numero, Complemento, Bairro, Cidade, UF, Latitude, Longitude e EnderecoEstruturado. Componentes/coordenadas são anuláveis; EnderecoEstruturado é BIT obrigatório com default 0. Endereco original permanece.

AgendamentoEnderecos é nova tabela 1:1 opcional com Agendamentos, contendo os componentes, coordenadas opcionais, EnderecoFormatado, EnderecoEstruturado e Modalidade. PK/FK AgendamentoId e checks de modalidade/coordenadas/estrutura preservam integridade. A tabela completa de campos, tipos, nulabilidade e finalidade está em MIGRACAO-BANCO.md.

Script ScriptsSQL/001_enderecos_geolocalizacao.sql executado com sucesso e testado por reexecução. Contagens e checksums dos textos legados comparados não mudaram. Sem DROP, parsing frágil de textos ou geocodificação em massa. Snapshot/reserva/notificações compartilham a transação. O endereço Local é copiado sob bloqueio SQL; domicílio não referencia perfil mutável; Online não cria snapshot.

Nenhum índice de busca geográfica foi adicionado sem consulta correspondente. A PK atende a leitura do snapshot. MER/modelo lógico/físico e composição do script completo foram documentados para atualização dos artefatos do PIM IV.

## Integrações externas

| Provider | Finalidade | Autenticação | Timeout | Cache | Limitações |
|---|---|---|---|---|---|
| ViaCEP | CEP e pesquisa UF/cidade/logradouro | Sem chave | 4 segundos | CEP 24h; pesquisa 15min; até 500 entradas | Saída até 20 resultados; sem retry; sem coordenadas |
| GeocodificacaoNaoConfigurada | Implementação neutra de IGeocodificacaoProvider | Nenhuma | Espera máxima da aplicação 4s para futuro provider | Nenhum | Retorna NULL; não habilita distância |

ViaCEP real respondeu às duas consultas opcionais com dados públicos; regressão usa fornecedor simulado. Sem credenciais ou tokens gravados. URL externa fixa em produção, sem repassar URL arbitrária recebida de usuário. Logs de integração não incluem endereços completos/headers ou segredos.

## Endpoints adicionados

| Método | Endpoint | Auth | Finalidade |
|---|---|---|---|
| GET | /api/v1/enderecos/cep/{cep} | JWT | Consulta CEP |
| GET | /api/v1/enderecos/pesquisar | JWT | Pesquisa UF/cidade/logradouro |
| GET | /api/v1/locais | JWT profissional | Lista próprios locais |
| GET | /api/v1/locais/{id} | JWT proprietário | Detalhes privados |
| POST | /api/v1/locais | JWT profissional | Criação estruturada |
| PUT | /api/v1/locais/{id} | JWT proprietário | Edição sem violar histórico |
| DELETE | /api/v1/locais/{id} | JWT proprietário | Remoção sem vínculos |

Detalhes de requests/status em ENDERECOS.md e OpenAPI. Consultas de endereço limitadas a 30/minuto/IP, compartilhadas com os endpoints MVC de consulta. Sem JWT: 401; perfil/propriedade: 403; validação: 400; conflito histórico: 409. Falhas de fornecedor distinguem inexistência, timeout, indisponibilidade e resposta inválida sem expor exceptions.

## Fluxos Web e privacidade

Local/Lista e Agendamento/Novo usam formulário comum: CEP, consulta, pesquisa, edição manual, número/complemento, mensagem acessível, loading, seleção por teclado e resumo. Local legado mantém texto original até confirmação manual dos componentes. Online não exige endereço físico. A busca textual existente não foi substituída por distância.

Consultas MVC usam sessão e EnderecoService diretamente. API usa JWT e os mesmos Services. Endereço completo e coordenadas não são publicados no catálogo. Local completo pertence ao profissional proprietário; snapshot só é acessível aos participantes e omite coordenadas no DTO. PRIVACIDADE.md registra o alcance e a diferença de exibição antes da confirmação entre API e View Recebidos.

## Testes

| Grupo | Aprovados |
|---|---:|
| Sprint 1 | 66 |
| Sprint 2 | 25 |
| Sprint 3 | 34 |
| Sprint 4 | 93 |
| Sprint 5 | 70 |
| TOTAL | 288 |
| FALHAS | 0 |

Executar `powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint5` após a migração. Cobertura e limites detalhados em TESTES.md. Falha na gravação do snapshot comprovadamente não deixa reserva parcial. Provider real foi verificado separadamente, sem dependência externa na suíte.

## Warnings

120 antes → 109 depois, redução de 11, sem supressão. Testes: zero warnings. Inventário em warnings.csv e rebuild em build-final.log.

| Código | Quantidade |
|---|---:|
| CS8601 | 37 |
| CS8618 | 40 |
| CS8604 | 17 |
| CS8603 | 9 |
| CS8600 | 6 |

## Pendências e impacto no PIM IV

Inspeção visual/responsiva e leitor de tela permanecem pendentes; a automação falhou ao iniciar. Usar CHECKLIST-MANUAL.md. Geocoder real, coordenadas confiáveis, busca por proximidade, mapas e botão de localização não foram implementados. Snapshot de preço/nome da oferta continua pendente das Sprints anteriores; esta evolução cobre endereço. Permanecem limites de revogação JWT, rate limiting por processo, warnings legados e bin/obj versionados.

PIM IV: evolução física/lógica de banco com migração e integridade; orientação a objetos e DI em Services/providers; desenvolvimento Web com preenchimento assistido e acessibilidade; APIs REST com contratos próprios; preparação Mobile sem dependência direta de fornecedores; privacidade e minimização de localização. A próxima evolução deve ser definida a partir dessas pendências; nenhuma Sprint seguinte foi iniciada automaticamente.
