param(
    [ValidateSet('Build', 'Run', 'UiCheck')][string]$Action = 'Run',
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug'
)
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
    switch ($Action) {
        'Build' { & $sdkCommand build ComplaintManagementSystem\ComplaintManagementSystem.csproj --configuration $Configuration --nologo }
        'Run' { & $sdkCommand run --project ComplaintManagementSystem\ComplaintManagementSystem.csproj --configuration $Configuration }
        'UiCheck' { & $sdkCommand run --project tools\UiSmokeChecks\UiSmokeChecks.csproj --configuration $Configuration -- (Join-Path $repoRoot '.tools\ui-checks') }
    }
    $runExit = $LASTEXITCODE
}
finally { Pop-Location }
exit $runExit
