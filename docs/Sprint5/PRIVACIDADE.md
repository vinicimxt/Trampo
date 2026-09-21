# Privacidade — Sprint 5

| Informação | Acesso implementado |
|---|---|
| Catálogo público de serviços | DTO da Sprint 4, sem endereço completo, link de reunião ou coordenadas |
| Resposta de CEP/pesquisa | Usuário autenticado; CEP/logradouro/bairro/cidade/UF, sem resposta bruta do fornecedor |
| Local profissional completo | Profissional proprietário nos endpoints /api/v1/locais e cadastro MVC |
| Snapshot de agendamento | Participantes autorizados; terceiros recebem 403 |
| Coordenadas do snapshot | Não expostas pelo DTO de agendamento |
| Domicílio futuro do cliente | Não publicado no catálogo; não enviado ao geocoder |

A API de agendamento disponibiliza o endereço aos participantes autorizados, inclusive antes da confirmação. A View Recebidos mantém a regra visual anterior de só mostrar o endereço domiciliar quando confirmado. Essa diferença não concede acesso a terceiros, mas deve ser alinhada caso o produto queira restringir também o participante antes de confirmar.

ViaCEP recebe apenas CEP ou UF/cidade/logradouro para pesquisa. Número e complemento do domicílio não são enviados. A consulta é assistiva e opcional: é possível salvar manualmente quando o fornecedor falhar.

IHttpClientFactory está configurado sem os loggers de requisição que registrariam a URL do fornecedor. Logs próprios registram apenas categoria de falha, sem endereço, coordenadas, JWT, headers ou segredo. Não habilitar indiscriminadamente logs HTTP detalhados nem logging de bodies/query strings na implantação. O cache de memória é limitado a 500 entradas, com CEP por 24 horas e pesquisas por 15 minutos; não armazena número/complemento residencial ou token.

Endereço é dado de contratação: o snapshot não muda quando outro endereço é usado em uma nova reserva. Sua retenção acompanha o agendamento. Não foi criada política de retenção legal ou exclusão de contas nesta Sprint; isso exige definição específica do produto. A relação em cascata remove snapshot apenas quando sua reserva é excluída.

Sem geocodificação configurada, sem coleta de localização do navegador e sem segredos novos. As regras JWT, sessão, propriedade e antiforgery das Sprints anteriores permanecem.
