# Roteiro de implantação do TRAMPO

**Estado real:** preparação e validação local. Nenhum serviço cloud, banco gerenciado,
domínio, certificado, secret de produção ou deploy foi criado. Seguir este roteiro
em homologação antes de aprovar produção. CI existe em arquivos locais; CD é planejado.

## Artefato e hospedagem

Executar na raiz do backend:

~~~powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/Test-CI.ps1
~~~

O script restaura/compila os quatro projetos, executa quatro verificações puras,
limpa somente artifacts/backend, publica e executa a suíte Sprint 8 contra o pacote.
Comando de publicação utilizado:

~~~powershell
dotnet publish BD-TRAMPO.csproj -c Release --no-restore --self-contained false -p:UseAppHost=false -o artifacts/backend
~~~

A pasta deve ser limpa antes de uma publicação avulsa para não reter arquivos de
versões anteriores. O script já faz essa limpeza com validação do destino.
O artefato é framework-dependent, sem apphost específico de Windows.
Iniciar no diretório publicado com dotnet BD-TRAMPO.dll.
O destino precisa de ASP.NET Core Runtime 10 compatível e atualizado.
A publicação local foi validada no Windows; Linux não foi executado nesta sessão.
Nenhuma configuração Development/User Secrets acompanha o pacote.

Rota simples proposta: serviço gerenciado para ASP.NET Core + SQL Server gerenciado.
Azure App Service + Azure SQL é um exemplo, não uma implantação realizada.
Confirmar suporte efetivo ao runtime, orçamento, região, rede e disponibilidade antes
de escolher/contratar o serviço. Docker continua dispensável para publicação direta.

## Configuração necessária

Usar configurações protegidas do provedor ou secret store, sem arquivos com senhas
no pacote. Para variáveis ASP.NET Core, dois underscores representam a hierarquia.

| Variável | Uso / requisito |
| --- | --- |
| ASPNETCORE_ENVIRONMENT | Production; se DOTNET_ENVIRONMENT também existir, manter coerente |
| ConnectionStrings__Xamou | Obrigatória no ambiente hospedado; servidor e banco devem existir |
| Jwt__SigningKey | Base64 de pelo menos 32 bytes aleatórios; secret externo, não reutilizar fixtures |
| Jwt__Issuer | Identificador esperado; padrão atual Trampo, alterar de forma coordenada |
| Jwt__Audience | Público esperado; padrão atual Trampo.Api |
| Jwt__ExpirationMinutes | Opcional, padrão 15; validação existente aceita 1 a 60 |
| AllowedHosts | Restringir aos hosts reais; padrão atual ainda é * |
| ASPNETCORE_URLS | Endereço de escuta do Kestrel quando o provedor não o configura |
| ASPNETCORE_HTTPS_PORT | Porta externa HTTPS, somente quando necessária à integração de hosting |

Exemplo **conceitual, não executável**, de ConnectionStrings__Xamou:

~~~text
Server=tcp:<servidor-real>,1433;Database=<banco-real>;User ID=<usuario-aplicacao>;Password=<secret-store>;Encrypt=True;TrustServerCertificate=False;
~~~

Alternativamente, selecionar autenticação por identidade suportada pelo provedor
e pelo Microsoft.Data.SqlClient. Não presumir que a autenticação integrada Windows
local funciona em Linux ou em um banco gerenciado.

Não mostrar variáveis sensíveis em screenshots, logs, terminal gravado ou documentação.
Não usar dotnet user-secrets list como evidência de configuração.
Em Production, User Secrets não são fonte padrão; a chave JWT deve vir do ambiente.
Por compatibilidade histórica, chave JWT ausente NÃO impede o MVC de iniciar:
autenticação API fica indisponível. /health verde não substitui validar login JWT.

## HTTPS, proxy e erros

HTTPS deve ser terminado no serviço gerenciado/proxy ou configurado no Kestrel.
HSTS fora de Development e UseHttpsRedirection foram preservados.
Não há configuração genérica de confiança em proxy adicionada nesta sprint.

Antes de hospedar atrás de proxy, confirmar a integração IIS do serviço ou configurar
Forwarded Headers para os proxies/redes efetivamente confiáveis, ANTES de HTTPS,
autenticação e rate limiting. É necessário preservar esquema HTTPS e IP do cliente;
caso contrário pode ocorrer redirecionamento incorreto e rate limit por IP do proxy.
Não limpar indiscriminadamente KnownProxies/KnownNetworks nem confiar em headers
enviados diretamente pela internet. Esse ajuste depende da topologia ainda inexistente.

Swagger/OpenAPI permanece restrito a Development. A API retorna erro técnico genérico,
sem stack trace. MVC em Production usa a página de erro existente.
Permissões, ownership, antiforgery e rate limiting não foram alterados.

## Banco: checklist de provisionamento

1. Criar banco vazio com a ferramenta/portal do provedor e identidade administrativa
   temporária. Não executar CREATE DATABASE/USE master do script local cegamente no serviço.
2. Revisar ScriptBD.txt. O schema começa em CREATE TABLE Usuarios e termina na
   tabela Assinaturas; os INSERTs seguintes são catálogo de Categorias/Subcategorias.
   Em banco vazio, aplicar esses trechos de schema e catálogo, excluindo CREATE DATABASE,
   USE e os blocos comentados de DROP/debug/admin. Não criar o admin de exemplo.
   O arquivo é bootstrap não idempotente, não um mecanismo de atualização.
3. Aplicar ScriptsSQL/001_enderecos_geolocalizacao.sql no banco selecionado.
   É evolução aditiva, transacional, com guardas para reexecução e sem apagar textos legados.
   Registrar versão e resultado; não executar a criação inicial sobre banco existente.
4. Provisionar usuário/identidade da aplicação com apenas operações de dados necessárias
   (SELECT/INSERT/UPDATE/DELETE nas tabelas usadas). Verificar sys.sp_getapplock usado
   pelos DAOs. Não conceder sysadmin/db_owner nem DDL ao usuário de execução.
   Administração/schema usam uma identidade distinta.
5. Configurar ConnectionStrings__Xamou por secret store, acesso de rede restrito,
   criptografia e certificado SQL válido. TrustServerCertificate=True é apenas local.
6. Configurar backups, retenção e recuperação pontual conforme o provedor; ENSAIAR
   restauração em banco separado e registrar RPO/RTO aprovados.
7. Validar /health, catálogo, login JWT, sessão MVC e uma operação de homologação.
   Não usar Tests/Sprint1 ou Sprint6 em produção: criam e removem fixtures.

Nesta sprint não houve criação/destruição de banco nem aplicação de migrações cloud.
A execução histórica usou apenas fixtures no SQL Express local existente.
Ainda é necessária homologação do schema e das permissões no serviço escolhido.

## Sessão e Data Protection

Hoje AddDistributedMemoryCache armazena sessão na memória do processo.
Reinício perde sessões; mais de uma instância não compartilha esse estado.
Data Protection usa comportamento padrão do host, sem storage explicitamente escolhido.
Mesmo com uma instância hospedada, confirmar persistência das chaves entre reinícios,
deploys e slots; configurar storage protegido e acesso restrito.

Para múltiplas instâncias, planejar chaves compartilhadas/protegidas, mesmo nome de
aplicação e estratégia de sessão compartilhada. Sticky session pode reduzir sintomas,
mas não garante recuperação nem substitui persistência. Redis não foi implementado.
Para a demonstração acadêmica, manter uma instância e registrar essas limitações.

## Monitoramento

GET /health é liveness: HTTP 200, texto Healthy e cache desabilitado quando a
aplicação responde. É anônimo, não consulta SQL e não divulga configuração.
Não indica readiness do banco, disponibilidade do provedor de CEP ou configuração JWT.
O monitor externo deve usar HTTPS; validar como o provedor realiza seu probe interno.

Manter logs ASP.NET existentes (Information geral, Warning para Microsoft.AspNetCore),
capturar saída padrão no serviço e erros 5xx/TraceId. Não registrar Authorization,
cookies, senhas, payloads de login ou connection strings.
O teste novo verifica ausência de um marcador de senha SQL nos logs; isso não é
certificação de todos os logs ou de todo histórico do repositório.
Coletar status/latência HTTP e disponibilidade pelo proxy/provedor com campos restritos,
retenção e controle de acesso. Não foi adicionada captura de corpos HTTP ou stack externa.

## CI versus CD e rollback

CI backend: workflow da Sprint 7 chama Test-CI.ps1, agora com validações Sprint 8
e upload do pacote somente após sucesso. Sem SQL Server real ou secrets de produção no CI.
CI Android: permanece com testes locais, lint e APK Debug.
Os workflows ainda precisam ser commitados/publicados e executados no GitHub.
Nenhuma execução remota desta versão foi comprovada.

CD futuro: escolher ambiente protegido, associar identidade de deploy de menor
privilégio (preferir federação/OIDC onde suportada), aprovar homologação, obter o
artefato da execução CI aprovada e publicar. Não há job fictício exigindo secrets ausentes.
Antes da troca, validar banco/migrações e backup; após a troca, testar HTTPS, /health,
login, ownership e regressão funcional em homologação.
Guardar versão anterior do artefato e plano de reversão compatível com o schema.
Migração de banco não é revertida automaticamente ao restaurar um binário.

## Android Release e fechamento do PIM

Na raiz Android, fornecer -PtrampoApiUrl com a URL HTTPS REAL terminada em barra.
A URL é configuração pública do cliente, não um segredo.
Após definir TRAMPO_API_URL com o endpoint realmente implantado, o comando futuro é:

~~~powershell
.\gradlew.bat assembleRelease "-PtrampoApiUrl=$env:TRAMPO_API_URL"
~~~

Esse comando não configura assinatura e não foi executado com endpoint cloud nesta sprint.
Não há endpoint fictício padrão nem assinatura de distribuição configurada.
HTTP continua permitido apenas para 10.0.2.2 no recurso Debug; a configuração principal
bloqueia cleartext. Não incluir JWT de servidor, credenciais SQL ou chaves cloud no APK.
A revisão de fontes/configuração não encontrou essas credenciais; não foi gerado nem
auditado um APK Release assinado. Play Store permanece fora do escopo.

Para encerrar o PIM: registrar evidências locais e demonstração Web/Mobile; apresentar
o diagrama e explicar liveness versus banco, CI versus CD e proposta versus deploy.
Se a banca exigir cloud efetiva, ainda é preciso escolher/provisionar o serviço,
homologar, implantar e coletar evidências reais. Não substituir essa etapa por screenshots
de configuração local.

## Referências técnicas

- [Health checks ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0)
- [Proxy e load balancer](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0)
- [Configuração Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0)
