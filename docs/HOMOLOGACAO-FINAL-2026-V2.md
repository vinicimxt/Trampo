# Homologação final V2 — TRAMPO

Execução em **08/11/2026**, ambiente local Windows, SQL Server e API em `127.0.0.1:5166`. Este documento complementa o [relatório histórico](HOMOLOGACAO-FINAL-2026.md) e substitui o [registro parcial da estabilização](ESTABILIZACAO-FINAL-2026.md) como estado atual. O plano completo foi recebido após o registro parcial. Não houve commit, push, merge, tag ou deploy.

## Antes e depois

| Item | Antes | Depois | Evidência |
| --- | --- | --- | --- |
| HF-01: avaliações públicas | Rota 404 | **CORRIGIDO** | 200 com avaliação real, média 5, total 1 e comentário para serviço ativo `291`; 404 para ID inexistente. Após desativação da oferta histórica, volta a 404 conforme regra de catálogo. |
| HF-01: avaliações recebidas | Rota 404 | **CORRIGIDO** | 200 Profissional com avaliação `73` e nome do serviço; 403 Cliente, 401 sem JWT. Identidade profissional derivada do JWT. |
| HF-02: notificações Profissional | 403 | **CORRIGIDO** | GET 200 para os dois perfis; leitura própria 204; leitura cruzada 404; anônimo 401. |
| HF-02: suporte Profissional | 403 | **CORRIGIDO** | GET 200 nos dois perfis; chamado próprio `9` enviado pelo Profissional (204), status e isolamento verificados; anônimo 401. |
| Perfil Profissional Android | API 403 | **OK no contrato; visual PARCIAL** | GET 200 e PUT idempotente 204 para Profissional e Cliente; 401 sem JWT. Tela, Retrofit e DTO já existiam. Senha não foi alterada. |
| Emulador Android | Falhou sem aceleração | **PENDENTE** | AVD x86 `Pixel_6`; `emulator -accel-check` código 6; nenhuma opção ARM ou dispositivo conectado. |
| Web visual | Pendente | **PENDENTE** | Smoke HTTP/HTML aprovado; controle visual indisponível. |
| Desktop visual | Pendente | **PENDENTE** | Runner, smokes e builds aprovados; controle visual indisponível. |

## Fluxo integrado `[PIM-FINAL]`

Fluxo real concluído com as contas de teste já fornecidas. A API local criou o serviço Online `291`, com oferta marcada `[PIM-FINAL]`, que apareceu no catálogo e na agenda do Cliente. O Cliente criou o agendamento `310`; o Profissional o encontrou na lista de recebidos e confirmou; o Cliente consultou o estado **Confirmado** e o nome real do serviço. Após o início do horário reservado, o Profissional finalizou (204), o Cliente viu **AguardandoCliente**, confirmou a conclusão (204), ficou elegível e avaliou com nota 5 (204). A repetição da avaliação retornou **409**.

O Profissional consultou as avaliações recebidas e encontrou o novo comentário com o nome do serviço. Antes da limpeza, o Cliente consultou `GET /servicos/291/avaliacoes` e recebeu lista **não vazia**, média 5 e total 1. O perfil público Web com sessão Cliente retornou 200 e continha a marca `[PIM-FINAL]` no HTML. Os smokes Desktop, executados enquanto a fixture estava ativa, carregaram os agendamentos e serviços do Profissional. Android não participou visualmente deste fluxo.

O serviço `291` foi desativado pela **API** após o fluxo, pois possui histórico. Consultas `SELECT` confirmaram: serviço inativo, agendamento `310` finalizado, exatamente uma avaliação `73` ligada ao serviço e chamado `9` existente. Não houve mutação por SQL. O agendamento, avaliação, notificações associadas e chamado permanecem por integridade/histórico. O smoke Desktop criou e removeu o serviço temporário `292` e o local `97` por suas próprias rotas.

## Regressão executada

| Área | Resultado atual |
| --- | --- |
| Backend restore | Aprovado |
| Backend Debug | 0 erros, **109 avisos de nulabilidade preexistentes** em compilação não incremental |
| Backend Release | 0 erros, **109 avisos de nulabilidade preexistentes** em compilação não incremental |
| Backend `Tests/stabilization-contract` | **33 verificações passaram**, 0 ignoradas, 0 falhas; inclui autorização, ownership, Cliente, duplicidade e contratos de avaliação |
| Backend `Tests/booking-contract` | 2 verificações passaram |
| Backend `Tests/binding-check` | Verificação de binding HTTP passou |
| Android `testDebugUnitTest` | **36 descobertos: 35 passaram, 1 ignorado, 0 falharam** |
| Android `lintDebug` | 0 erros, **21 avisos** |
| Android `assembleDebug` | Build concluído sem erros |
| Desktop restore, build Debug e Release | Aprovados; 0 erros, 0 avisos nos builds |
| Desktop runner Release | **124 verificações passaram** |
| Desktop `--smoke-final` | Login, `auth/me`, agendamentos (6), serviços (4), locais (2) e limpeza do JWT aprovados |
| Desktop `--smoke` | Criar, editar e remover serviço próprio por API aprovados; fixture temporária removida |
| Desktop `--smoke-local` | CEP, criar, editar e remover local próprio por API aprovados; fixture temporária removida |
| Web smoke HTTP/HTML | Home, login com antiforgery, perfil público e comentário de fixture aprovados |

O Android já declarava as rotas e tratava loading, sucesso, lista vazia, 401/403/404, rede e erro inesperado por `UiState`/mensagens. As listas e o detalhe de agendamento priorizam `servicoNome`. Isso foi inspecionado em código e compilado; a aparência e o comportamento na tela **não** foram certificados. A matriz por interface está em [MATRIZ-WEB-API-MOBILE.md](MATRIZ-WEB-API-MOBILE.md#matriz-final-de-execução--08112026).

## Dependências e testes históricos

`dotnet list BD-TRAMPO.csproj package --vulnerable --include-transitive` encontrou `System.Security.Cryptography.Pkcs 9.0.13`, transitivo de `Microsoft.Data.SqlClient 7.0.0`, com avisos **High** e **Moderate**. O backend passou a fixar `Pkcs 9.0.21`, patch da mesma linha. Restore e builds Debug/Release passaram; nova auditoria NuGet **não encontrou pacote vulnerável** nas fontes atuais. O Desktop também não apresentou pacotes vulneráveis na auditoria. Fontes da correção: [aviso High do .NET](https://github.com/dotnet/runtime/security/advisories/GHSA-846f-f764-76mx), [aviso Moderate do .NET](https://github.com/dotnet/runtime/security/advisories/GHSA-2fmv-rv2c-59x7) e [pacote 9.0.21 no NuGet](https://www.nuget.org/packages/System.Security.Cryptography.Pkcs/9.0.21).

Os números das Sprints 1/6/8/9 em [TESTES-FINAIS.md](TESTES-FINAIS.md) são históricos. Seus projetos não estão neste checkout; `Scripts/Test-CI.ps1` ainda os referencia e, por isso, não foi executado nesta rodada. Os três runners presentes foram executados. Nenhum teste fictício foi criado para reproduzir contagens antigas.

## Limites e pendências

- **VALIDAÇÃO VISUAL PENDENTE** para Android Cliente e Profissional, Web Cliente e Profissional, Desktop Cliente e Profissional. O emulador x86 não possui aceleração; `adb devices -l` não listou dispositivo físico e só há o AVD `Pixel_6`. O estado de Windows Hypervisor Platform exigiu elevação e não pôde ser lido. BIOS e recursos do Windows não foram alterados. A ferramenta visual encerrou ao iniciar com erro de sandbox do Windows. HTTP, HTML, testes de componentes e smokes **não equivalem** a inspeção visual.
- Não foram executadas na interface as listas completas das seções 19–21, inclusive VLibras, responsividade, TabIndex, foco, redimensionamento e acessibilidade assistiva. O fluxo cruzado ideal Web → Android → Desktop → Android ficou **parcial**: API real em ambos os papéis, Desktop Profissional por smoke e Web Cliente por HTML; nenhuma interação visual Android.
- A regra de troca de senha foi reutilizada para Profissional, mas não executada, conforme proibição do plano. Nenhuma senha, JWT, email ou connection string foi gravada em arquivos de projeto ou relatórios.
- Os 109 avisos de nulabilidade do backend e os 21 avisos lint Android permanecem. O aviso cosmético de texto de conclusão observado no relatório histórico não foi reproduzido visualmente nesta rodada; permanece como ressalva baixa histórica, não como nova falha.
- `Tests/stabilization-contract` depende de duas contas de teste e uma oferta ativa fornecidas por variáveis de ambiente do processo. `TRAMPO_TEST_REVIEWED_BOOKING_ID` é opcional para repetir o 409. Não há dados de acesso fixados no runner.

## Git e fixtures

O backend já tinha alterações locais e arquivos novos antes desta sprint, listados no [relatório anterior](HOMOLOGACAO-FINAL-2026.md); foram preservados. **Editados nesta estabilização:** `BD-TRAMPO.csproj`, `Program.cs`, `Models/Avaliacao.cs`, `DAO/AvaliacaoDAO.cs`, `Controllers/Api/ServicosApiController.cs`, `Controllers/Api/ClienteAtendimentoApiController.cs`, `Controllers/Api/PerfilApiController.cs`, `README.md`, `docs/API-FINAL.md`, `docs/ARQUITETURA-FINAL.md`, `docs/MATRIZ-WEB-API-MOBILE.md` e `docs/TESTES-FINAIS.md`. **Novos nesta estabilização:** `Api/Contracts/AvaliacaoConsultaContracts.cs`, `Services/AvaliacaoConsultaService.cs`, `Controllers/Api/ProfissionaisApiController.cs`, `Tests/stabilization-contract/` e os dois relatórios de estabilização. Alguns dos arquivos editados já constavam como modificados no Git antes desta execução; nenhuma alteração anterior foi descartada.

**Já modificados/novos antes desta estabilização e não editados nela:** `Api/Contracts/ClienteContracts.cs`, `Api/Contracts/HttpContracts.cs`, `DAO/AgendamentoDAO.cs`, `DAO/ServicoDAO.cs`, `DAO/SuporteDAO.cs`, `Models/Servico.cs`, `Services/ClienteAtendimentoService.cs`, `Services/ServicoService.cs`, `Tests/booking-contract/`, `docs/HOMOLOGACAO-FINAL-2026.md`, os dois relatórios Sprint 13 Desktop e `docs/evidencias-homologacao/`. No Android houve somente alteração de `README.md`; no Desktop, somente de `README.md`. Não houve commit ou push.

`git status --short` final, por repositório:

```text
BACKEND/WEB/API
 M Api/Contracts/ClienteContracts.cs
 M Api/Contracts/HttpContracts.cs
 M BD-TRAMPO.csproj
 M Controllers/Api/ClienteAtendimentoApiController.cs
 M Controllers/Api/PerfilApiController.cs
 M Controllers/Api/ServicosApiController.cs
 M DAO/AgendamentoDAO.cs
 M DAO/AvaliacaoDAO.cs
 M DAO/ServicoDAO.cs
 M DAO/SuporteDAO.cs
 M Models/Avaliacao.cs
 M Models/Servico.cs
 M Program.cs
 M README.md
 M Services/ClienteAtendimentoService.cs
 M Services/ServicoService.cs
 M docs/API-FINAL.md
 M docs/ARQUITETURA-FINAL.md
 M docs/MATRIZ-WEB-API-MOBILE.md
 M docs/TESTES-FINAIS.md
?? Api/Contracts/AvaliacaoConsultaContracts.cs
?? Controllers/Api/ProfissionaisApiController.cs
?? Services/AvaliacaoConsultaService.cs
?? Tests/booking-contract/
?? Tests/stabilization-contract/
?? docs/ESTABILIZACAO-FINAL-2026.md
?? docs/HOMOLOGACAO-FINAL-2026-V2.md
?? docs/HOMOLOGACAO-FINAL-2026.md
?? docs/SPRINT-13-DESKTOP-CLIENTE-AUDITORIA.md
?? docs/SPRINT-13-DESKTOP-MULTIPERFIL.md
?? docs/evidencias-homologacao/

ANDROID
 M README.md

DESKTOP
 M README.md
```

Fixtures históricas restantes: serviço inativo `288`, agendamento `309`, avaliação `72`, suporte `8` e notificações. Desta estabilização: suporte profissional `9` com marca `[PIM-TESTE]`; serviço inativo `291`, agendamento finalizado `310`, avaliação `73` e notificações com marca `[PIM-FINAL]`. Não apagar histórico por SQL. O arquivo local ignorado `artifacts/stabilization/final-flow-fixture.json` guarda apenas IDs/estado, sem credenciais.

## Classificação

**APROVADO COM RESSALVAS** para a entrega acadêmica: HF-01 e HF-02 corrigidos, fluxos centrais comprovados na API/banco, build e regressões aprovados, sem bug crítico ou alto reproduzido. A homologação visual das três interfaces continua pendente por limitação do ambiente, e os avisos não bloqueantes acima permanecem documentados. Contagem nesta execução: **0 críticos, 0 altos, 0 médios reproduzidos; 1 ressalva baixa cosmética histórica não revalidada**.
