# Relatório — Sprint 6: Aplicação Mobile TRAMPO

## Resultado

A primeira versão Android nativa do TRAMPO foi implementada no projeto existente `C:\Users\Vinicius\AndroidStudioProjects\TRAMPO`, usando Kotlin e Views/XML. O fluxo cliente cobre validação de sessão, login, Home, catálogo paginado, detalhes, agenda da API, endereço domiciliar por CEP/fallback manual, revisão, criação, lista, detalhes e cancelamento.

O APK debug foi gerado. Testes locais: 23 aprovados, zero falhas; Lint: zero erros e 10 avisos. A integração do cliente Kotlin com a API e o SQL Server reais passou em seis verificações: três modalidades foram reservadas, snapshots físicos confirmados, cancelamento persistido e fixtures removidas. A validação visual em dispositivo permanece pendente porque não havia AVD, imagem de sistema ou aparelho conectado.

## Android criado/alterado

| Grupo | Resultado |
|---|---|
| Gradle | Retrofit 3/Gson, OkHttp, RecyclerView, Lifecycle, coroutines, MockWebServer e Robolectric no catálogo de versões |
| Rede | URL única; debug `10.0.2.2:5165`; release exige HTTPS configurada; cleartext apenas no debug/emulador |
| Sessão | JWT privado, Bearer automático, limpeza seletiva em 401, logout local, sem refresh |
| Modelos/API | DTOs compatíveis com contratos reais e endpoints necessários em `TrampoApi` |
| Arquitetura | Activity → ViewModel → Repository → Retrofit, com injeção manual em `TrampoApplication` |
| Login/Home | Campos, loading, erros, `/auth/me`, perfil cliente, navegação e logout |
| Serviços | RecyclerView, paginação de 20, detalhes e preços/modalidades reais |
| Reserva | Data, horários da API, endereço apenas em Domicilio, CEP via TRAMPO, revisão e POST restrito |
| Agendamentos | Lista, detalhes autorizados, status da API e cancelamento confirmado/reconsultado |
| UI | Layouts XML, cards XML, tema claro/escuro, textos centralizados, alvos de toque e mensagens acessíveis |
| Testes | Contratos HTTP, sessão, erros, duplicidade, layouts e integração API/SQL |

Não foram adicionados Compose, Hilt/Dagger, Firebase, GPS, mapa, push, pagamento, chat, refresh token ou acesso SQL no Android.

## Contratos e decisões

O app não apresenta campos inexistentes. Serviço exibe nome, descrição, modalidade, preço e identificadores reais; a API não oferece nome do profissional/subcategoria nem endereço público. Agendamento exibe IDs, data/hora, status, descrição, endereço autorizado e valor final quando existir; o DTO não oferece nome/modalidade histórica.

Disponibilidade é consultada em `GET /servicos/{id}/agenda?data=yyyy-MM-dd`. A criação envia somente os campos aceitos. Online/Local omitem endereço; Domicilio envia componentes, sem coordenadas. Após 201, a mensagem fala em solicitação, não confirmação.

400/401/403/404/409/429/5xx, timeout, conexão e resposta inválida são separados. Uma falha técnica depois do POST pode esconder sucesso; o app bloqueia reenvio imediato e orienta consultar a lista. A API ainda não fornece idempotência.

## Segurança e rede

Somente INTERNET é solicitada. Não há logging de senha, JWT, Authorization ou endereço. Preferências são privadas e excluídas de backup/transferência. Release recusa placeholder e exige URL HTTPS; debug permite HTTP apenas para `10.0.2.2`. Não há TrustManager permissivo.

Logout é local. O JWT permanece válido no servidor até expirar. A Sprint 5.5 não foi implementada e não foi incorporada silenciosamente.

## Evidências

| Verificação | Resultado |
|---|---:|
| Testes locais Android | 23 aprovados |
| Falhas locais | 0 |
| LiveApiTest | 1 aprovado |
| Verificações runner API/SQL | 6 aprovadas |
| Lint | 0 erros, 10 avisos |
| Build API/runner | 0 erros, 0 warnings |
| APK debug | 7.331.103 bytes |

O runner usa registros exclusivos, senha/JWT aleatórios e limpeza por IDs. IDs identity podem avançar. A suíte histórica de 288 testes da Sprint 5 não foi repetida, pois a API funcional não foi alterada; o runner recompilou o backend e exercitou diretamente os endpoints consumidos pelo app.

## Documentação e execução

- `ARQUITETURA.md`: camadas, contratos e limites.
- `AUTENTICACAO-REDE.md`: JWT, URL, HTTP debug e inicialização.
- `TESTES.md`: comandos, cobertura e integração real.
- `CHECKLIST-MANUAL.md`: aceite em dispositivo.
- `Iniciar-Api.ps1`: inicialização segura na porta 5165.
- `Tests/Sprint6/Run.ps1`: integração Kotlin/API/SQL reproduzível.

Pendências: inspeção visual/responsiva/TalkBack em dispositivo; endpoint HTTPS real de produção; idempotência para criação; proteção do token com Keystore se o projeto evoluir além do escopo acadêmico; snapshot histórico de nome/preço/modalidade no backend. Nenhuma Sprint seguinte foi iniciada.
