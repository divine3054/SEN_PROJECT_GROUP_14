# Test plan - Member 3

Prepared 26 November 2026 against application commit 8e4b9d5. Requirements source: [SRS](../requirements/SRS.md). This plan separates automated backend checks from tests that require the running WinForms application.

## 1. Roles and approach

Member 3 owns test code, test execution records, defect reporting and requirements traceability. Member 1 owns UI fixes and Member 2 owns backend fixes. A passing backend suite does not approve the UI, performance or the Waterfall phase gate.

The xUnit project references the real application project. Each test runs in its own temporary data directory. Assembly parallelism is disabled because StorageService uses relative filenames and process-wide current-directory changes must not overlap. Fixtures restore the directory and remove only the generated temporary folder.

Prerequisites: Windows, .NET 10 SDK, and NuGet network access for the first restore. The local .tools/dotnet installation is supported by the runner script; other contributors can use an SDK on PATH.

## 2. Run and capture evidence

From the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1
```

This builds the solution, executes the test project, saves the actual TRX result in ignored TestResults/backend-tests.trx and updates docs/testing/TEST_RESULTS.md. ExecutionPolicy Bypass applies only to that process; it does not change the machine's policy. If local policy prohibits it, use an approved script-launch method or run the equivalent dotnet commands below.

With a normal SDK installation:

```powershell
dotnet build ComplaintManagementSystem.sln --configuration Release
dotnet test ComplaintManagementSystem.Tests/ComplaintManagementSystem.Tests.csproj --configuration Release --logger "trx;LogFileName=backend-tests.trx" --results-directory TestResults
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Write-TestReport.ps1
```

Use the script's nonzero exit code to detect failure. Read failed assertions and file a defect; do not edit recorded results to make a failure look like a pass.

## 3. Automated cases: inputs and expected outputs

Theory cases run once per listed input; their test identifiers appear in the generated result record.

- **AUTH01 / FR-01:** Seeded Customer, SupportAgent and Admin credentials -> authenticated user with the matching role.
- **AUTH02 / FR-01:** Uppercase CUSTOMER1 with correct password -> same valid customer.
- **AUTH03 / FR-01:** Wrong password, unknown user, wrong password case, or empty credentials -> null.
- **SUB01 / FR-02, FR-08:** Padded title/description and Billing category -> trimmed persisted record, correct submitter, Pending status, initial review notes and valid timestamp.
- **SUB02 / FR-02, NFR-02:** Empty or whitespace-only title/description -> ArgumentException and no complaint in memory or storage.
- **CAT01 / FR-03:** Each of the four defined categories -> persisted category matches the selection, including GeneralFeedback.
- **REF01 / FR-04, FR-08:** Three submissions, recreating the service before the third -> distinct references ending 0001, 0002 and 0003 for the current year.
- **TRACK01 / FR-05:** Lowercase reference with surrounding spaces -> correct complaint.
- **TRACK02 / FR-05:** Unknown, empty or whitespace reference -> null. This backend outcome does not verify a UI validation alert.
- **FILTER01 / FR-06:** Three mixed-category/status tickets -> correct all/category/status/combined results and an empty result when none match.
- **FILTER02 / FR-06:** Two records with explicit different creation dates -> newest first.
- **UPDATE01 / FR-03, FR-07, FR-08:** Existing reference, Resolved status, new category and padded notes -> correct changes survive service restart; creation time unchanged and update time current.
- **UPDATE02 / FR-07:** Whitespace notes following nonblank saved notes -> previous notes retained.
- **UPDATE03 / FR-07:** Unknown reference -> false and persisted JSON unchanged.
- **STORE01 / FR-08:** Missing complaints.json -> empty list.
- **STORE02 / FR-08:** Unicode, multiline and quoted complaint details with timestamps -> every persisted field round-trips unchanged.
- **STORE03 / FR-08, NFR-06:** Missing users.json followed by reload -> exactly three persisted demo accounts with stable IDs and no plaintext password in JSON.
- **STORE04 / FR-08:** Save one explicitly supplied agent account -> subsequent load returns that account without reseeding defaults.
- **UNIT01 / NFR-06:** Hash the known UTF-8 string abc -> expected SHA-256 digest encoded as Base64. This is an isolated unit test.
- **STORE05 / NFR-02:** Malformed complaints.json -> JsonException and source file preserved. This records the storage contract; graceful application recovery is still missing.

Most service/storage cases are integration tests because they combine business logic, serialization and the filesystem. Do not describe them all as isolated unit tests.

## 4. Manual system tests - NOT RUN

Run in a separate demonstration folder using sample accounts/data. For every case record tester, date, input, expected result, actual result, pass/fail and screenshot reference. Do not overwrite real users/complaints to reset a demonstration.

- **MAN-01 / FR-01, NFR-01:** Log in once with each role, then with wrong credentials. Expected: correct screen for each role, masked password, useful failed-login feedback. Actual: NOT RUN. Result: NOT RUN.
- **MAN-02 / FR-02, FR-04:** Customer submits title Incorrect bill, Billing, description Extra charge. Expected: visible reference and persisted Pending complaint. Earlier wireframe also requests a confirmation popup; verify it separately, since current code only sets a label. Actual: NOT RUN. Result: NOT RUN.
- **MAN-03 / FR-02:** Submit blank title and blank description separately. Expected: validation feedback with no extra record. Actual: NOT RUN. Result: NOT RUN.
- **MAN-04 / FR-03, FR-06, FR-07:** Agent filters, resets, selects a ticket and saves Resolved/Service/Fixed. Expected: correct filter/reset results, clear selection, saved update. Verify empty-grid behavior, selection controls and notes layout. Actual: NOT RUN. Result: NOT RUN.
- **MAN-05 / FR-05:** Track valid, lowercase/padded, unknown and blank references. Expected: correct details for hits, no stale result for misses, alert for blank input. The current blank-input alert is a known gap. Actual: NOT RUN. Result: NOT RUN.
- **MAN-06 / FR-08:** Close and reopen the app after submission and after an update. Expected: reference and changed status/notes retained. Actual: NOT RUN. Result: NOT RUN.
- **MAN-07 / NFR-02:** In an isolated data folder, simulate malformed JSON and an unwritable complaints file. Expected target: useful error and no false success. Current startup/save recovery needs developer work. Actual: NOT RUN. Result: NOT RUN.
- **PERF-01 / NFR-03:** Use 1,000 sample tickets and time search/filter on the demo PC. Expected proposed target: within 2 seconds. Record CPU/RAM and actual measurements. Actual: NOT RUN. Result: NOT RUN.

## 5. Acceptance tests - NOT RUN

A team reviewer acting as a Customer and another as a Support Agent walk through the four SRS scenarios. Record their names, feedback, acceptance decisions and any remaining defects. This is stakeholder acceptance, separate from xUnit results. No acceptance signature or Phase 2 sign-off has been supplied by preparing these files.

## 6. Open issues and exit criteria

Known by code inspection, not marked as executed manual-test failures:

- QA-01 (Member 2/Member 1): failed saves can leave cached changes; corrupt JSON/startup or agent-save errors lack graceful UI handling.
- QA-02 (Member 1): blank tracking input is not explicitly rejected; submit confirmation popup is absent.
- QA-03 (Member 1): grid auto-generates all complaint properties rather than the six planned columns; single-selection/disabled-save/multiline-notes behavior needs reconciliation with the wireframe.
- QA-04 (team): decide whether to retain or defer administrator account management/summary use cases from the earlier design.
- QA-05 (Member 3/team): activity diagram, feasibility/schedule/risks, screenshots, video and assembled report remain separate deliverables.

Exit criteria: automated cases pass; required manual and acceptance cases have actual results; core defects are fixed/retested or explicitly accepted with limitations; final report links are viewable; team approval is recorded with real names and dates.

Framework references: [Microsoft xUnit tutorial](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit), [xUnit parallel execution](https://xunit.net/docs/running-tests-in-parallel).
