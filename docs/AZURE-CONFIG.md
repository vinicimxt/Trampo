# Configuração do TRAMPO no Azure App Service

Preencher em **App Service > Settings > Environment variables** (Application settings), depois do provisionamento gratuito confirmado. Não gravar valores sensíveis em arquivos, comandos versionados, workflows, logs ou evidências. O App Service reinicia ao aplicar alterações.

| Nome | Obrigatório | Valor/uso sem segredo |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Sim | `Production` |
| `ConnectionStrings__Xamou` | Sim | String do Azure SQL `Xamou`, com servidor `tcp:...database.windows.net,1433`, `Database=Xamou`, autenticação configurada, `Encrypt=True;TrustServerCertificate=False`. O valor completo é segredo. |
| `Jwt__SigningKey` | Sim para API | Base64 de **32 bytes aleatórios ou mais**, exclusivo de Production. Nunca reutilizar a chave local. |
| `Jwt__Issuer` | Não | Padrão `Trampo` em `appsettings.json`; alterar apenas se os clientes acompanharem. |
| `Jwt__Audience` | Não | Padrão `Trampo.Api`. |
| `Jwt__ExpirationMinutes` | Não | Padrão `15`, permitido `1` a `60`. |
| `AllowedHosts` | Recomendado | Host real `<nome>.azurewebsites.net`, após validar o domínio. O padrão atual é `*`. |
| `DOTNET_ENVIRONMENT` | Somente se já existir | Deve ser `Production`, sem conflito com `ASPNETCORE_ENVIRONMENT`. |

Usar **Application settings** para `ConnectionStrings__Xamou`; não duplicar o mesmo nome em Connection strings do portal. O código chama `GetConnectionString("Xamou")`, que lê `ConnectionStrings:Xamou`. Conferir a precedência efetiva se houver outro provedor com a mesma chave.

Para gerar a chave JWT localmente sem exibi-la em terminal ou chat, usar um gerador criptográfico confiável e inserir diretamente no campo protegido do portal. Evitar copiar a chave para documentação ou histórico de shell. Após trocar a chave, tokens anteriores deixam de ser válidos.

Configurações do recurso, fora de variáveis:

- **Stack**: .NET 10; preferir Windows Code deployment para o primeiro deploy e confirmar o runtime na região/plano.
- **HTTPS Only**: On. Verificar `https://<nome>.azurewebsites.net/health` e redirect HTTP → HTTPS.
- **Plano**: Free F1 somente se a tela da assinatura/região exibir explicitamente Free e custo zero. Não aceitar upgrade automático.
- **Azure SQL**: Free tier com `useFreeLimit=true` e `freeLimitExhaustionBehavior=AutoPause`; confirmar essas propriedades após a criação. Sem `BillOverUsage`.
- **Rede SQL**: aprovar regras de firewall limitadas para deploy/administração e a saída real do App Service. Não abrir acesso geral por conveniência.

`/health` é liveness: pode responder 200 mesmo com SQL inacessível ou `Jwt__SigningKey` ausente. Fazer login REST e consulta ao catálogo na verificação final. A sessão MVC é em memória; reinícios podem desconectar usuários. Não habilitar logs de payload, headers `Authorization`, cookies, conexão SQL ou chave JWT.

Fontes: [configuração ASP.NET Core no App Service](https://learn.microsoft.com/azure/app-service/configure-language-dotnetcore), [oferta gratuita Azure SQL](https://learn.microsoft.com/azure/azure-sql/database/free-tier?view=azuresql), [firewall Azure SQL](https://learn.microsoft.com/azure/azure-sql/database/firewall-configure?view=azuresql).
