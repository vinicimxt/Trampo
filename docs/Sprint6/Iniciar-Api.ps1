$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$previousKey = $env:Jwt__SigningKey
$previousHttpsPort = $env:ASPNETCORE_HTTPS_PORT
Push-Location $projectRoot
try {
    if ([string]::IsNullOrWhiteSpace($env:Jwt__SigningKey)) {
        $bytes = New-Object byte[] 32
        $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
        try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
        $env:Jwt__SigningKey = [Convert]::ToBase64String($bytes)
    }
    $env:ASPNETCORE_HTTPS_PORT = $null
    Write-Host 'API local em http://localhost:5165. No emulador: http://10.0.2.2:5165.'
    Write-Host 'A chave JWT existe somente no processo. Use Ctrl+C para encerrar.'
    dotnet run --project BD-TRAMPO.csproj --launch-profile http
    if ($LASTEXITCODE -ne 0) { throw 'A API encerrou com erro.' }
} finally {
    $env:Jwt__SigningKey = $previousKey
    $env:ASPNETCORE_HTTPS_PORT = $previousHttpsPort
    Pop-Location
}
