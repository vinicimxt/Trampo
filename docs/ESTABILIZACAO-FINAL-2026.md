# Estabilização final — Web, API, Android e Desktop

> Registro parcial de 07/11/2026, produzido antes da continuação do plano. O fluxo público não vazio, os smokes adicionais e a auditoria de dependências foram executados depois; o estado final está em [HOMOLOGACAO-FINAL-2026-V2.md](HOMOLOGACAO-FINAL-2026-V2.md).

Data: 07/11/2026. Continuação de [HOMOLOGACAO-FINAL-2026.md](HOMOLOGACAO-FINAL-2026.md). O arquivo do pedido desta sprint recebido no ambiente termina no meio da seção 18; este relatório cobre os requisitos recebidos até esse ponto.

## Resultado

| Item | Resultado | Evidência e limite |
| --- | --- | --- |
| HF-01: avaliações públicas | Corrigido na API | `GET /api/v1/servicos/{id}/avaliacoes`: 200 para serviço ativo `215`, 404 para ID inexistente; resumo e lista consistentes. Não havia avaliação vinculada a serviço **ativo** na base de teste, então uma lista pública não vazia não foi executada. |
| HF-01: avaliações recebidas | Corrigido na API | `GET /api/v1/profissionais/me/avaliacoes`: 200 com avaliação real e nome do serviço para Profissional, 403 para Cliente, 401 sem JWT. O profissional é obtido da identidade autenticada. |
| Avaliação duplicada | Preservada | Nova tentativa no agendamento de teste já avaliado `309` retornou 409; nenhuma avaliação nova foi criada. |
| HF-02: notificações | Corrigido na API | GET 200 nos dois perfis e 401 sem JWT. Listas de IDs não se misturam. Profissional marcou notificação própria como lida (204); tentativa de marcar notificação do Cliente retornou 404. |
| HF-02: suporte | Corrigido na API | GET 200 nos dois perfis e 401 sem JWT. Chamados próprios isolados. Envio pelo Profissional retornou 204 e o chamado `9` apareceu apenas na própria lista. |
| Perfil profissional | Corrigido na API | GET 200 para Cliente e Profissional, 401 sem JWT; PUT idempotente com os dados atuais retornou 204 para ambos. A rota de senha usa o mesmo `ContaService` do Web; nenhuma senha foi alterada nesta sprint. |
| Android | Build e contratos aprovados; visual pendente | Retrofit/DTO e telas existentes já correspondiam aos endpoints liberados; nenhuma alteração funcional Android foi necessária. Emulador sem aceleração. |
| Web | Smoke HTTP aprovado; visual pendente | Página inicial e login 200. Sessão Cliente acessou perfil público de profissional (200) e o HTML continha o comentário `[PIM-TESTE]` já registrado. |
| Desktop | Build, runner e smoke aprovados; visual pendente | Build Debug/Release sem erros ou avisos; 124 verificações não visuais; smoke real do Profissional carregou 5 agendamentos, 3 serviços e 2 locais. |

## Alterações

- `AvaliacaoConsultaService` reutiliza `ServicoService` e `AvaliacaoDAO`, sem aceitar `profissionalId` de propriedade vindo do cliente. O DAO inclui nome do serviço e filtra por serviço na consulta pública. Serviço inativo continua fora da rota pública; avaliações históricas continuam visíveis ao seu Profissional.
- Dois endpoints REST retornam `media`, `total` e `avaliacoes` com nota, comentário, nome do Cliente já exibido no perfil público Web, data e nome do serviço quando disponível. Não expõem email, telefone ou outros dados privados.
- Autorização de notificações, suporte e perfil passa a aceitar Cliente e Profissional. Os Services/DAOs existentes continuam derivando o usuário do JWT para consultas e mutações.
- `Tests/stabilization-contract` é um runner HTTP reproduzível com fixtures e credenciais fornecidas por variáveis de ambiente. Verifica status, formato, regressão Cliente e separação de dados entre contas.
- README e documentação da API/matriz foram atualizados. O README Android registra a liberação dos contratos e a limitação visual.

## Execução e contagens

| Verificação | Resultado |
| --- | --- |
| Backend/Web `dotnet build` Release e Debug com saída isolada | 0 erros, 0 avisos em ambos |
| Runner `Tests/stabilization-contract` contra API local `http://127.0.0.1:5166` | 32 passaram, 0 ignorados, 0 falharam |
| Android `gradlew.bat testDebugUnitTest lintDebug assembleDebug` | Build concluído; 36 testes encontrados, 35 passaram, 1 ignorado, 0 falharam; lint 0 erros, 21 avisos |
| Desktop `dotnet build` Release e Debug | 0 erros, 0 avisos em ambos |
| Runner Desktop Release | 124 verificações passaram |
| Desktop `--smoke-final` com API local | Login, `auth/me`, agendamentos, serviços, locais e limpeza do token passaram |
| Web smoke HTTP com sessão Cliente | Página inicial, login e perfil público aprovados |

Para repetir o runner novo, iniciar a API local com a base de teste e definir `TRAMPO_TEST_API_URL`, `TRAMPO_TEST_PUBLIC_SERVICE_ID`, `TRAMPO_TEST_CLIENT_EMAIL`, `TRAMPO_TEST_CLIENT_PASSWORD`, `TRAMPO_TEST_PRO_EMAIL` e `TRAMPO_TEST_PRO_PASSWORD` no processo. `TRAMPO_TEST_REVIEWED_BOOKING_ID` é opcional para a verificação 409 em uma reserva já avaliada que pertença ao Cliente. Depois executar:

```powershell
dotnet run --project Tests/stabilization-contract/stabilization-contract.csproj -c Release
```

Não armazenar credenciais ou JWT no repositório. O runner não os imprime.

## Limites da homologação

- A consulta pública não vazia não pôde ser exercitada com dados existentes: a avaliação da fixture `72` pertence ao serviço `288`, que está inativo. Reativá-lo afetaria o catálogo; isso não foi feito. A consulta profissional não vazia, que usa o mesmo mapeamento de avaliação, foi validada.
- O `emulator -accel-check` informou código 6 e ausência do Android Emulator Hypervisor Driver. O processador reportou `VirtualizationFirmwareEnabled=False`; há apenas o AVD x86 `Pixel_6`. A consulta ao estado de Windows Hypervisor Platform exigiu elevação e não retornou estado. Não foram alterados BIOS ou recursos do Windows.
- A ferramenta de controle visual encerrou o processo ao iniciar em duas tentativas. Nenhuma tela Android, Web ou Windows Forms foi homologada visualmente nesta execução. Os checks de layout do runner Desktop são não visuais.
- A troca de senha não foi executada, conforme proibição do pedido. O perfil foi testado sem modificar os valores. O fluxo de suporte do Profissional criou a fixture etiquetada `[PIM-TESTE]` de ID `9`; ela permanece Aberta porque não há exclusão segura pela API. A fixture `8` e as demais do relatório anterior também permanecem.
- Nenhum commit, push, merge, tag ou deploy foi feito. As alterações locais preexistentes dos três repositórios foram preservadas.

## Estado de entrega

Os bloqueios REST HF-01 e HF-02 estão resolvidos e passaram nos testes de contrato. A validação visual dos três clientes e uma avaliação pública não vazia ainda exigem ambiente/dados apropriados antes de declarar homologação visual completa.
