# Trampo

Plataforma web para agendamento de serviços entre clientes e profissionais.

---

## Funcionalidades

- Cadastro e login
- Perfil de cliente e profissional
- Agendamento de serviços
- Avaliações
- Plano premium
- Dashboard profissional
- Histórico de agendamentos
- Disponibilidade de horários
- Painel administrativo visual
- Acessibilidade com VLibras

---

## Tecnologias

- ASP.NET MVC
- C#
- SQL Server
- HTML
- CSS
- JavaScript

---

## Banco de Dados

O sistema utiliza SQL Server com:
- usuários
- profissionais
- clientes
- serviços
- agendamentos
- avaliações
- notificações
- assinaturas premium

---

## Acessibilidade

- Compatibilidade com VLibras
- Labels semânticas
- Navegação estruturada
- Inputs acessíveis
- ALT em imagens importantes

---

## Imagens

[screenshots]
<img width="1920" height="1080" alt="image" src="https://github.com/user-attachments/assets/26ff776d-688a-4516-98ee-2e28692dd004" />
<img width="1896" height="868" alt="image" src="https://github.com/user-attachments/assets/f390c51f-87f4-4691-b0a0-3f9599344a4b" />
<img width="1859" height="740" alt="image" src="https://github.com/user-attachments/assets/701661fa-1a67-43f9-87a8-865bdbac6f66" />
<img width="1864" height="758" alt="image" src="https://github.com/user-attachments/assets/884dc120-f89c-4f8f-bfb3-33b288649f4a" />
<img width="1860" height="864" alt="image" src="https://github.com/user-attachments/assets/6b948691-e64b-4e25-8ce3-e76d19499474" />
<img width="1865" height="869" alt="image" src="https://github.com/user-attachments/assets/15bcc57e-12a1-4216-a23e-9a8594aa082f" />

---

## Arquitetura

O projeto foi desenvolvido utilizando o padrão MVC (Model-View-Controller), separando:
- regras de negócio
- interface
- controle de fluxo
- persistência de dados

---
## Folders
```text
BD_TRAMPO/
│
├── Controllers/
├── Models/
├── Views/
├── DAO/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── assets/
│
├── ScriptsSQL/
├── ViewModels/
└── Program.cs
```

---


## Estrutura do projeto

- Controllers → regras e fluxo da aplicação
- Models → entidades do sistema
- DAO → acesso ao banco de dados
- Views → interface do usuário
- ViewModels → comunicação entre controller e view
- wwwroot → arquivos estáticos (CSS, JS e imagens)


---


## Como executar

1. Clone o projeto
2. Abra no Visual Studio / VSCode
3. Baixe a extensão do C#
4. Configure a connection string
5. Copie o ScriptBD e Execute o script SQL SERVER 
6. Rode o projeto ("dotnet watch run" no terminal)

## REST API V1 (Sprint 4)

MVC continua com sessão; a API em `/api/v1` usa JWT Bearer e compartilha os Services. Não é necessário mudar o banco para executar esta Sprint.

A chave JWT não está no repositório. Para habilitar o login API no desenvolvimento, gerar uma chave aleatória apenas na sessão PowerShell:

```powershell
$bytesJwt = New-Object byte[] 32
$geradorJwt = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$geradorJwt.GetBytes($bytesJwt)
$geradorJwt.Dispose()
$env:Jwt__SigningKey = [Convert]::ToBase64String($bytesJwt)
dotnet run --launch-profile https
```

Não imprimir, compartilhar ou salvar essa chave no Git. Sem a variável, MVC continua funcionando e login API retorna 503. `Jwt:Issuer`, `Jwt:Audience` e `Jwt:ExpirationMinutes` ficam em appsettings.json; também podem ser configurados por variáveis com `__`.

Em Development, acessar `/swagger/index.html` na URL da aplicação. Executar `POST /api/v1/auth/login` com uma conta existente e informar o accessToken no botão Authorize. Usar HTTPS. OpenAPI: `/openapi/v1.json`; documentação desligada em Production.

- [Endpoints, contratos e histórico](docs/Sprint4/API.md)
- [Autenticação, configuração e limitações](docs/Sprint4/AUTENTICACAO.md)
- [Testes e reprodução](docs/Sprint4/TESTES.md)
- [Relatório da Sprint 4](docs/Sprint4/RELATORIO.md)

Regressão completa: `powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint4`. O runner gera sua própria chave efêmera para o processo de teste e limpa seus registros temporários no SQL Server configurado.

## Endereços estruturados (Sprint 5)

Antes de executar esta versão sobre banco anterior, aplicar **somente** a migração incremental `ScriptsSQL/001_enderecos_geolocalizacao.sql`; não recriar o banco com ScriptBD.txt. Ela preserva textos antigos e adiciona componentes de local e snapshots de endereço das reservas.

```powershell
sqlcmd -S 'localhost\SQLEXPRESS' -d Xamou -E -C -b -i ScriptsSQL/001_enderecos_geolocalizacao.sql
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint5
```

ViaCEP usa HTTPS sem chave, timeout de quatro segundos e cache em memória. Consulta e pesquisa são autenticadas e limitadas; preencher manualmente continua possível. Geocoder real não está configurado: latitude/longitude permanecem NULL e não há busca por proximidade.

- [Relatório e endpoints](docs/Sprint5/RELATORIO.md)
- [Modelo e integração](docs/Sprint5/ENDERECOS.md)
- [Migração e modelo físico](docs/Sprint5/MIGRACAO-BANCO.md)
- [Privacidade](docs/Sprint5/PRIVACIDADE.md)
- [Testes](docs/Sprint5/TESTES.md)
- [Checklist visual pendente](docs/Sprint5/CHECKLIST-MANUAL.md)


## Sprint 6 — Android

O aplicativo Android nativo está no projeto existente `C:\Users\Vinicius\AndroidStudioProjects\TRAMPO`. Ele usa Kotlin e Views/XML e consome a API por Retrofit. A documentação, os testes e o roteiro de demonstração estão em [docs/Sprint6/RELATORIO.md](docs/Sprint6/RELATORIO.md).

Para iniciar a API local na porta usada pelo emulador:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File docs/Sprint6/Iniciar-Api.ps1
```

Para executar a integração Kotlin → API → SQL Server com fixtures temporárias:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint6/Run.ps1
```
