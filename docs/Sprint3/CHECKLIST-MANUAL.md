# Checklist manual — Sprint 3

## Alcance da validação realizada

Os testes automatizados verificam HTTP, HTML e persistência SQL. Não constituem validação visual. Na retomada em 20/09/2026, o navegador de automação não iniciou: helper_sandbox_lock_failed / SetNamedSecurityInfoW (erro 5). Nenhuma tela foi inspecionada visualmente nesta execução.

## Preparação

Executar a aplicação pelo perfil local do IDE e usar a URL informada no terminal. Usar contas de teste de cliente e de dois profissionais, sem dados reais; cadastrar categoria/subcategoria e locais necessários em ambiente de teste. Reservas válidas devem estar no futuro, dentro de três meses e da disponibilidade. Para testar conclusão, preparar uma reserva de teste cujo horário já tenha passado. Não alterar reservas reais.

Repetir os fluxos em desktop e largura de 375 px, observando sobreposição, rolagem horizontal, legibilidade, foco e console do navegador. Marcar os itens somente após execução; registrar navegador, data e evidência.

## Fluxos

- [ ] Cadastro: abrir /Usuario/Cadastro, testar campos obrigatórios, senha curta, mostrar/ocultar senha e cadastro válido; confirmar rótulos e mensagens.
- [ ] Login: abrir /Usuario/Login, testar senha incorreta, login válido e logout; tentar acessar página protegida após logout.
- [ ] Home: abrir / e /Home/Index; seguir busca, cadastro e navegação principal.
- [ ] Busca: abrir /Profissional/Lista, filtrar e abrir perfil público; verificar resultados vazios e profissional sem contato/foto.
- [ ] Local: como profissional, abrir /Local/Lista?abrir=true, cadastrar local e voltar à criação de serviço; tentar excluir/alterar local usado no histórico e conferir a recusa.
- [ ] Serviço: abrir /Servico/Criar, escolher categoria/subcategoria, criar ofertas Local, Online e Domicilio; testar link, preço e endereço inválidos.
- [ ] Disponibilidade: selecionar dias e intervalo, salvar, reabrir edição e verificar persistência; alterar dias sem duplicar regras. Conferir intervalo que cruza meia-noite.
- [ ] Edição: em /Profissional/MeusServicos, editar nome, preço e disponibilidade; abrir outro modal e confirmar que não conserva o local anterior.
- [ ] Reserva: como cliente, escolher serviço/data/horário e enviar; conferir ausência de horários bloqueados, passados e ocupados; testar endereço domiciliar obrigatório.
- [ ] Modal: abrir por teclado, usar Tab/Shift+Tab, Escape e fechar; verificar foco devolvido ao acionador e rolagem restaurada.
- [ ] Meus Agendamentos: conferir novo pedido, detalhes, filtros e cancelamento; conferir status e mensagem após ação.
- [ ] Recebidos: confirmar, recusar solicitação pendente e cancelar reserva; testar tentativa repetida e profissional que não é proprietário.
- [ ] Conclusão: finalizar atendimento de teste já iniciado, conferir valor fixo, taxa/líquido e confirmação pelo cliente; validar recusa antes do horário.
- [ ] Notificações: conferir avisos para os participantes, contador, dropdown, leitura individual e todas; tentar ler aviso de outra conta.
- [ ] Avaliação: após conclusão confirmada, enviar nota/comentário e conferir exibição; tentar duplicar ou avaliar atendimento alheio.
- [ ] Histórico: remover oferta com reservas e conferir permanência de reservas/avaliações. A limitação de atributos atuais está documentada em HISTORICO.md.
- [ ] Dashboard: abrir /Profissional/Dashboard; conferir totais e valores após os fluxos, inclusive cancelados.
- [ ] Acessibilidade: percorrer navegação/formulários/modais sem mouse; conferir labels, foco visível e anúncios das mensagens com leitor de tela.

## Registro

| Data/navegador | Fluxo | Resultado | Evidência/defeito |
|---|---|---|---|
| A preencher | Validação visual pendente | Não executada | Automação bloqueada pelo ambiente |
