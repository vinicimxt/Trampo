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

## Estrutura do projeto

- Controllers → regras e fluxo da aplicação
- Models → entidades do sistema
- DAO → acesso ao banco de dados
- Views → interface do usuário
- ViewModels → comunicação entre controller e view
- wwwroot → arquivos estáticos (CSS, JS e imagens)

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

---


## Como executar

1. Clone o projeto
2. Abra no Visual Studio / VSCode
3. Configure a connection string
4. Execute o script SQL
5. Rode o projeto (dotnet watch run no terminal VSCODE)
