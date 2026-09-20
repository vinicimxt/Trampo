# Testes — Sprint 4

## Resultado final em 20/09/2026

| Grupo | Aprovadas |
|---|---:|
| Sprint 1 | 66 |
| Sprint 2 | 25 |
| Sprint 3 | 34 |
| Sprint 4 | 93 |
| Total | 218 |
| Falhas na execução final | 0 |

Evidência: testes-final.log. O runner mostra explicitamente a baseline de 125 antes das verificações novas. A limpeza final dos registros temporários foi confirmada no log. Nenhuma verificação anterior foi removida ou enfraquecida.

Build completo da aplicação: 120 warnings, zero erros (build-final.log). Build dos testes: zero warnings/erros. A última execução do runner reutilizou a compilação da aplicação e exibiu zero warnings por ser incremental: isso não significa ausência dos 120 avisos, que foram medidos no rebuild e catalogados em warnings.csv.

## Reprodução

Pré-requisitos: SDK .NET 10, pacotes NuGet restaurados, SQL Server configurado por conexao.cs e esquema existente com categoria/subcategoria. Usar ambiente de desenvolvimento/teste: a suíte escreve e remove dados temporários.

Na raiz do repositório:

```powershell
dotnet restore BD-TRAMPO.csproj
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint4
```

Bypass vale somente para esse processo. Sem -Sprint4 continuam disponíveis as opções anteriores. A suíte inicia seu processo local em http://127.0.0.1:5177; a porta deve estar livre. A chave é gerada aleatoriamente e colocada apenas no ambiente do processo filho. O teste final reinicia o mesmo binário em Production sem chave. Nenhum token ou chave é impresso.

Fixtures usam GUID e contas @example.invalid. Cleanup remove apenas IDs registrados pela execução, em ordem de dependência. Sequências identity podem avançar. Não executa DDL ou DROP. A falha por bloqueio mantém uma transação temporária por aproximadamente dez segundos e a desfaz; não executar contra dados reais.

## Cobertura nova

- Login correto, senha incorreta, usuário inexistente e migração SHA-256 real para hash moderno pelo caminho da API.
- Token emitido com claims mínimas/expiração; token ausente, malformado, expirado, issuer/audience/assinatura incorretos; perfil alterado invalida o token.
- /me sem informações internas; sessão MVC não autentica API; Bearer não cria sessão MVC; admin não se torna participante.
- Catálogo paginado, consulta, inexistente/inativo, criação por profissional, cliente recusado, edição própria/alheia, remoção própria/alheia.
- Serviço removido sem histórico é excluído; com histórico é desativado, com resposta que reflete a persistência.
- Rejeição de IDs de proprietário, status e preço adicionais no JSON. Profissional de avaliação é derivado pelo Service.
- Agenda serializada, indisponibilidade, inatividade, consulta de próprios/recebidos, participante autorizado e acesso alheio recusado.
- Duas requisições simultâneas para o mesmo serviço/profissional/horário: uma 201, outra 409 e exatamente uma linha no banco. Location identifica o recurso criado.
- Cancelamento, confirmação, recusa, finalização, conclusão; recusa de confirmado, finalização futura e ações de outros usuários recusadas.
- Avaliação válida, antes da conclusão, por outro cliente e duplicada.
- Valor final persistido, IDs coerentes na lista e marcador explícito de ausência de snapshot da oferta.
- JSON malformado, formato não JSON, rota inexistente e erros com envelope consistente.
- Erros SQL 51001/51002/51003 mapeados por número; SQL desconhecido permanece exceção técnica. Além da tradução isolada, o DAO é exercitado com serviço ausente, local inválido e timeout real de sp_getapplock.
- Middleware de erro exercitado com falha técnica para comprovar resposta 500 genérica, sem o detalhe interno injetado.
- Documento OpenAPI HTTP com rotas V1, schemas, 201 e segurança Bearer; página Swagger UI retorna sucesso em Development; ambos retornam 404 em Production.
- CORS não libera origem arbitrária. Rate limiting retorna 429/Retry-After sem bloquear login MVC.
- Sem chave JWT, MVC inicia, login API retorna 503 e um token anterior não autentica.

## Limites e ocorrências durante implementação

A suíte é de integração local e usa SQL Server real. Não representa teste de carga, auditoria de segurança completa, teste distribuído de rate limiting ou cobertura de todas as falhas de rede. A página Swagger foi validada por HTTP e seu documento inspecionado automaticamente; não se afirma inspeção visual no navegador. O checklist visual da Sprint 3 continua pendente.

Durante a construção dos testes foram corrigidos um conflito de nome local C# e uma colisão entre parâmetro @R e variável @r no teste SQL (comparação sem distinção de maiúsculas no servidor). São falhas resolvidas da implementação do teste; não foram contadas como testes aprovados. As 218 verificações passaram na execução final.
