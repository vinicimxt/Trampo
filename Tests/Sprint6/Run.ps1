param([string]$AndroidProject = (Join-Path $env:USERPROFILE 'AndroidStudioProjects\TRAMPO'))
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Push-Location $projectRoot
try {
    New-Item -ItemType Directory -Force -Path docs/Sprint6 | Out-Null
    dotnet build BD-TRAMPO.csproj --no-restore --nologo -o Tests/Sprint1/app
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação da API.' }
    dotnet build Tests/Sprint6/Sprint6.csproj --nologo -p:BuildProjectReferences=false
    if ($LASTEXITCODE -ne 0) { throw 'Falha na compilação do runner.' }
    dotnet run --project Tests/Sprint6/Sprint6.csproj --no-build --no-launch-profile -- $AndroidProject
    if ($LASTEXITCODE -ne 0) { throw 'Falha na integração. Consulte docs/Sprint6/integracao-kotlin.log.' }
} finally { Pop-Location }