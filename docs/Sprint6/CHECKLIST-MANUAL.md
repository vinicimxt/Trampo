# Sprint 6 — Checklist em Dispositivo

Status: pendente em emulador/dispositivo real. O ambiente possui SDK e ADB, mas não tinha AVD, imagem de sistema ou dispositivo conectado. Os testes Robolectric não substituem aceite visual.

## Preparação

1. Abrir `C:\Users\Vinicius\AndroidStudioProjects\TRAMPO` no Android Studio.
2. Criar/iniciar um dispositivo no Device Manager, sincronizar Gradle e selecionar debug.
3. Executar `docs/Sprint6/Iniciar-Api.ps1`; SQL Server e a migração 001 da Sprint 5 devem estar disponíveis.
4. Usar conta cliente de teste e ofertas ativas com disponibilidade futura.

## Fluxo funcional

- [ ] Login cliente → Home com nome vindo de `/auth/me`.
- [ ] Catálogo e detalhes apresentam os dados reais da API.
- [ ] A data consulta a agenda; os horários vêm do servidor.
- [ ] Online e Local não pedem endereço do cliente.
- [ ] Domicilio consulta CEP pela API TRAMPO e permite preenchimento manual.
- [ ] Revisão precede o POST; cliques repetidos não geram pedidos simultâneos.
- [ ] Sucesso mostra “Agendamento solicitado com sucesso.”.
- [ ] O pedido pendente aparece no Mobile e no Web com o mesmo ID.
- [ ] Cancelamento pede confirmação e o novo status aparece também no Web.

## Falhas e sessão

- [ ] Campos vazios, senha incorreta e perfil não cliente apresentam mensagens.
- [ ] Fechar/reabrir com token válido retorna à Home após `/auth/me`.
- [ ] Token expirado pede login; Voltar não recupera tela autenticada.
- [ ] API desligada aparece como falha de conexão, não senha incorreta.
- [ ] 403/404/409/429/500 apresentam mensagens legíveis.
- [ ] Após conflito, atualizar agenda antes de reenviar.
- [ ] Após resultado incerto do POST, consultar a lista antes de tentar novamente.

## Visual e acessibilidade

- [ ] Tema claro/escuro, fonte grande, tela pequena e horizontal.
- [ ] Teclado não cobre campos/botões; rolagem funciona.
- [ ] Rotação preserva rascunho em memória e não repete POST.
- [ ] TalkBack anuncia labels, horário selecionado, estado e erro.
- [ ] Ordem de foco, contraste e alvos de toque adequados.
- [ ] Listas vazias, paginação e loading são compreensíveis.
