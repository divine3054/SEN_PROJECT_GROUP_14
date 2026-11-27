param([ValidateSet('Debug', 'Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$localSdk = Join-Path $repoRoot '.tools\dotnet\dotnet.exe'
if (Test-Path -LiteralPath $localSdk) { $dotnetCommand = $localSdk }
else {
    $installedSdk = Get-Command dotnet -ErrorAction SilentlyContinue
    if (!$installedSdk) { throw 'Install the .NET 10 SDK on Windows, then rerun this script.' }
    $dotnetCommand = $installedSdk.Source
}
$env:DOTNET_CLI_HOME = Join-Path $repoRoot '.tools\cli-home'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = '0'
Push-Location $repoRoot
try {
    & $dotnetCommand build ComplaintManagementSystem.sln --configuration $Configuration --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Solution build failed; tests were not run.' }
    # A unique run folder prevents an old passing TRX file from being reported if
    # the runner fails before writing a fresh result.
    $runDirectory = Join-Path $repoRoot ('TestResults\' + [Guid]::NewGuid().ToString('N'))
    & $dotnetCommand test ComplaintManagementSystem.Tests\ComplaintManagementSystem.Tests.csproj --configuration $Configuration --no-build --logger 'trx;LogFileName=backend-tests.trx' --results-directory $runDirectory --nologo
    $testExitCode = $LASTEXITCODE
    $freshResult = Join-Path $runDirectory 'backend-tests.trx'
    if (!(Test-Path -LiteralPath $freshResult)) { throw 'The test runner produced no fresh TRX results.' }
    Copy-Item -LiteralPath $freshResult -Destination (Join-Path $repoRoot 'TestResults\backend-tests.trx') -Force
    & (Join-Path $PSScriptRoot 'Write-TestReport.ps1')
    if ($testExitCode -ne 0) { throw "Backend tests failed (exit code $testExitCode); inspect TEST_RESULTS.md." }
}
finally { Pop-Location }
