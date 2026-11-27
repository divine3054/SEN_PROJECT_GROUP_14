$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$localSdk = Join-Path $repoRoot '.tools\dotnet\dotnet.exe'
if (Test-Path -LiteralPath $localSdk) { $sdkCommand = $localSdk }
else {
    $installedSdk = Get-Command dotnet -ErrorAction SilentlyContinue
    if (!$installedSdk) { throw 'Install the .NET 10 SDK on Windows, then rerun this command.' }
    $sdkCommand = $installedSdk.Source
}
$env:DOTNET_CLI_HOME = Join-Path $repoRoot '.tools\cli-home'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
Push-Location $repoRoot
try {
    & $sdkCommand run --project tools\BackendReliabilityChecks\BackendReliabilityChecks.csproj --configuration Release -- (Join-Path $repoRoot '.tools\backend-reliability')
    $checkExit = $LASTEXITCODE
}
finally { Pop-Location }
exit $checkExit
