# Relatório de preparação e deploy Azure — TRAMPO

Data: 22/11/2026. **Estado: PREPARADO PARA DEPLOY.** Não foi realizado nem validado deploy público. A CLI `az` não está instalada neste ambiente; não foi possível executar `az --version`, `az account show`, consultar a assinatura, confirmar F1/SQL Free ou criar recursos. Não há URL real para inserir no README.

## Alterações desta preparação

- Auditoria de backend e limites dos clientes em `docs/DEPLOY-AZURE-AUDITORIA.md`.
- Configuração Production sem valores secretos em `docs/AZURE-CONFIG.md`.
- Roteiro de provisionamento, deploy, atualização e testes em `docs/DEPLOY-AZURE.md`.
- `ScriptsSQL/000_azure_schema_catalogo.sql`: corpo do esquema/catálogo de `ScriptBD.txt` adaptado para execução direta em banco Azure SQL novo e vazio. `ScriptBD.txt` e migração 001 permanecem intactos.
- `ScriptsSQL/azure_validacao.sql`: inspeção somente leitura de schema, constraints, índices, migração e catálogo.

## Recursos e resultados

| Campo | Resultado real |
| --- | --- |
| App Service / plano / região | Não criado; Free F1 Windows/.NET 10 é o alvo, disponibilidade e custo por assinatura ainda não verificados |
| Azure SQL / banco / tier / região | Não criado; `Xamou` no Free tier com `AutoPause` é o alvo, elegibilidade ainda não verificada |
| URL Web / URL health | Não existem nesta execução |
| Configuração Production | Especificada, não aplicada; nenhum secret de produção criado ou exposto |
| Schema / migração / catálogo | Revisados estaticamente, não aplicados no Azure |
| Web / CSS / JS / imagens / sessão / antiforgery | Não testados publicamente |
| API / login / JWT / catálogo / agendamentos | Não testados publicamente |
| Android Debug/Release / aparelho real | Não testados contra Azure; repositório Android ausente deste checkout |
| Desktop HTTPS | Não testado contra Azure; repositório Windows Forms ausente deste checkout |
| Custos esperados | F1 e Azure SQL Free podem ser gratuitos dentro de suas condições; custo efetivo **não confirmado** para assinatura/região. Nenhum recurso foi criado. |

## Limitações e próximos marcos

Validação local desta preparação: `dotnet publish BD-TRAMPO.csproj -c Release --no-restore --self-contained false -p:UseAppHost=false` concluiu com código 0, mantendo os avisos de nulabilidade já presentes no backend. O bootstrap foi comparado linha a linha com o corpo de `ScriptBD.txt` e contém 14 `CREATE TABLE` e dois índices únicos explícitos; os guardas adicionais recusam banco diferente de `Xamou` ou schema `dbo` não vazio. Nenhum script SQL foi executado em banco real.

1. Instalar Azure CLI e autenticar com `az login`, ou usar Portal; escolher assinatura e confirmar custo zero de cada recurso antes da criação.
2. Criar apenas recursos comprovadamente gratuitos; conferir `useFreeLimit=true` e `freeLimitExhaustionBehavior=AutoPause` no banco.
3. Aplicar bootstrap/001 no banco vazio e executar validação SQL.
4. Configurar App Service com conexão e chave JWT Production em settings protegidos; publicar artefato Release.
5. Testar URLs públicas, Web, API, Android e Desktop com contas de demonstração. Só então alterar estado para **DEPLOY VALIDADO** e acrescentar `## Demo online` ao README.

Até o marco 4, usar **PREPARADO PARA DEPLOY**. Após URL real responder, usar **DEPLOY REALIZADO**; após testes públicos completos, usar **DEPLOY VALIDADO**. Não inferir SQL/JWT de `/health` 200.

Fontes para tiers: [App Service F1/.NET 10](https://learn.microsoft.com/azure/app-service/quickstart-dotnetcore), [Azure SQL Free e AutoPause](https://learn.microsoft.com/azure/azure-sql/database/free-tier?view=azuresql), [Azure SQL Free FAQ](https://learn.microsoft.com/azure/azure-sql/database/free-tier-faq?view=azuresql).
