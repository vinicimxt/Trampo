# Sprint 6 — Arquitetura Mobile

O projeto Android existente está em `C:\Users\Vinicius\AndroidStudioProjects\TRAMPO`, com namespace e `applicationId` `com.example.trampo`. A implementação usa Kotlin, Views/XML, minSdk 24 e compile/target 37. Foram mantidos Gradle 9.6.0, AGP 9.4.1 e Kotlin integrado ao AGP original.

## Caminho das operações

Activity → ViewModel → Repository → Retrofit/TrampoApi → API ASP.NET Core → Services → DAO → SQL Server.

`TrampoApplication` monta dependências manualmente. ViewModels usam `viewModelScope` e LiveData com Idle/Loading/Success/Error. Activities observam estado e não acessam HTTP nem calculam disponibilidade. O Android não possui driver/acesso SQL.

| Grupo | Responsabilidade |
|---|---|
| data/model | DTOs dos JSON reais, decimais e datas ISO |
| data/api | Retrofit, configuração única e interceptor Bearer |
| data/repository | Autenticação, serviços, agendamentos e endereço |
| data/SessionManager | Armazenamento privado apenas do access token |
| ui/login e ui/home | Entrada, validação de sessão, navegação e logout |
| ui/servicos | Catálogo paginado, RecyclerView e detalhes |
| ui/agendamentos | Agenda, endereço, revisão, criação, lista, detalhes e cancelamento |
| res/layout e res/values | Telas, itens, formulário, textos, cores e estilos |

O launcher mostra validação de sessão. Sem token abre login; com token consulta `/auth/me`. 401 limpa sessão e pede autenticação. Falha de conexão não aparece como senha incorreta nem apaga token existente. Contas profissional/admin são orientadas ao Web nesta V1.

## Contratos e fluxos

- Login envia e-mail/senha, salva JWT e consulta `/auth/me`.
- Catálogo apresenta apenas campos da API; não inventa nome de profissional/subcategoria, endereço do local ou link.
- Agenda envia `data=yyyy-MM-dd` e usa horários `HH:mm:ss` retornados.
- Reserva envia somente `servicoId`, `data`, `hora`, `descricao` e `endereco` opcionais. Não envia responsáveis, status, taxa, valor final ou coordenadas.
- Endereço aparece somente em `Domicilio`. CEP usa a API TRAMPO e permite fallback manual. `Local` e `Online` não pedem endereço do cliente.
- Revisão mostra oferta atual, data, horário e endereço. O preço exibido não cria snapshot financeiro, limitação existente do backend.
- Após 201, mostra “Agendamento solicitado com sucesso.”, pois o estado inicial é Pendente.
- A lista usa status real. Como o DTO não contém nome da oferta/modalidade, apresenta IDs reais em vez de reconstruir histórico com oferta mutável.
- Cancelamento pede confirmação, faz POST e reconsulta o servidor.
- `/enderecos/pesquisar` não recebeu interface nesta V1; o fluxo principal usa CEP.

## Falhas e ciclo de vida

400, 401, 403, 404, 409, 429, 5xx, timeout, conexão e resposta inválida têm mensagens próprias. JSON bruto, stack trace e mensagens internas não aparecem.

Botões/ViewModels impedem cliques simultâneos. Falha técnica após POST é resultado incerto: bloqueia novo envio e orienta consultar Meus agendamentos. Sem chave de idempotência, não há garantia de uma única criação após morte do processo/reabertura.

Rascunho e revisão ficam no ViewModel durante rotação normal, sem preferências. Morte do processo descarta o rascunho. Senha não é salva, é limpa ao enviar e nunca registrada. `CancellationException` não vira erro de rede.

Referências: [ViewModel](https://developer.android.com/topic/libraries/architecture/viewmodel), [coroutines](https://developer.android.com/topic/libraries/architecture/coroutines), [Retrofit](https://square.github.io/retrofit/) e [Robolectric](https://robolectric.org/getting-started/).
