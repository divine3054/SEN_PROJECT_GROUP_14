# Member 2 backend verification

Prepared 27 November 2026 against local revision 2fea1c6 plus the uncommitted Member 2 backend changes. This records executed technical checks; it is not a human acceptance signature or Waterfall sign-off.

## Executed results

- Application Release build: succeeded, **0 warnings and 0 errors**, on Windows with .NET SDK 10.0.404.
- New reliability harness: **20 passed, 0 failed**. See [actual results](reliability-results.json).
- Member 3's separately prepared xUnit backend project: **33 passed, 0 failed, 0 skipped** against the new services. Raw local TRX: .tools/member2-regression/member2-backend-regression.trx.
- Member 1's UI regression harness: **15 passed, 0 failed** against the new services.
- StorageRecovery console: Release build succeeded with **0 warnings/errors**. Its preview command left an isolated malformed primary unchanged. The explicit --restore command restored a validated empty-list backup and preserved the malformed original in exactly one .corrupt-* file.
- git diff --check passed. No real complaint/account JSON was written in the repository root by these checks.

The combined automated baseline is 68 passing checks (20 new reliability, 33 existing backend, 15 existing UI), plus the recovery CLI preview/restore exercise. These are not 68 independent unit tests: they include integration, concurrency, UI-control and failure-injection scenarios.

## Reproduction

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action Build -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Check-BackendReliability.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action UiCheck -Configuration Release
```

Where Member 3's project is present, also run dotnet test ComplaintManagementSystem.Tests/ComplaintManagementSystem.Tests.csproj --configuration Release. The console reliability tool is independent of that project and introduces no third-party test packages.

## Reliability coverage

REL-01/02 deliberately fail complaint writes and verify unchanged cache and disk. REL-03 through REL-06 cover migration, deletion, year changes and sequence expansion. REL-07 checks 12 concurrent submissions from separate service instances inside one process. REL-08 through REL-10 check defensive copies and validation. REL-11/12 check backup creation and failed atomic replacement/temporary cleanup. REL-13 through REL-18 check malformed files, explicit backup restoration, missing primaries and schema/identity validation. REL-19 confirms a storage instance retains its original directory. REL-20 checks corrupt/exhausted counters. Recovery also verifies that the persisted reference counter is not rewound when restoring older complaints.

Each reliability scenario runs in a unique temporary directory. A deliberate-failure StorageService subclass is injected through the optional ComplaintService constructor; production uses the default implementation. Existing public UI calls and JSON model fields remain compatible.

## Limits and remaining work

- One application process is supported per data directory. The in-process lock is not a cross-process lock or database transaction.
- Read-only queries use the current service's cache. New services reload storage and writes reload the current list. Live external-file synchronization is not claimed.
- Failed submissions can reserve a number and leave a gap. Issued/reserved numbers are not reused while the healthy ledger is retained. Deleting or rewinding the counter destroys that guarantee; corrupt/missing counters with backups cause an error instead of automatic reset.
- The recovery console requires an explicit --restore action after preview; a previous-version backup can omit the newest changes. It leaves the ledger intact, validates records and preserves the displaced primary. It does not perform automatic or historical-counter recovery.
- The existing UI still conservatively blocks retry until reopening after an I/O error, even though backend cache rollback is now safe. No Member 1 source change was needed.
- Password hashing remains the supplied SHA-256 demonstration behavior; account management, stronger authorization and a strict status-transition policy were not added.
- Member 3 must update the earlier SRS/class diagram/test descriptions to reflect these API and persistence changes. Human end-to-end testing, acceptance evidence and formal approvals remain separate tasks.

See [Member 2 handoff and exact commit commands](../MEMBER_2_HANDOFF.md). Only Member 2 files belong in that commit; the prepared Member 3 contribution must remain separate.
