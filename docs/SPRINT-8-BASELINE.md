# Baseline Sprint 8 — 22/09/2026

Auditoria antes das edições. Alterações e 521 exclusões do índice da Sprint 7 ainda
pendentes de commit; preservadas. Workflows locais ainda não versionados no HEAD.
GitHub CLI não disponível; Android sem repositório/remoto. Nenhuma execução remota
da nova CI realizada.

- Scripts/Test-CI.ps1 aprovado: restore/build Release dos três projetos, 4
  verificações sem banco e publish. Build incremental sem avisos novos;
  a recompilação da Sprint 7 documentou 109 avisos de nulabilidade.
- Tests/Sprint1/Run.ps1 -Sprint5: 288 verificações aprovadas, zero falhas.
- Android testDebugUnitTest lintDebug assembleDebug --rerun-tasks: aprovado;
  24 testes descobertos, 23 aprovados, 1 ignorado (LiveApiTest), zero falhas.
  Lint: zero erros, 10 avisos; aviso de acesso nativo Robolectric/Java 25.
- Conexao usava string fixa SQL Express/Windows. DAOs instanciam Conexao diretamente.
- JWT usa configuração padrão ASP.NET Core e User Secrets em Development.
  Chave ausente deixa API de autenticação indisponível; MVC continua disponível,
  comportamento coberto pelos testes históricos. Chave malformada falha no startup.
- Session usa AddDistributedMemoryCache: armazenamento em memória do processo.
  Não há persistência/compartilhamento explícito de chaves Data Protection.
- Swagger restrito a Development. HSTS fora de Development e redirecionamento
  HTTPS presentes. Não existe configuração explícita de proxy encaminhado.
- Erros técnicos da API retornam mensagem genérica; log inclui exceção/TraceId.
  Conexao antiga concatenava detalhes SQL em exceções.
- ScriptBD.txt: criação não idempotente de banco/schema e seeds de catálogo.
  Blocos DROP, debug e administração (incluindo senha fraca de exemplo) estão
  COMENTADOS. Não são rotina de produção e não devem ser descomentados.
  ScriptsSQL/001_enderecos_geolocalizacao.sql é migração aditiva transacional.
- Android Release sem URL padrão, exige HTTPS; cleartext restrito a 10.0.2.2
  em Debug; assinatura de distribuição não configurada. Sem alterações planejadas.

Evidências locais ignoradas: artifacts/sprint8-baseline-ci.log,
artifacts/sprint8-baseline-integration.log e, no Android, sprint8-baseline.log.
