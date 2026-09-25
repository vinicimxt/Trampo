# Sprint 8.5 — refinamento visual e UX mobile

## Baseline de retomada — 22/09/2026

Auditoria realizada antes das alterações nos 11 layouts, recursos values/values-night/values-v28, drawables, launcher, Activities, adapters, formatação e modelos. Projeto Android: C:\Users\Vinicius\AndroidStudioProjects\TRAMPO (sem repositório Git). Cópia dos fontes anteriores preservada em artifacts/sprint85-before no backend.

- Log anterior sprint85-baseline.log: testDebugUnitTest, lintDebug e assembleDebug concluídos com BUILD SUCCESSFUL; evidência histórica, não nova execução.
- Títulos de 28sp repetidos em cards e cabeçalhos; todas as ações com o mesmo botão preenchido; ausência de hierarquia entre voltar, cancelar e confirmar.
- Paleta verde sem relação com a logo oficial; launcher padrão Android; nenhuma logo nos recursos.
- Logo oficial: PNG 584 × 427, ARGB com transparência, símbolo azul/ciano/roxo. Preservar proporção e arquivo original; cópia integral no Android.
- Campos simples com labels; observações herdavam singleLine. Layouts roláveis já presentes. Insets de barras/teclado e adjustResize existentes.
- Loaders e mensagens já existem; descrições dos serviços escondidas nos cards; dados agrupados em textos extensos.
- Material Components já disponível; nenhuma dependência adicional necessária.
- AgendamentoResponse não contém nome do serviço nem modalidade: manter referência real do serviço, sem inventar nomes/modalidades nem alterar API.
- Emulador Pixel_6 configurado, inicialmente desligado. Validação visual e resultados finais serão registrados após a execução.

## Resultado

Refinamento implementado no projeto Android existente, mantendo Kotlin/XML, Activities, ViewModels, Retrofit/OkHttp e autenticação JWT. Nenhum código funcional do backend, endpoint, modelo de rede, Repository ou regra de negócio foi alterado. Não foram adicionadas dependências.

## Identidade visual e componentes

- Paleta roxa com apoio ciano, derivada da marca; cores semânticas e superfícies próprias para temas claro e escuro.
- Estilos centralizados de títulos, subtítulos, labels, preço, campos, cards, mensagens e badges. Ações principais preenchidas, secundárias contornadas, voltar/atualizar/sair discretos e cancelamento em cor de erro.
- Logo oficial copiada integralmente para `app/src/main/res/drawable-nodpi/trampo_logo.png`; igualdade SHA-256 confirmada com o arquivo Web. `drawable-nodpi` evita reescala por densidade. Uso em Login e Home com `fitCenter`, proporção preservada e descrição TRAMPO.
- Launcher usa a marca original em área segura sobre fundo branco, mantendo adaptive icon e camada monochrome. Recursos `mipmap-anydpi` fornecem fallback nas versões suportadas anteriores à API 26. Os dez WebP do robô Android foram substituídos por esses recursos; backup preservado. A aparência do launcher em diferentes máscaras e modo de ícones temáticos ainda requer conferência específica.
- Doze vetores locais: busca, calendário, voltar, atualizar, sair, casa, online, localização, relógio, seta, fechar e confirmar. Sem biblioteca adicional e sem emojis.

## Telas e UX

- Login: marca, slogan, labels permanentes, foco nos campos, ação Entrar e ação Concluir do teclado usando o mesmo fluxo de autenticação.
- Home: saudação real, logo e ações com ícones e explicações.
- Catálogo: nome, preço, modalidade com ícone real e descrição de até três linhas. Detalhe exibe descrição completa e ação de agenda.
- Agenda: resumo do serviço, seção de data/horário, instrução inicial para escolher data, observações multilinha, revisão e endereço em seção própria para Domicílio. Fallback manual preservado.
- Agendamentos: referência verdadeira do serviço, data/hora, badge de status e referência do pedido. API atual não fornece nome do serviço/modalidade no agendamento; não foram inventados nem buscados por chamadas extras.
- Detalhe do agendamento: seções de data/hora e atendimento, endereço real retornado, valor disponível e cancelamento com confirmação e elegibilidade originais.
- Estados: mensagens vazias recolhem seu espaço; loading e erros mantêm live region; erros usam cor/superfície semântica; botão de erro oferece Tentar novamente. Estados vazios trazem orientação curta.
- Status existentes traduzidos para apresentação sem alterar o valor de rede: Pendente, Confirmado, Finalizado, Cancelado, CanceladoCliente, CanceladoProfissional e AguardandoCliente. Valor desconhecido continua visível como retornado.

## Acessibilidade e responsividade

Labels associados aos campos, alvos de toque de pelo menos 48dp, textos em sp, títulos semânticos herdados, conteúdo rolável e mensagens anunciáveis. Os status têm texto, além de cor. Ícones que acompanham texto não adicionam descrições duplicadas.

Insets das barras e do teclado aplicados à área rolável, mantendo o padding da página. Corrigida sobreposição sob barras do sistema em formulários longos. Com foco na senha, a rolagem reserva espaço para Entrar acima do teclado. Contraste dos ícones das barras acompanha tema claro/escuro. Observações deixam de herdar singleLine; campo Número continua alfanumérico para endereços como S/N.

## Testes e build — execução final

Com `JAVA_HOME=C:\Program Files\Android\Android Studio\jbr`, na raiz Android:

```powershell
.\gradlew.bat testDebugUnitTest lintDebug assembleDebug --console=plain
```

| Verificação | Resultado |
| --- | --- |
| Testes unitários/Robolectric | 24 descobertos, 23 aprovados, 1 ignorado, zero falhas/erros |
| Teste ignorado | LiveApiTest requer o runner dedicado de integração |
| Inflação de layouts | Teste existente de telas e itens aprovado |
| Lint | Zero erros; 10 avisos preexistentes |
| Avisos | 9 sugestões de atualização de dependências; 1 TextFields para Número alfanumérico |
| Build | BUILD SUCCESSFUL; APK Debug gerado; versão de validação instalada no emulador |
| Logo | Cópia integral confirmada por hash |

Aviso de acesso nativo do Robolectric no Java 25 permaneceu. Não houve supressão de lint nem atualização de dependências. O runner Sprint 6 e uma nova regressão backend completa não foram executados nesta retomada; o smoke abaixo utilizou a API e o SQL reais.

## Validação visual efetivamente realizada

Pixel_6/API 37 (1080 × 2400), iniciado sem janela e controlado por ADB, com screenshots reais inspecionadas. Backend local em `http://10.0.2.2:5165/` e SQL Express Xamou. Criados dois usuários temporários, profissional, local e três serviços temporários (Online, Local e Domicílio). Nenhum dado preexistente foi alterado.

| Tela/fluxo | Evidência observada |
| --- | --- |
| Login | Tela clara, campos, login real, envio pela ação do teclado |
| Login acessível | Tema escuro, fonte 130%, teclado aberto; campos e Entrar alcançáveis/visíveis |
| Home | Saudação da fixture, logo e navegação |
| Serviços | Cards com modalidades Online e Local; rolagem até Domicílio; descrição longa |
| Detalhe do serviço | Online e Domicílio, preço, modalidade e descrição completa |
| Agenda | Data futura e horários reais; instrução inicial e formulário Online sem endereço |
| Novo agendamento | Domicílio, endereço fictício preenchido manualmente, revisão e envio |
| Meus agendamentos | Estado vazio inicial e card Pendente após criação |
| Detalhe do agendamento | Endereço, horário, status e ação de cancelamento |
| Cancelamento | Diálogo, confirmação da reserva temporária, badge Cancelado por você e ação removida |

O smoke completo foi realizado durante a iteração. Após os últimos ajustes de insets, foram reabertos Login, Home, catálogo, detalhe Online, agenda Online, lista/detalhe da reserva e cancelamento. O formulário Domicílio preenchido/revisado foi validado antes do ajuste final de insets; não foi feita uma segunda reserva após esse ajuste. Fonte 130%/tema escuro foram inspecionados no Login, não em todas as telas.

As capturas que revelaram o problema de teclado/rolagem foram mantidas como evidência de iteração; `keyboard-fixed.png`, `login-dark-large.png`, `keyboard-dark-large.png` e `agenda-final.png` mostram as correções finais. Horário do emulador aparece em UTC, avançando a data em relação à sessão local.

Após o smoke, a checagem independente dos XML detectou uma sequência de bytes fora de UTF-8 no texto de AguardandoCliente. O recurso foi normalizado, todos os XML e fontes alterados foram validados como UTF-8 e os testes/lint/build foram repetidos. O APK regenerado após essa normalização não foi reinstalado; a alteração final foi somente de codificação desse recurso.

Fixtures removidas por seus IDs exclusivos ao terminar, incluindo a reserva de teste. Credenciais temporárias locais apagadas. Fonte 1.0 e tema claro do emulador restaurados. Sessão de fixture invalidada e aplicativo voltou ao Login. API e emulador iniciados para a validação foram encerrados. A imagem API 37 registrou falhas do serviço UWB do próprio emulador; o fluxo TRAMPO descrito acima foi concluído apesar disso.

## Evidências e artefatos locais

- Android: `sprint85-baseline.log` (anterior), `sprint85-final.log`, `app/build/reports/lint-results-debug.html`, `app/build/test-results/testDebugUnitTest/`.
- APK: `C:\Users\Vinicius\AndroidStudioProjects\TRAMPO\app\build\outputs\apk\debug\app-debug.apk`.
- Backend, pasta ignorada: `artifacts/sprint85-visual/` (capturas PNG), `artifacts/sprint85-files.json` (inventário), `artifacts/sprint85-before/src/` (backup integral anterior).
- Apenas este relatório foi adicionado ao repositório backend. O Android continua sem repositório Git; nenhuma publicação, commit ou push realizado.

## Limitações e pendências

- Revisão humana do visual antes da apresentação; TalkBack, orientação horizontal, outras densidades, fonte 200% e versões anteriores não foram exercitados.
- Conferir launcher em outras máscaras e modo de ícones temáticos; a validação visual principal ocorreu dentro do aplicativo.
- Consulta externa de CEP, simulação visual de timeout/offline e todos os estados possíveis de agendamento não foram exercitados no emulador nesta sessão. Testes automatizados existentes continuam aprovados.
- Release/assinatura, cloud, API e funcionalidades novas permanecem fora desta sprint.
- Versionar o projeto Android e preservar as evidências para a entrega acadêmica.

## Inventário exato

Caminhos relativos a `C:\Users\Vinicius\AndroidStudioProjects\TRAMPO\app\src`.


### Arquivos criados (21)

- `main/res/drawable-nodpi/trampo_logo.png`
- `main/res/drawable/bg_badge.xml`
- `main/res/drawable/bg_field.xml`
- `main/res/drawable/bg_logo.xml`
- `main/res/drawable/bg_message.xml`
- `main/res/drawable/bg_section.xml`
- `main/res/drawable/ic_arrow.xml`
- `main/res/drawable/ic_back.xml`
- `main/res/drawable/ic_calendar.xml`
- `main/res/drawable/ic_check.xml`
- `main/res/drawable/ic_clock.xml`
- `main/res/drawable/ic_close.xml`
- `main/res/drawable/ic_home.xml`
- `main/res/drawable/ic_location.xml`
- `main/res/drawable/ic_logout.xml`
- `main/res/drawable/ic_online.xml`
- `main/res/drawable/ic_refresh.xml`
- `main/res/drawable/ic_search.xml`
- `main/res/mipmap-anydpi/ic_launcher.xml`
- `main/res/mipmap-anydpi/ic_launcher_round.xml`
- `main/res/values/dimens.xml`

### Arquivos alterados (25)

- `main/java/com/example/trampo/ui/BaseActivity.kt`
- `main/java/com/example/trampo/ui/Formatting.kt`
- `main/java/com/example/trampo/ui/agendamentos/AgendamentosActivity.kt`
- `main/java/com/example/trampo/ui/agendamentos/NovoAgendamentoActivity.kt`
- `main/java/com/example/trampo/ui/login/LoginActivity.kt`
- `main/java/com/example/trampo/ui/servicos/ServicosActivity.kt`
- `main/res/drawable/ic_launcher_background.xml`
- `main/res/drawable/ic_launcher_foreground.xml`
- `main/res/layout/activity_agendamentos.xml`
- `main/res/layout/activity_detalhe_agendamento.xml`
- `main/res/layout/activity_detalhe_servico.xml`
- `main/res/layout/activity_home.xml`
- `main/res/layout/activity_login.xml`
- `main/res/layout/activity_main.xml`
- `main/res/layout/activity_novo_agendamento.xml`
- `main/res/layout/activity_servicos.xml`
- `main/res/layout/include_endereco.xml`
- `main/res/layout/item_agendamento.xml`
- `main/res/layout/item_servico.xml`
- `main/res/mipmap-anydpi-v26/ic_launcher.xml`
- `main/res/mipmap-anydpi-v26/ic_launcher_round.xml`
- `main/res/values-night/colors.xml`
- `main/res/values/colors.xml`
- `main/res/values/strings.xml`
- `main/res/values/themes.xml`

### Recursos antigos substituídos (10)

- `main/res/mipmap-hdpi/ic_launcher.webp`
- `main/res/mipmap-hdpi/ic_launcher_round.webp`
- `main/res/mipmap-mdpi/ic_launcher.webp`
- `main/res/mipmap-mdpi/ic_launcher_round.webp`
- `main/res/mipmap-xhdpi/ic_launcher.webp`
- `main/res/mipmap-xhdpi/ic_launcher_round.webp`
- `main/res/mipmap-xxhdpi/ic_launcher.webp`
- `main/res/mipmap-xxhdpi/ic_launcher_round.webp`
- `main/res/mipmap-xxxhdpi/ic_launcher.webp`
- `main/res/mipmap-xxxhdpi/ic_launcher_round.webp`
