# Member 1 UI verification

Verified 26 November 2026 on Windows against branch `member1-ui-completion` based on Git main revision `8e4b9d5`.

## Results

- Application Release build: succeeded.
- UI harness: **15 passed, 0 failed**.
- Automated UI checks produced six rendered form images and machine-readable results.
- Source diagnostics: no errors in the edited C# files.
- `git diff --check`: passed.

This machine check does not replace the required human walkthrough. Actual popup interaction and cross-login restart verification remain for Tarilah to confirm before pull-request approval.

## Commands

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action Build -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action UiCheck -Configuration Release
```

The first local run requires .NET 10 Desktop Runtime. This machine has .NET 11 installed, and the passing UI run used `DOTNET_ROLL_FORWARD=Major` for local validation.

## Evidence

- [Machine-readable results](ui-check-results.json)
- [Login screenshot](screenshots/01-login.png)
- [Customer submission screenshot](screenshots/02-customer-submit.png)
- [Customer tracking screenshot](screenshots/03-customer-track.png)
- [Agent workspace screenshot](screenshots/04-agent.png)
- [Customer minimum-size screenshot](screenshots/05-customer-minimum.png)
- [Agent minimum-size screenshot](screenshots/06-agent-minimum.png)

## Remaining limits

Backend save rollback and durable reference allocation remain Member 2 responsibilities. Account-management screens and human acceptance remain outside this implementation. Member 3 must update its report and acceptance records only after manual walkthrough evidence is gathered.
