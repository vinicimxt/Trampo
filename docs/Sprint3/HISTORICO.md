# Histórico do contrato — decisão técnica da Sprint 3

## Evidência e problema atual

A análise de ScriptBD.txt, DAO/AgendamentoDAO.cs, Services/AgendamentoService.cs e das listagens mostra que preservar a reserva e seu local não preserva todos os atributos contratados. As consultas ainda juntam Agendamentos ao cadastro atual de Servicos. Alterar a oferta pode alterar o nome, a modalidade, o link e o preço apresentados em reservas antigas.

Finalizar também consulta o TipoPreco e PrecoBase atuais do serviço para determinar o valor fixo. Portanto a limitação afeta reservas em andamento, além da apresentação histórica. ValorFinal, Taxa e ValorLiquido, depois de gravados, são valores próprios do agendamento e não devem ser recalculados ao editar a oferta.

## Campos afetados

| Dado | Situação atual | Decisão para evolução |
|---|---|---|
| Nome do serviço | Lido de Servicos.Nome | Capturar na criação da reserva |
| Modalidade | Lida de Servicos.Atendimento | Capturar na criação da reserva |
| Tipo de preço e preço base | Cadastro atual; PrecoBase pode ser NULL para Combinar | Capturar ambos; NULL continua significando preço a combinar |
| Valor final, taxa e líquido | Persistidos no agendamento na finalização | Preservar; não confundir preço ofertado com valor final |
| Link online | Cadastro atual; coluna aceita NULL | Capturar quando Online; mudança posterior exige política explícita de comunicação |
| Local e endereço | Agendamento guarda LocalId; apresentação consulta Locais | Capturar nome/endereço do local, mantendo a referência |
| Endereço domiciliar | EnderecoCliente já pertence à reserva e aceita NULL | Preservar; obrigatório apenas para Domicilio |

## Alternativas

1. Continuar com joins: sem migração, mas permite alteração retroativa da apresentação e do preço usado para concluir.
2. Impedir toda edição de serviço com reservas: reduz o problema, mas restringe desnecessariamente o catálogo e não recupera alterações antigas.
3. Versionar todo o serviço: consistente, porém exige versões, referências e regras adicionais.
4. Snapshot dos atributos contratados em Agendamentos: solução incremental recomendada, mantendo os IDs para relacionamentos e copiando somente os dados necessários ao contrato.

## Decisão e impacto no banco

Recomenda-se a alternativa 4 para implementação posterior. Nesta Sprint foi feita somente a decisão técnica: nenhuma coluna foi adicionada e nenhum script de migração foi criado ou executado. A manutenção da apresentação atual é uma limitação explícita, não uma garantia de snapshot.

A migração futura deverá adicionar campos anuláveis para nome, modalidade, tipo de preço, preço base, link e endereço/nome do local, além de um indicador de origem/versão do snapshot. Os tamanhos devem respeitar os campos de origem e o preço deve usar DECIMAL(10,2). Os nomes finais e o esquema efetivamente instalado precisam ser conferidos antes de gerar o script incremental.

## Estratégia de migração e compatibilidade

1. Fazer backup e conferir o esquema instalado; aplicar somente ALTER TABLE incremental, sem DROP ou recriação.
2. Publicar leitura compatível com registros antigos e novos. Usar a versão/origem para distinguir um snapshot ausente de um campo legitimamente NULL, como preço a combinar ou link não aplicável. COALESCE indiscriminado por campo não resolve essa distinção.
3. Capturar todos os atributos na mesma transação e sob o mesmo bloqueio da criação da reserva. Não capturar no Controller nem confiar no formulário para esses valores.
4. Fazer listagens e finalização usarem o snapshot para novas reservas. Manter ValorFinal histórico já gravado.
5. Para reservas antigas, não apresentar dados atuais como se fossem os originais. Sem fonte histórica confiável, manter fallback explicitamente identificado como legado; eventual preenchimento será marcado como reconstruído, nunca original.
6. Testar reserva, edição da oferta, histórico inalterado, preço fixo na conclusão, modalidade/local/link, preço a combinar, rollback e convivência com legado antes da publicação.

A configuração de preço de reservas antigas em andamento e mudanças necessárias no link de reunião exigem política de negócio explícita nessa evolução. A Sprint 4 não foi iniciada automaticamente.
