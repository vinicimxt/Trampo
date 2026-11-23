-- Somente leitura. Executar conectado diretamente ao banco Xamou apos 000 e 001.
SET NOCOUNT ON;

SELECT DB_NAME() AS BancoAtual;

SELECT e.Nome, CASE WHEN t.object_id IS NULL THEN 'AUSENTE' ELSE 'OK' END AS Estado
FROM (VALUES
  (N'Usuarios'), (N'Clientes'), (N'Profissionais'), (N'Categorias'),
  (N'Subcategorias'), (N'Locais'), (N'Servicos'), (N'Disponibilidade'),
  (N'BloqueiosAgenda'), (N'Agendamentos'), (N'Avaliacoes'),
  (N'ContatoSuporte'), (N'Notificacoes'), (N'Assinaturas'),
  (N'AgendamentoEnderecos')
) AS e(Nome)
LEFT JOIN sys.tables AS t ON t.schema_id = SCHEMA_ID(N'dbo') AND t.name = e.Nome
ORDER BY e.Nome;

SELECT name, parent_object_id, referenced_object_id, is_disabled, is_not_trusted
FROM sys.foreign_keys ORDER BY name;
SELECT name, parent_object_id, is_disabled, is_not_trusted
FROM sys.check_constraints ORDER BY name;
SELECT OBJECT_SCHEMA_NAME(object_id) AS Esquema, OBJECT_NAME(object_id) AS Tabela,
       name, is_unique, is_primary_key, is_disabled
FROM sys.indexes WHERE object_id IN (SELECT object_id FROM sys.tables WHERE schema_id = SCHEMA_ID(N'dbo'))
  AND index_id > 0 ORDER BY Tabela, name;

SELECT e.name AS ColunaMigracao, CASE WHEN c.column_id IS NULL THEN 'AUSENTE' ELSE 'OK' END AS Estado
FROM (VALUES (N'CEP'), (N'Logradouro'), (N'Numero'), (N'Complemento'),
             (N'Bairro'), (N'Cidade'), (N'UF'), (N'Latitude'),
             (N'Longitude'), (N'EnderecoEstruturado')) AS e(name)
LEFT JOIN sys.columns AS c ON c.object_id = OBJECT_ID(N'dbo.Locais') AND c.name = e.name;

SELECT COUNT(*) AS TotalCategorias FROM dbo.Categorias;
SELECT COUNT(*) AS TotalSubcategorias FROM dbo.Subcategorias;
