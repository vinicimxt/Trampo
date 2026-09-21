# Migração de banco — Sprint 5

## Decisão e execução

Primeira evolução estrutural incremental do projeto. A análise anterior à execução está em ENDERECOS.md e usos-endereco.txt. Script: `ScriptsSQL/001_enderecos_geolocalizacao.sql`. Foi aplicado ao banco Xamou em localhost\SQLEXPRESS e reexecutado com sucesso, sem DROP, recriação ou decomposição de textos antigos. Evidências: migracao.log e migracao-reexecucao.log.

As contagens e os checksums dos campos legados comparados antes/depois permaneceram iguais (banco-antes.txt e banco-depois.txt). Essa verificação detecta mudanças nos campos comparados, mas não substitui backup. Não houve preenchimento nem geocodificação em massa. Os testes posteriores criam/removem somente seus registros temporários.

## Colunas de Locais

| Campo | Tipo | Nullable | Finalidade |
|---|---|---|---|
| CEP | CHAR(8) | Sim | CEP normalizado |
| Logradouro | NVARCHAR(100) | Sim | Rua/avenida |
| Numero | NVARCHAR(20) | Sim | Número ou S/N |
| Complemento | NVARCHAR(60) | Sim | Informação adicional |
| Bairro | NVARCHAR(80) | Sim | Bairro |
| Cidade | NVARCHAR(80) | Sim | Município |
| UF | CHAR(2) | Sim | Unidade federativa |
| Latitude | DECIMAL(9,6) | Sim | Coordenada de provider confiável |
| Longitude | DECIMAL(9,6) | Sim | Coordenada de provider confiável |
| EnderecoEstruturado | BIT, default 0 | Não | Distingue legado de estrutura confirmada |

Endereco NVARCHAR(255), Nome, Id e ProfissionalId existentes foram preservados. Os componentes antigos ficam NULL. Quando estruturado, o backend valida todos os campos obrigatórios e formata Endereco para os leitores legados. Endereço formatado continua limitado a 255 caracteres.

## Nova tabela AgendamentoEnderecos

| Campo | Tipo | Nullable | Finalidade |
|---|---|---|---|
| AgendamentoId | INT, PK/FK | Não | Relação 1:1 com Agendamentos.Id |
| CEP | CHAR(8) | Sim | CEP contratado |
| Logradouro | NVARCHAR(100) | Sim | Logradouro contratado |
| Numero | NVARCHAR(20) | Sim | Número contratado |
| Complemento | NVARCHAR(60) | Sim | Complemento contratado |
| Bairro | NVARCHAR(80) | Sim | Bairro contratado |
| Cidade | NVARCHAR(80) | Sim | Cidade contratada |
| UF | CHAR(2) | Sim | UF contratada |
| Latitude | DECIMAL(9,6) | Sim | Coordenada copiada de local profissional quando disponível |
| Longitude | DECIMAL(9,6) | Sim | Coordenada copiada de local profissional quando disponível |
| EnderecoFormatado | NVARCHAR(255) | Não | Texto para exibição histórica |
| EnderecoEstruturado | BIT | Não | Origem estruturada ou legado explícito |
| Modalidade | VARCHAR(10) | Não | Local ou Domicilio |

FK com ON DELETE CASCADE: ao excluir uma reserva, remove somente seu snapshot dependente. Não há referência a endereço mutável de perfil. O relacionamento lógico é Agendamentos 1 → 0..1 AgendamentoEnderecos: Online e reservas anteriores podem não ter linha.

Constraints verificam modalidade, coordenadas em par (ou ambas NULL), latitude -90..90, longitude -180..180 e componentes não nulos/CEP numérico quando estruturado. UF, conteúdo, tamanhos e formatação também são validados pela aplicação. A PK garante unicidade e atende as consultas por AgendamentoId. Não foram criados índices especulativos de CEP/Cidade/UF.

## Aplicação em outro ambiente

Parar gravações, realizar backup conforme a política do ambiente e conferir o banco de destino. Executar somente o script incremental:

```powershell
sqlcmd -S 'localhost\SQLEXPRESS' -d Xamou -E -C -b -i ScriptsSQL/001_enderecos_geolocalizacao.sql
```

O script usa XACT_ABORT, TRY/CATCH e transação; alterações são aditivas e verificam existência antes de criar. A reexecução foi testada sobre o esquema esperado; não é um mecanismo de reparo de esquemas divergentes. Publicar a versão C# após a migração, pois ela lê as novas colunas. Não executar ScriptBD.txt sobre o banco atual para aplicar esta evolução.

Rollback operacional: a versão anterior do aplicativo pode continuar usando os campos legados. Não remover novas colunas/tabela com dados para reverter a aplicação; qualquer reversão estrutural exige plano separado. Falha durante a migração reverte sua transação.

## Artefatos do PIM IV

Atualizar o MER com a entidade dependente AgendamentoEnderecos e cardinalidade 1:0..1; atualizar o modelo lógico com PK/FK e modalidade; atualizar o modelo físico com tipos/constraints acima. Para reproduzir o esquema evoluído, usar a base documentada em ScriptBD.txt em ambiente vazio apropriado e depois 001_enderecos_geolocalizacao.sql. A base original não foi reescrita; esta documentação e o script incremental descrevem a evolução para a consolidação posterior do script completo.
