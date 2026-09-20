# Relatório — Sprint 4: REST API V1

## Resultado em 20/09/2026

Implementados 17 endpoints em /api/v1, reutilizando os Services de Agendamento e Serviço, com autenticação JWT Bearer, autorização por perfil/propriedade, erros consistentes, OpenAPI e Swagger UI em Development. MVC mantém sessão e antiforgery. Regressão: 218 verificações aprovadas (125 anteriores + 93 novas), zero falhas finais. Build completo: 120 warnings legados, zero erros.

Nenhum segredo real foi gravado nos fontes/configurações/documentação. A chave utilizada nos testes foi criada em memória para o processo filho. Nenhuma alteração estrutural no SQL Server. Sprint 5 não foi iniciada.

## Arquivos criados nesta Sprint

| Arquivo | Responsabilidade |
|---|---|
| Api/ConfiguracaoApi.cs | JWT, validação de configuração, rate limiting e OpenAPI |
| Api/TokenService.cs | Configuração de chave e emissão de access token |
| Api/UsuarioClaims.cs | ClaimsPrincipal → UsuarioContexto |
| Api/ErrosApiMiddleware.cs | Envelope de erros e proteção de detalhes técnicos |
| Api/Contracts/HttpContracts.cs | Requests/responses próprios do HTTP, sem Models SQL expostos |
| Controllers/Api/ApiControllerBase.cs | Bearer, metadados comuns e dispensa de antiforgery somente na API |
| Controllers/Api/AuthController.cs | Login e identidade atual |
| Controllers/Api/ServicosApiController.cs | Catálogo, operações de serviço e agenda |
| Controllers/Api/AgendamentosApiController.cs | Consulta, reserva e transições por intenção |
| Controllers/Api/AvaliacoesApiController.cs | Avaliação com profissional derivado |
| Services/AutenticacaoService.cs | Reutiliza verificação/migração de senha existente e consulta usuário atual |
| DAO/FalhasSql.cs | Traduz somente números SQL conhecidos em falhas de aplicação |
| docs/Sprint4/API.md | Endpoints, contratos, status e limitações |
| docs/Sprint4/AUTENTICACAO.md | Configuração, coexistência, segurança e evolução de tokens |
| docs/Sprint4/TESTES.md | Evidências, reprodução e alcance da validação |
| docs/Sprint4/RELATORIO.md | Este relatório |
| docs/Sprint4/warnings.csv e logs | Evidências de compilação e regressão |

## Arquivos alterados nesta Sprint

| Arquivo | Alteração |
|---|---|
| BD-TRAMPO.csproj | Pacotes JwtBearer/OpenApi 10.0.12 e SwaggerUI 10.2.3, com versões fixadas |
| Program.cs | Registro e pipeline da API; MVC mantém seu tratamento de erro e sessão |
| appsettings.json | Issuer, Audience e ExpirationMinutes; sem chave |
| Services/ServicoService.cs | Consulta pública paginada/individual e resultado real da remoção |
| Services/AgendamentoService.cs | Consulta por visão e avaliação com profissional derivado da reserva |
| Contracts/Solicitacoes.cs | Rejeita membros JSON desconhecidos no contrato de reserva |
| DAO/ServicoDAO.cs | Consulta paginada; códigos SQL exclusivos; resultado de excluir/desativar na mesma transação |
| DAO/AgendamentoDAO.cs | Preenche ServicoId e ValorFinal nas consultas consumidas pelos DTOs |
| Tests/Sprint1/Program.cs | 93 verificações novas e configuração efêmera de JWT |
| Tests/Sprint1/Run.ps1 | Opção -Sprint4 inclui as quatro Sprints |
| README.md | Configuração e uso da API e documentação |

As alterações anteriores da Sprint 3 foram preservadas. Controllers MVC não foram convertidos para JWT. conexao.cs e ScriptBD.txt não foram alterados nesta Sprint. bin/obj e Tests/Sprint1/app contêm saídas de build já presentes/versionadas; novas dependências também geraram artefatos. Nenhum commit foi criado e nenhum artefato anterior foi apagado para limpar o Git.

## Endpoints implementados

| Método | Endpoint | Auth | Perfil | Resultado |
|---|---|---|---|---|
| POST | /api/v1/auth/login | Pública | Conta válida | 200 token |
| GET | /api/v1/auth/me | Bearer | Todos os perfis | 200 identidade segura |
| GET | /api/v1/servicos | Pública | Todos | 200 catálogo ativo paginado |
| GET | /api/v1/servicos/{id} | Pública | Todos | 200 oferta ativa |
| POST | /api/v1/servicos | Bearer | profissional | 201 + Location |
| PUT | /api/v1/servicos/{id} | Bearer | proprietário | 204 |
| DELETE | /api/v1/servicos/{id} | Bearer | proprietário | 200 excluido/desativado |
| GET | /api/v1/servicos/{id}/agenda | Bearer | cliente/profissional | 200 horários disponíveis |
| GET | /api/v1/agendamentos | Bearer | cliente/profissional | 200 meus/recebidos autorizados |
| GET | /api/v1/agendamentos/{id} | Bearer | participante | 200 reserva |
| POST | /api/v1/agendamentos | Bearer | cliente/profissional contratante | 201 + Location |
| POST | /api/v1/agendamentos/{id}/cancelar | Bearer | participante | 204 |
| POST | /api/v1/agendamentos/{id}/confirmar | Bearer | profissional responsável | 204 |
| POST | /api/v1/agendamentos/{id}/recusar | Bearer | profissional responsável | 204 |
| POST | /api/v1/agendamentos/{id}/finalizar | Bearer | profissional responsável | 204 |
| POST | /api/v1/agendamentos/{id}/confirmar-conclusao | Bearer | contratante | 204 |
| POST | /api/v1/agendamentos/{id}/avaliacao | Bearer | contratante | 204 |

Requests, responses, parâmetros e códigos de falha de cada operação estão em API.md e no OpenAPI gerado. Operações de intenção não aceitam substituição arbitrária do status.

## Autenticação e segurança

JWT HS256, chave aleatória externa em Jwt__SigningKey, mínimo de 32 bytes em Base64; validade padrão de 15 minutos. Assinatura/algoritmo, issuer, audience e expiração são verificados pelo JwtBearer padrão do ASP.NET Core. Claims limitadas à identidade/perfil e metadados de token. O usuário persistido é revalidado a cada autenticação Bearer.

Login API reutiliza UsuarioDAO.BuscarLogin/Seguranca, incluindo hash moderno e migração SHA-256. Login MVC continua usando o mesmo mecanismo e sua sessão. A chave não tem valor padrão; ausência impede emissão e autenticação Bearer sem derrubar o MVC.

Perfis são exigidos nos endpoints e propriedade nos Services. IDs de autor/responsável são derivados, não aceitos no payload. DTOs controlam a saída; catálogo não expõe link de reunião. API dispensa antiforgery porque aceita apenas Bearer; MVC mantém o filtro existente. Todas as respostas API incluem no-store.

Rate limiting nativo: dez logins por minuto por IP, sem fila; 429 e Retry-After. CORS não foi liberado, pois não existe consumidor Web externo definido. OpenAPI/Swagger ficam somente em Development e não dispensam JWT nos endpoints protegidos.

FalhaOperacao → 400/401/403/404/409. SQL 51001/51002/51003 recebe mapeamento semântico por número. Falhas desconhecidas → 500 genérico no cliente e log técnico no servidor, sem stack trace/SQL/connection string no JSON. Não foi implementado refresh token; limites de revogação e comprometimento estão em AUTENTICACAO.md.

## Arquitetura

```text
MVC (sessão/Razor) ─┐
                   ├→ Application Services → DAOs → SQL Server
API (JWT/JSON) ─────┘
```

Os Controllers API fazem binding, mapeamento de DTOs e status HTTP. Regras críticas de propriedade, disponibilidade, preço, conclusão, avaliação e preservação de histórico continuam nos Services/DAOs transacionais. Services não recebem ClaimsPrincipal, sessão, HttpContext ou dependências Razor. MVC não chama a própria API via HTTP.

## Testes

| Sprint | Aprovadas |
|---|---:|
| 1 | 66 |
| 2 | 25 |
| 3 | 34 |
| 4 | 93 |
| Total | 218 |
| Falhas finais | 0 |

Comando: `powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint4`.

Concorrência HTTP: uma reserva criada e uma resposta 409 para duas requisições simultâneas ao mesmo horário/profissional. A regressão preserva os testes anteriores de transação/rollback e antiforgery. Os testes novos cobrem JWT inválido/expirado/claims incorretas, perfil/propriedade, payload adulterado, fluxos completos, erros SQL reais, OpenAPI, rate limiting e inicialização sem chave. Detalhes em TESTES.md e testes-final.log.

Swagger UI foi verificado por HTTP, sem inspeção visual de navegador; não substitui o checklist visual pendente da Sprint 3.

## Warnings

Antes: 120 na Sprint 3. Depois: 120 no rebuild da Sprint 4; zero erros. Nenhum warning novo nos arquivos novos de API/Services/FalhasSql, sem supressão. Testes: zero warnings e zero erros.

| Código | Quantidade |
|---|---:|
| CS8601 | 43 |
| CS8618 | 42 |
| CS8604 | 19 |
| CS8603 | 10 |
| CS8600 | 6 |

Inventário em warnings.csv; build-final.log registra o rebuild. Um build incremental sem recompilar pode mostrar zero avisos e não deve ser usado para dizer que a aplicação está livre de warnings.

## Banco

Nenhuma tabela alterada ou criada. Nenhuma migration/script incremental necessário nesta Sprint. Nenhum DROP/recriação. Apenas os dados temporários de teste foram inseridos/alterados/removidos; sequências identity podem avançar.

As transações e bloqueios SQL permanecem. O resultado de DELETE vem da própria transação; sem histórico exclui, com histórico desativa. A API não permite contornar as verificações de concorrência do DAO.

## Pendências reais

- Snapshot da oferta ainda não implementado. A finalização Fixo usa preço atual do serviço; API expõe somente valor final persistido, com marcador de origem, sem apresentá-lo como preço contratado original. Implementar snapshot incremental antes de uso comercial que exija essa garantia.
- Sem refresh token, revogação por dispositivo ou invalidação automática por troca de senha. Access token pode durar até sua expiração; definir evolução antes do Mobile final.
- Rate limiting local ao processo; estratégia distribuída/proxies confiáveis fica para implantação futura.
- Listagem de agendamentos sem paginação e relógio local herdado do MVC; revisar para escala/fusos diferentes.
- 120 warnings legados, validação visual completa pendente, pagamentos simulados e artefatos bin/obj versionados.
- Catálogo público intencionalmente omite link de reunião/endereço; enriquecer detalhes autorizados conforme contrato do futuro cliente, preservando privacidade.

## Impacto no PIM IV

Desenvolvimento Web e APIs REST: contratos JSON, códigos HTTP, rotas versionadas e OpenAPI. Desenvolvimento Mobile futuro: canal Bearer sobre os mesmos casos de uso. Arquitetura de Software e Orientação a Objetos: separação entre apresentação, aplicação e persistência com DI e DTOs. Segurança: autenticação, autorização, propriedade, rate limiting e erros controlados. Banco de Dados: concorrência e atomicidade preservadas e comprovadas por testes de integração.

## Próxima etapa

Sprint 5 — Endereços e Geolocalização, após revisar as pendências relevantes. Não foram iniciados Mobile, mapas, geolocalização, Docker, Cloud, CI/CD, microserviços, EF, Identity ou redesign Web.
