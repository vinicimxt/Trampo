$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $projectRoot
try {
    $testProjects = @('Tests/binding-check/binding-check.csproj',
        'Tests/booking-contract/booking-contract.csproj',
        'Tests/ux-feedback/ux-feedback.csproj')
    foreach ($project in @('BD-TRAMPO.csproj') + $testProjects) {
        dotnet restore $project
        if ($LASTEXITCODE -ne 0) { throw "Restore falhou: $project" }
        dotnet build $project -c Release --no-restore --nologo -p:RunAnalyzersDuringBuild=true
        if ($LASTEXITCODE -ne 0) { throw "Build falhou: $project" }
    }
    # Estes runners sao executaveis e nao exigem banco/credenciais.
    # stabilization-contract exige API local e contas de teste; nao pertence ao CI sem fixtures.
    foreach ($project in $testProjects) {
        dotnet run --project $project -c Release --no-build --no-launch-profile
        if ($LASTEXITCODE -ne 0) { throw "Verificacao falhou: $project" }
    }
    # Limpa somente a saida conhecida; logs e demais artefatos permanecem.
    $publishPath = [IO.Path]::GetFullPath((Join-Path $projectRoot 'artifacts/backend'))
    $expected = [IO.Path]::GetFullPath((Join-Path $projectRoot 'artifacts')) + [IO.Path]::DirectorySeparatorChar
    if (-not $publishPath.StartsWith($expected, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Destino de publish fora da pasta artifacts.'
    }
    if (Test-Path -LiteralPath $publishPath) { Remove-Item -LiteralPath $publishPath -Recurse -Force }
    dotnet publish BD-TRAMPO.csproj -c Release --no-restore --self-contained false -p:UseAppHost=false -o $publishPath
    if ($LASTEXITCODE -ne 0) { throw 'Publish falhou.' }
    if (Test-Path -LiteralPath (Join-Path $publishPath 'appsettings.Development.json')) {
        throw 'Configuração Development não pode acompanhar o publish.'
    }
    if (-not (Test-Path -LiteralPath (Join-Path $publishPath 'BD-TRAMPO.dll')) -or
        -not (Test-Path -LiteralPath (Join-Path $publishPath 'wwwroot'))) {
        throw 'Publish incompleto: DLL ou arquivos estáticos ausentes.'
    }
} finally { Pop-Location }
