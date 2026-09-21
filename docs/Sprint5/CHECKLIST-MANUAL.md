# Checklist visual — Sprint 5

A automação de navegador falhou na inicialização durante esta Sprint. Os testes HTTP/HTML passaram, mas não são inspeção visual. Todos os itens abaixo permanecem pendentes de execução em navegador; registrar data, navegador e evidência.

Preparação: usar contas e dados de teste, aplicação local, migração aplicada. Repetir em desktop e largura de 375 px; conferir console, foco visível e ausência de rolagem horizontal.

## Cadastro de local

- [ ] Abrir Local/Lista, Novo local e modal de edição pelo teclado.
- [ ] Consultar CEP válido; conferir loading, status, campos preenchidos e foco no número.
- [ ] Informar CEP inválido e inexistente; conferir erro compreensível e correção manual.
- [ ] Pesquisar por UF/cidade/logradouro e selecionar resultado com teclado.
- [ ] Simular falha de rede; preencher todos os campos manualmente e salvar.
- [ ] Editar local estruturado e confirmar persistência dos componentes.
- [ ] Abrir local legado; conferir texto original sem decomposição; alternar modo e confirmar componentes.
- [ ] Abrir dois locais consecutivos; confirmar que campos, resultados e resumo não conservam dados do anterior.
- [ ] Tentar editar/excluir local histórico; conferir mensagem e preservação dos dados.
- [ ] Conferir Tab/Shift+Tab, Escape, retorno de foco ao fechar modal e labels com leitor de tela.

## Agendamento Domicilio

- [ ] Selecionar data/horário e consultar CEP.
- [ ] Completar número/complemento e revisar o resumo antes de confirmar.
- [ ] Omitir campos obrigatórios; conferir feedback e possibilidade de corrigir.
- [ ] Conferir endereço em Meus e, após confirmação, Recebidos.
- [ ] Fazer nova reserva com outro endereço e conferir histórico da anterior.
- [ ] Reservar Online sem campos físicos e Local com local profissional apropriado.

## Busca e privacidade

- [ ] Pesquisar endereço por texto; testar nenhum resultado, pesquisa curta e resultado selecionado.
- [ ] Conferir busca existente por serviço/profissional/categoria/localização textual.
- [ ] Confirmar ausência de coleta automática de localização, mapa obrigatório ou promessa de distância.
- [ ] Consultar catálogo sem login e garantir que não exibe domicílio ou coordenadas privadas.

## Registro

| Data/navegador | Item | Resultado | Evidência |
|---|---|---|---|
| A preencher | Validação visual | Pendente | Automação não iniciou |
