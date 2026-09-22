$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $projectRoot
try {
    foreach ($project in @('BD-TRAMPO.csproj', 'Tests/Sprint1/Sprint1.csproj', 'Tests/Sprint6/Sprint6.csproj', 'Tests/Sprint8/Sprint8.csproj')) {
        dotnet restore $project
        if ($LASTEXITCODE -ne 0) { throw "Restore falhou: $project" }
        dotnet build $project -c Release --no-restore --nologo -p:RunAnalyzersDuringBuild=true
        if ($LASTEXITCODE -ne 0) { throw "Build falhou: $project" }
    }
    # Runners existentes sao executaveis; dotnet test nao executaria as verificacoes.
    dotnet run --project Tests/Sprint1/Sprint1.csproj -c Release --no-build --no-launch-profile -- --unit
    if ($LASTEXITCODE -ne 0) { throw 'Testes sem banco falharam.' }
    # Limpa somente a saida conhecida; logs e demais artefatos permanecem.
    $publishPath = [IO.Path]::GetFullPath((Join-Path $projectRoot 'artifacts/backend'))
    $expected = [IO.Path]::GetFullPath((Join-Path $projectRoot 'artifacts')) + [IO.Path]::DirectorySeparatorChar
    if (-not $publishPath.StartsWith($expected, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Destino de publish fora da pasta artifacts.'
    }
    if (Test-Path -LiteralPath $publishPath) { Remove-Item -LiteralPath $publishPath -Recurse -Force }
    dotnet publish BD-TRAMPO.csproj -c Release --no-restore --self-contained false -p:UseAppHost=false -o $publishPath
    if ($LASTEXITCODE -ne 0) { throw 'Publish falhou.' }
    dotnet run --project Tests/Sprint8/Sprint8.csproj -c Release --no-build --no-launch-profile
    if ($LASTEXITCODE -ne 0) { throw 'Verificacoes de hospedagem falharam.' }
} finally { Pop-Location }
