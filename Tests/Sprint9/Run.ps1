$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Push-Location $projectRoot
try {
    dotnet build BD-TRAMPO.csproj --no-restore --nologo -o Tests/Sprint1/app
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar a API.' }
    dotnet build Tests/Sprint9/Sprint9.csproj --nologo -p:BuildProjectReferences=false
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar o runner Sprint 9.' }
    dotnet run --project Tests/Sprint9/Sprint9.csproj --no-build --no-launch-profile
    if ($LASTEXITCODE -ne 0) { throw 'Integração Sprint 9 falhou.' }
} finally { Pop-Location }
