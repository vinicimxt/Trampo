param([switch]$Sprint2, [switch]$Sprint3, [switch]$Sprint4)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$previousDll = $env:TRAMPO_TEST_DLL
Push-Location $projectRoot
try {
    # Saída isolada para não disputar o executável aberto pelo Visual Studio.
    dotnet build BD-TRAMPO.csproj --no-restore --nologo -o Tests/Sprint1/app
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação da aplicação.' }
    dotnet build Tests/Sprint1/Sprint1.csproj --nologo -p:BuildProjectReferences=false
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação dos testes.' }
    Copy-Item -LiteralPath Tests/Sprint1/app/BD-TRAMPO.dll -Destination Tests/Sprint1/bin/Debug/net10.0/BD-TRAMPO.dll -Force
    $env:TRAMPO_TEST_DLL = Join-Path $projectRoot 'Tests/Sprint1/app/BD-TRAMPO.dll'
    if ($Sprint4) {
        dotnet run --project Tests/Sprint1/Sprint1.csproj --no-build --no-launch-profile -- --sprint4
    } elseif ($Sprint3) {
        dotnet run --project Tests/Sprint1/Sprint1.csproj --no-build --no-launch-profile -- --sprint3
    } elseif ($Sprint2) {
        dotnet run --project Tests/Sprint1/Sprint1.csproj --no-build --no-launch-profile -- --sprint2
    } else {
        dotnet run --project Tests/Sprint1/Sprint1.csproj --no-build --no-launch-profile
    }
    if ($LASTEXITCODE -ne 0) { throw 'Falha nos testes.' }
} finally {
    $env:TRAMPO_TEST_DLL = $previousDll
    Pop-Location
}