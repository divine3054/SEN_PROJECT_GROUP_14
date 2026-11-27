param(
    [string]$ResultsFile = (Join-Path $PSScriptRoot '..\TestResults\backend-tests.trx'),
    [string]$OutputFile = (Join-Path $PSScriptRoot '..\docs\testing\TEST_RESULTS.md')
)
$ErrorActionPreference = 'Stop'
[xml]$trx = Get-Content -Raw -LiteralPath $ResultsFile
$ns = New-Object Xml.XmlNamespaceManager($trx.NameTable)
$ns.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')
$counters = $trx.SelectSingleNode('//t:ResultSummary/t:Counters', $ns)
$results = @($trx.SelectNodes('//t:UnitTestResult', $ns))
if (!$counters -or [int]$counters.total -eq 0 -or $results.Count -eq 0) {
    throw 'No executed test results were found; an empty run is not a passing suite.'
}
$times = $trx.SelectSingleNode('//t:Times', $ns)
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$baseline = & git -C $repoRoot rev-parse --short HEAD 2>$null
if ($LASTEXITCODE -ne 0) { $baseline = 'not available' }
$lines = @(
    '# Automated backend test results', '',
    "Source revision before this uncommitted contribution: $baseline.",
    "Run start (as recorded by TRX): $($times.start).",
    "Run finish (as recorded by TRX): $($times.finish).", '',
    "Total: $($counters.total). Executed: $($counters.executed). Passed: $($counters.passed). Failed: $($counters.failed). Not executed: $($counters.notExecuted).", '',
    'These results were generated from actual xUnit/VSTest output. They cover backend unit/integration checks, not WinForms system testing, performance, stakeholder acceptance or phase approval.', '',
    'Raw local evidence: TestResults/backend-tests.trx (ignored by Git). Reproduce with scripts/Test-Backend.ps1. Member 3 must rerun on their own machine before committing their verification record.', '',
    '## Executed cases', ''
)
foreach ($result in ($results | Sort-Object testName)) {
    $name = $result.testName.Replace('`', "'").Replace("`r", ' ').Replace("`n", ' ')
    $lines += "- $name - **$($result.outcome)**; duration $($result.duration)."
    if ($result.outcome -ne 'Passed' -and $result.Output.ErrorInfo.Message) {
        $lines += "  Error: $($result.Output.ErrorInfo.Message)"
    }
}
$lines += @('', '## Manual and acceptance evidence', '',
    'MAN-01 through MAN-07, PERF-01 and stakeholder acceptance remain NOT RUN. See TEST_PLAN.md. No human tester signature or Member 3 approval is inferred from this automated run.')
$encoding = New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllLines([IO.Path]::GetFullPath($OutputFile), [string[]]$lines, $encoding)
Write-Output "Wrote $OutputFile"
