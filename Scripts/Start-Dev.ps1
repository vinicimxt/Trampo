$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$previousHttpsPort = $env:ASPNETCORE_HTTPS_PORT
Push-Location $projectRoot
try {
    $env:ASPNETCORE_HTTPS_PORT = $null
    Write-Host 'API: http://localhost:5165/ | Emulador: http://10.0.2.2:5165/'
    Write-Host 'JWT usa User Secrets existentes ou Jwt__SigningKey do ambiente. Nenhuma chave sera gerada.'
    dotnet run --project BD-TRAMPO.csproj --launch-profile http
    if ($LASTEXITCODE -ne 0) { throw 'A API encerrou com erro.' }
} finally {
    $env:ASPNETCORE_HTTPS_PORT = $previousHttpsPort
    Pop-Location
}
