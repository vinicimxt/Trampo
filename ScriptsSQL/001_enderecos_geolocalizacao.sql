-- Evolução aditiva. Executar no banco existente (ex.: sqlcmd -d Xamou -i este-arquivo).
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF OBJECT_ID(N'dbo.Locais', N'U') IS NULL OR OBJECT_ID(N'dbo.Agendamentos', N'U') IS NULL
        THROW 52000, 'Esquema base não encontrado.', 1;
    IF COL_LENGTH('dbo.Locais','CEP') IS NULL ALTER TABLE dbo.Locais ADD CEP CHAR(8) NULL;
    IF COL_LENGTH('dbo.Locais','Logradouro') IS NULL ALTER TABLE dbo.Locais ADD Logradouro NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Locais','Numero') IS NULL ALTER TABLE dbo.Locais ADD Numero NVARCHAR(20) NULL;
    IF COL_LENGTH('dbo.Locais','Complemento') IS NULL ALTER TABLE dbo.Locais ADD Complemento NVARCHAR(60) NULL;
    IF COL_LENGTH('dbo.Locais','Bairro') IS NULL ALTER TABLE dbo.Locais ADD Bairro NVARCHAR(80) NULL;
    IF COL_LENGTH('dbo.Locais','Cidade') IS NULL ALTER TABLE dbo.Locais ADD Cidade NVARCHAR(80) NULL;
    IF COL_LENGTH('dbo.Locais','UF') IS NULL ALTER TABLE dbo.Locais ADD UF CHAR(2) NULL;
    IF COL_LENGTH('dbo.Locais','Latitude') IS NULL ALTER TABLE dbo.Locais ADD Latitude DECIMAL(9,6) NULL;
    IF COL_LENGTH('dbo.Locais','Longitude') IS NULL ALTER TABLE dbo.Locais ADD Longitude DECIMAL(9,6) NULL;
    IF COL_LENGTH('dbo.Locais','EnderecoEstruturado') IS NULL
        ALTER TABLE dbo.Locais ADD EnderecoEstruturado BIT NOT NULL CONSTRAINT DF_Locais_EnderecoEstruturado DEFAULT 0;
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Locais_Coordenadas')
        EXEC(N'ALTER TABLE dbo.Locais WITH CHECK ADD CONSTRAINT CK_Locais_Coordenadas CHECK
            ((Latitude IS NULL AND Longitude IS NULL) OR
             (Latitude IS NOT NULL AND Longitude IS NOT NULL AND Latitude BETWEEN -90 AND 90 AND Longitude BETWEEN -180 AND 180));');
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE name='CK_Locais_EnderecoEstruturado')
        EXEC(N'ALTER TABLE dbo.Locais WITH CHECK ADD CONSTRAINT CK_Locais_EnderecoEstruturado CHECK
            (EnderecoEstruturado=0 OR (CEP IS NOT NULL AND CEP NOT LIKE ''%[^0-9]%'' AND
             Logradouro IS NOT NULL AND Numero IS NOT NULL AND Bairro IS NOT NULL AND Cidade IS NOT NULL AND UF IS NOT NULL));');
    IF OBJECT_ID(N'dbo.AgendamentoEnderecos', N'U') IS NULL
        CREATE TABLE dbo.AgendamentoEnderecos (
            AgendamentoId INT NOT NULL CONSTRAINT PK_AgendamentoEnderecos PRIMARY KEY,
            CEP CHAR(8) NULL,
            Logradouro NVARCHAR(100) NULL,
            Numero NVARCHAR(20) NULL,
            Complemento NVARCHAR(60) NULL,
            Bairro NVARCHAR(80) NULL,
            Cidade NVARCHAR(80) NULL,
            UF CHAR(2) NULL,
            Latitude DECIMAL(9,6) NULL,
            Longitude DECIMAL(9,6) NULL,
            EnderecoFormatado NVARCHAR(255) NOT NULL,
            EnderecoEstruturado BIT NOT NULL,
            Modalidade VARCHAR(10) NOT NULL,
            CONSTRAINT FK_AgendamentoEnderecos_Agendamentos FOREIGN KEY(AgendamentoId)
                REFERENCES dbo.Agendamentos(Id) ON DELETE CASCADE,
            CONSTRAINT CK_AgendamentoEnderecos_Modalidade CHECK(Modalidade IN ('Local','Domicilio')),
            CONSTRAINT CK_AgendamentoEnderecos_Coordenadas CHECK
                ((Latitude IS NULL AND Longitude IS NULL) OR
                 (Latitude IS NOT NULL AND Longitude IS NOT NULL AND Latitude BETWEEN -90 AND 90 AND Longitude BETWEEN -180 AND 180)),
            CONSTRAINT CK_AgendamentoEnderecos_Estruturado CHECK
                (EnderecoEstruturado=0 OR (CEP IS NOT NULL AND CEP NOT LIKE '%[^0-9]%' AND
                 Logradouro IS NOT NULL AND Numero IS NOT NULL AND Bairro IS NOT NULL AND Cidade IS NOT NULL AND UF IS NOT NULL))
        );
    COMMIT;
    PRINT 'Migração 001 aplicada/verificada sem alterar textos legados.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
