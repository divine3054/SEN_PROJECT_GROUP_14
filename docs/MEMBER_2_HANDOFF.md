# Member 2 - Backend reliability completion

Owner: Kailota Igwe (Member 2). Prepared 27 November 2026 on top of Member 1 commit 2fea1c6. This is your follow-up coding contribution. Member 3's pending requirements, test project and documentation remain separate.

## 1. Changes

- StorageService captures an absolute data directory at construction, while preserving the existing filenames and JSON schema.
- Writes serialize into a unique temporary file, flush it, and replace the primary atomically. The previous successful primary is retained as `<filename>.bak`. The initial write moves a new file into place; no backup exists until a later successful replacement.
- Save/parse failures propagate to the existing UI error handlers. Invalid/null records, duplicate complaint references/usernames and undefined enum values are rejected.
- Missing primaries with existing backups cause a recovery error rather than silent empty data or reseeded users.
- Explicit complaints/users backup restoration validates the backup before replacement and preserves the displaced primary as `.corrupt-*`.
- ComplaintService stages submission/update changes separately and publishes them to its cache only after a successful persistent save. Returned objects are copies, so forms/callers cannot mutate cached records accidentally.
- The new `reference-sequence.json` counter preserves the highest reserved number for each year, reconciles it with existing references and does not reuse a number after a ticket is removed. Existing installations migrate on first successful allocation without editing their stored tickets.
- Reference reservation is durable before ticket save. A failed ticket write can leave a gap; a successful write never reports the failed ticket as saved. The original sequential behavior is retained for successful submissions without failures.
- A directory-scoped process lock covers complaint read/modify/write operations across service instances inside one process. Before writing, each service reloads the current persisted list to avoid overwriting another service instance's successful submission.
- Hashing, model fields, seeded accounts, existing UI calls and normal status/category behavior remain compatible. Additional validation covers blank submitters, null/empty login/reference inputs and undefined enums.

## 2. Files in your package

- ComplaintManagementSystem/Data/StorageService.cs
- ComplaintManagementSystem/Services/ComplaintService.cs
- tools/BackendReliabilityChecks/BackendReliabilityChecks.csproj
- tools/BackendReliabilityChecks/Program.cs
- tools/StorageRecovery/StorageRecovery.csproj
- tools/StorageRecovery/Program.cs
- scripts/Check-BackendReliability.ps1
- docs/MEMBER_2_HANDOFF.md
- docs/member-2/VERIFICATION.md
- docs/member-2/reliability-results.json

No Member 1 screen or Member 3 pending file is included. The two console tools reference the application project; they need no third-party test packages or solution changes. They are meaningful regression/recovery code accompanying your production changes.

## 3. Build and verify

From the repository root, on Windows with the .NET 10 SDK:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action Build -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Check-BackendReliability.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action UiCheck -Configuration Release
```

The scripts detect the workspace-local SDK, otherwise use dotnet on PATH. The execution policy option is local to that launched process. If the machine disallows it, use an approved script-launch method or the equivalent dotnet commands.

The reliability runner checks 20 scenarios with unique temporary data directories. It writes `.tools/backend-reliability/results.json`; this is ignored generated output. The prepared result is copied to docs/member-2/reliability-results.json. Review your own run and refresh that record before committing any changes you make.

With Member 3's prepared project present, also run:

```powershell
dotnet test ComplaintManagementSystem.Tests/ComplaintManagementSystem.Tests.csproj --configuration Release
```

Prepared verification: Release build with 0 warnings/errors, 20 reliability checks, 33 existing xUnit backend tests and 15 UI checks passing. See VERIFICATION.md for evidence and limits. Passing automated checks is not a human acceptance signature.

## 4. Explicit recovery tool

Close the application first. Its data folder is the working directory used when it was launched; scripts/Run-Project.ps1 uses the repository root. Keep the whole data folder, including the reference counter, together.

First inspect the backup path without changing data (replace the example directory with the actual data folder):

```powershell
dotnet run --project tools/StorageRecovery/StorageRecovery.csproj -- "C:\path\to\data" complaints
```

This is preview-only. Inspect `complaints.json.bak` in your editor. It is the previous saved version and can omit the newest changes. If you choose to restore that version:

```powershell
dotnet run --project tools/StorageRecovery/StorageRecovery.csproj -- "C:\path\to\data" complaints --restore
```

Use `users` instead of `complaints` for account-file recovery. On this workspace, if dotnet is not on PATH, replace `dotnet` with `.\.tools\dotnet\dotnet.exe`.

The tool validates before replacing, retains any existing primary in a unique .corrupt-* file, preserves the backup, and returns a nonzero exit code on failure. Reopen the app afterward to reload recovered records. The counter is deliberately not rewound by complaints recovery.

Do not delete/reset/restore an older reference counter to make an error disappear. Missing/corrupt counters with backups need an operator review against existing tickets and saved counter history; automatic counter recovery is deliberately unsupported because an old watermark can reuse historically issued numbers.

## 5. Commit only Member 2 work

Merge Member 1's pull request into shared main first when possible. Local main currently already includes their commit; if their PR is still pending, your branch also carries that dependency and it must be explained in the PR. Do not push local main merely to bypass their review.

From the current prepared workspace:

```powershell
git status
git switch -c member2-backend-reliability
git config user.name
git config user.email
git diff --check
git add ComplaintManagementSystem/Data/StorageService.cs ComplaintManagementSystem/Services/ComplaintService.cs tools/BackendReliabilityChecks tools/StorageRecovery scripts/Check-BackendReliability.ps1 docs/MEMBER_2_HANDOFF.md docs/member-2
git diff --cached --stat
git diff --cached
git commit -m "Member 2: Make complaint persistence and references reliable"
git push -u origin member2-backend-reliability
git log -1 --format=fuller
git status
```

Verify the commit identity is your own verified GitHub identity. Open a pull request into main, include actual results and the Member 1 dependency, and have a teammate review it. The working tree can still show Member 3's unrelated pending files after your commit; that is expected. Do not use `git add .` or claim those files as your contribution.

## 6. Remaining boundaries for the report

- One application process is supported. The lock is not a cross-process/distributed database transaction. Do not run two copies of the desktop app against the same data directory.
- Read-only lookups use the service's snapshot. A new service instance reloads files; existing forms share one service. Dynamic external-file synchronization is not implemented.
- A backup restore intentionally returns a previous version. The recovery tool requires the user to choose it; corrupt data is never silently treated as empty.
- Model/database migrations, stricter role authorization, administrator account management, salted password authentication and a status transition policy are separate scope choices, not claimed by this change.
- Member 3 must update the SRS/class diagram/test plan to describe the new constructor option, backup/recovery methods, persisted counter, copy semantics and actual test results. Their existing package described the older baseline.
- Human end-to-end walkthrough, video/report compilation and formal team sign-off remain to be completed by the responsible members.
