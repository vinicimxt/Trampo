# Testes — Sprint 5

## Resultado final

| Grupo | Aprovadas |
|---|---:|
| Sprint 1 | 66 |
| Sprint 2 | 25 |
| Sprint 3 | 34 |
| Sprint 4 | 93 |
| Sprint 5 | 70 |
| TOTAL | 288 |
| FALHAS | 0 |

Execução final na retomada: testes-final.log. Os 218 testes anteriores foram preservados. Os registros temporários foram removidos ao terminar. Rebuild completo: 109 warnings, zero erros (build-final.log e warnings.csv); testes compilam sem warnings. O runner incremental pode mostrar zero avisos por não recompilar a aplicação: a contagem válida é a do rebuild.

## Reprodução

Aplicar antes ScriptsSQL/001_enderecos_geolocalizacao.sql no banco de teste existente. Requer .NET 10, SQL Server configurado por conexao.cs e portas 5177/5178 livres. Na raiz:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tests/Sprint1/Run.ps1 -Sprint5
```

A política Bypass vale apenas para o processo. A suíte cria contas identificadas por GUID, locais, ofertas e reservas temporárias; limpa somente os IDs dessa execução. Identity pode avançar. Usa chave JWT efêmera e um servidor HTTP simulado em loopback, sem chamadas ao ViaCEP real na regressão. O override de base URL é permitido somente para loopback em Development; produção aceita a URL fixa HTTPS do ViaCEP.

## Cobertura

Normalização e formatos inválidos de CEP/UF; limites de campos e pesquisa; consulta válida/inexistente; timeout, indisponibilidade e JSON/resposta inválidos; cache positivo e ausência de cache de falhas; cadastro manual; geocoder não configurado e resultados válidos/inválidos de provider substituível.

Banco/API: local estruturado, edição própria, recusa de outro profissional, legado com NULLs e texto intacto, conversão manual do legado, coordenadas do cliente recusadas, constraint SQL de limites, snapshots Local/Domicilio e Online sem snapshot. Mudança de endereço numa nova reserva não muda a anterior. Falha SQL na inserção do snapshot reverte a reserva e mantém contagens de snapshots/notificações, comprovando atomicidade.

Privacidade e segurança: JWT obrigatório para consultas; terceiro não acessa domicílio; DTO próprio sem campos brutos do fornecedor ou coordenadas de cliente; rate limiting de consultas inclusive em hits de cache; erros HTTP 400/404/502/503/504 controlados. OpenAPI inclui os novos endpoints.

MVC: consulta com sessão, formulário de local com controles assistidos, gravação estruturada por POST e campos físicos somente na modalidade Domicilio. Antiforgery e demais regressões são preservados pelos testes anteriores.

## Migração e fornecedor real

A migração foi aplicada e reexecutada com sucesso. Contagens/checksums dos campos legados comparados permaneceram iguais antes/depois. Logs estão em migracao.log, migracao-reexecucao.log, banco-antes.txt e banco-depois.txt.

Separadamente, duas consultas opcionais diretas ao ViaCEP real tiveram sucesso: CEP público 01001000 e pesquisa pública SP/Sao Paulo/Paulista. Evidência em fornecedor-real.log. A pesquisa real retornou 50 resultados; o adaptador do TRAMPO limita a saída a 20. Essas verificações não integram a contagem determinística de 288 testes nem representam geocodificação.

## Limites

Não houve inspeção visual: a automação de navegador falhou ao iniciar. Checklist em CHECKLIST-MANUAL.md. Testes HTTP/HTML não substituem inspeção responsiva, teclado e leitor de tela. Não foi feito teste de carga, geocodificação real, busca por distância, recuperação de backup ou exaustão de todas as falhas de rede. A verificação de preservação por checksum não é uma prova criptográfica nem backup.
