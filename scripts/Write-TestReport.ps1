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
$baseline = & git -c "safe.directory=$repoRoot" -C $repoRoot rev-parse --short HEAD 2>$null
if ($LASTEXITCODE -ne 0) { $baseline = 'not available' }
$workingChanges = & git -c "safe.directory=$repoRoot" -C $repoRoot status --porcelain 2>$null
$sourceState = if ($workingChanges) { 'Includes local uncommitted changes; results apply to the tested working tree, not the remote commit alone.' } else { 'Clean working tree at report generation.' }
$lines = @(
    '# Automated backend test results', '',
    "Source base revision at test run: $baseline.",
    $sourceState,
    "Run start (as recorded by TRX): $($times.start).",
    "Run finish (as recorded by TRX): $($times.finish).", '',
    "Total: $($counters.total). Executed: $($counters.executed). Passed: $($counters.passed). Failed: $($counters.failed). Not executed: $($counters.notExecuted).", '',
    'These results were generated from actual xUnit/VSTest output. They cover backend unit/integration checks, including a filter/lookup microbenchmark. They do not measure WinForms grid rendering or establish stakeholder acceptance or phase approval.', '',
    'Raw local evidence: TestResults/backend-tests.trx (ignored by Git). Reproduce with scripts/Test-Backend.ps1. Member 3 must rerun on their own machine before committing their verification record.', '',
    '## Executed cases', ''
)
foreach ($result in ($results | Sort-Object testName)) {
    $name = $result.testName.Replace('`', "'").Replace("`r", ' ').Replace("`n", ' ')
    $lines += "- $name - **$($result.outcome)**; duration $($result.duration)."
    $testOutput = $result.SelectSingleNode('t:Output/t:StdOut', $ns)
    if ($testOutput -and ![string]::IsNullOrWhiteSpace($testOutput.InnerText)) {
        $outputText = $testOutput.InnerText.Trim().Replace("`r", '').Replace("`n", '; ')
        $lines += "  Output: $outputText"
    }
    if ($result.outcome -ne 'Passed' -and $result.Output.ErrorInfo.Message) {
        $lines += "  Error: $($result.Output.ErrorInfo.Message)"
    }
}
$lines += @('', '## Manual and acceptance evidence', '',
    'Assisted UI observations, remaining manual scenarios, stakeholder acceptance, and evidence limitations are recorded in TEST_PLAN.md. Member 3 phase approval is recorded separately in docs/phase-2/PROJECT_LOG.md.')
$encoding = New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllLines([IO.Path]::GetFullPath($OutputFile), [string[]]$lines, $encoding)
Write-Output "Wrote $OutputFile"
