# Requisitos do PIM — matriz final

ADS — PIM IV, projeto TRAMPO. Consolidação em 26/09/2026. O enunciado/rubrica oficial da instituição não foi fornecido: esta matriz cobre os itens solicitados para a Sprint 10, **não certifica aprovação acadêmica**. A organização é uma proposta acadêmica; não há comprovação de empresa real, pesquisa de mercado ou impacto social mensurado.

Situações: Implementado = código existente; Validado = evidência de execução indicada; Documentado = concepção/descrição, sem alegação de implementação; Parcial/Pendente = lacuna explícita.

| Requisito do PIM | Implementação no TRAMPO | Evidência | Situação |
| --- | --- | --- | --- |
| Caracterização da organização | Plataforma acadêmica de intermediação entre clientes e profissionais; papéis Cliente, Profissional e Admin | [README](../README.md), [paridade](MATRIZ-WEB-API-MOBILE.md) | Documentado; caracterização empresarial formal pendente |
| Empreendedorismo/proposta | Descoberta e organização de serviços/agendas; plano Free/Premium simulado | [PagamentoController](../Controllers/PagamentoController.cs), [apresentação](ROTEIRO-APRESENTACAO.md) | Protótipo implementado; viabilidade financeira/mercado não comprovados |
| Responsabilidade social | Potencial de facilitar acesso a profissionais e oportunidades de trabalho | [proposta](../README.md) | Documentado como intenção; sem avaliação de impacto |
| Diversidade | Catálogo de atividades e experiência disponível a diferentes perfis de uso | [SQL catálogo](../ScriptBD.txt) | Parcial; sem estudo com grupos diversos ou política formal validada |
| Acessibilidade | Labels, navegação, recursos VLibras Web; XML/estados/tema Mobile | [layout Web](../Views/Shared/_Layout.cshtml), [UX Mobile](SPRINT-8-5-MOBILE-UX.md) | Parcial; TalkBack, teclado e conformidade integral pendentes |
| Aplicação Web .NET | MVC/Razor, sessão, cliente/profissional/admin | [Controllers](../Controllers), [Views](../Views) | Implementado; regressão local validada |
| Aplicação Mobile | Kotlin/XML, foco cliente | [repositório Android](https://github.com/vinicimxt/TRAMPO-APP), [testes](TESTES-FINAIS.md) | Implementado; build/testes locais validados |
| Aplicação Desktop | Windows Forms C# para o Profissional; login JWT, agendamentos, serviços e locais pela API | [Sprint 12 Desktop](SPRINT-12-DESKTOP.md), [arquitetura](ARQUITETURA-FINAL.md) | Implementado; quatro das oito janelas tiveram captura visual revisada, sem CI remoto confirmado |
| Integração Web/Mobile/Desktop | Mesma API/Services/banco, clientes independentes | [arquitetura](ARQUITETURA-FINAL.md), [runner Kotlin](../Tests/Sprint6/Program.cs), [Sprint 12 Desktop](SPRINT-12-DESKTOP.md) | Implementado; resultados separados em TESTES-FINAIS e SPRINT-12-DESKTOP |
| API | 32 operações REST v1, contratos JSON/JWT | [inventário](API-FINAL.md) | Implementado |
| Orientação a objetos | Entidades, Services, DAOs, encapsulamento; interfaces de providers/TokenStore | [Models](../Models), [providers](../Integrations/Enderecos/Providers.cs) | Implementado; sem necessidade de padrões artificiais |
| Arquitetura | Camadas e DI nos módulos extraídos; MVC legado parcial | [arquitetura final](ARQUITETURA-FINAL.md) | Implementado/documentado com limites |
| Banco lógico/físico | 15 tabelas, chaves, constraints, índices e transações SQL | [banco](BANCO-DE-DADOS.md) | Inspecionado em metadados locais |
| MER | Relacionamentos e cardinalidades em definição textual/Mermaid | [MER final](BANCO-DE-DADOS.md) | Documentado a partir do schema real |
| Procedures/triggers | Nenhum objeto de negócio encontrado; regras em Services/DAO/constraints | [banco](BANCO-DE-DADOS.md) | Não implementado; conferir exigência na rubrica |
| Scripts SQL | Script base e migração incremental 001; catálogo | [ScriptBD](../ScriptBD.txt), [migração](../ScriptsSQL/001_enderecos_geolocalizacao.sql) | Existentes; diferenças/limites documentados |
| Cloud | Arquitetura proposta, config externa, health e artefato Release | [DevOps/Cloud](DEVOPS-E-CLOUD.md), [implantação](IMPLANTACAO.md) | Preparado/documentado; deploy real pendente |
| DevOps | Git, scripts de execução, logs locais, builds reproduzíveis | [Scripts](../Scripts), [DevOps](DEVOPS-E-CLOUD.md) | Implementado |
| CI/CD | Workflows Backend e Android; workflow Desktop local ainda sem repositório Git | [resultado remoto](DEVOPS-E-CLOUD.md), [Sprint 12 Desktop](SPRINT-12-DESKTOP.md) | CI backend passou; CI Android falhou no setup; Desktop sem execução remota; CD não implementado |
| Metodologia ágil | Histórico das sprints consolidado, sem inventar cerimônias | [ágil](AGIL.md) e relatórios preservados | Documentado retrospectivamente |
| Product Backlog | Itens, prioridade, situação e sprint | [backlog](AGIL.md) | Consolidado no fechamento |
| Sprint Backlog | Auditoria/documentação/validação/entrega da Sprint 10 | [Sprint 10](AGIL.md) | Consolidado; bloqueios explícitos |
| Kanban | A Fazer/Em andamento/Concluído do fechamento | [Kanban](AGIL.md) | Representação documental; sem afirmar quadro histórico externo |
| Testes | Runners .NET, Kotlin, lint e integração SQL | [resultados finais](TESTES-FINAIS.md) | Executados conforme tabela, sem dupla contagem |
| Segurança | Hash Identity, JWT, sessão/CSRF, ownership, SQL parametrizado, secrets externos | [auditoria](SEGURANCA-FINAL.md) | Implementado parcialmente; limites registrados |

## Limites para a defesa

A intenção de inclusão não demonstra impacto social. VLibras não certifica acessibilidade. Pipeline com publish não significa deploy contínuo. Artefato Release não comprova produção HTTPS. MER textual deve ser convertido/formalizado conforme o padrão exigido pela instituição, se necessário. Caso procedures, triggers ou cloud real sejam obrigatórios na rubrica, registrar como pendentes: esta sprint não cria funcionalidades para preencher artificialmente o requisito.
