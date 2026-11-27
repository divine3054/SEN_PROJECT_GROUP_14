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

## 4. Local UI execution record - 27 November 2026

The following exploratory checks were performed in the local WinForms application using the built-in demo accounts. Operator: Copilot via Computer Use, not an independent human stakeholder. The app ran against its local demonstration data; one sample complaint was created and updated. No screenshot files were saved, so these observations are not a substitute for the team's signed acceptance run. The team should repeat the scenarios on the designated demonstration computer and retain screenshots.

- **MAN-01 / FR-01, NFR-01:** Customer `customer1` and Support Agent `agent1` logins opened their respective screens; password characters were masked. A nonmatching password value was rejected with “Invalid username or password.” Actual: observed. Result: **PASS (exploratory UI check)**.
- **MAN-02 / FR-02, FR-04:** Submitted title “Incorrect bill”, Billing, description “Extra charge”. The form displayed `CMP-2026-0001`; the Agent view showed it as Pending after reload. No separate confirmation popup appeared; only the in-form reference label was shown. Actual: observed. Result: **PARTIAL** (core submission/reference passed; popup expectation not met).
- **MAN-03 / FR-02:** Submitted with a blank title and then with a blank description. Both attempts displayed “Title and description are required.” The Agent grid still contained only the original sample ticket. The prior successful reference remained visible on the submit tab after the rejected attempts. Actual: observed. Result: **PARTIAL** (validation/no extra ticket passed; stale success label is a UI defect).
- **MAN-04 / FR-03, FR-06, FR-07:** Filtering for Technical produced an empty grid; Reset restored the Billing ticket. Selecting it populated the edit controls. Saving category Service, status Resolved and notes “Fixed” displayed “Updated CMP-2026-0001.” The grid exposes all complaint properties rather than the six planned columns. Actual: observed. Result: **PARTIAL** (filter/reset/update passed; grid presentation still needs review).
- **MAN-05 / FR-05:** A lowercase, space-padded valid reference showed the complaint details. An unknown reference replaced the prior result with “No complaint found with that reference number.” Blank input followed the same no-match path and did not display a dedicated blank-input alert. Actual: observed. Result: **PARTIAL** (lookup/no-stale-result passed; blank-input alert missing).
- **MAN-06 / FR-08:** Closed and reopened the app after submission; the reference was still trackable. After the agent update, closed/reopened again and the Customer view showed Service, Resolved and “Fixed.” Actual: observed. Result: **PASS (exploratory UI check)**.
- **MAN-07 / NFR-02:** UI startup/save behavior with malformed JSON and an unwritable file was not exercised; doing so safely requires a separately controlled demonstration working directory. Backend STORE05 verifies malformed JSON raises an error and preserves the source file, but does not prove graceful UI recovery. Actual: NOT RUN. Result: **NOT RUN**.
- **PERF-01 / NFR-03:** The latest `PERF01` run measured category/status filtering of 1,000 records at 1.056 ms and lookup of the last reference at 0.285 ms. Environment reported by the test: Windows `10.0.26200`, .NET `10.0.15`, 8 logical CPUs. These are service-level timings on the current test host, not a full WinForms grid-rendering measurement or confirmed measurement on the designated demonstration PC. Result: **BACKEND MEASUREMENT ONLY**; UI performance acceptance remains pending.

## 5. Stakeholder acceptance - NOT RUN

A team reviewer acting as a Customer and another as a Support Agent must walk through the four SRS scenarios. Record their names, feedback, acceptance decisions and any remaining defects. The exploratory UI checks above are not stakeholder acceptance. Member 3's Phase 2 approval and Phase 3 authorization are recorded separately in [PROJECT_LOG.md](../phase-2/PROJECT_LOG.md); stakeholder acceptance remains pending.

## 6. Open issues and exit criteria

The issues below are separated by the evidence that supports them: code inspection, exploratory UI observations, or outstanding team decisions.

- QA-01 (Member 2/Member 1): failed saves can leave cached changes; corrupt JSON/startup or agent-save errors lack graceful UI handling.
- QA-02 (Member 1): UI check confirmed blank tracking input has no dedicated validation alert and the submit confirmation popup is absent.
- QA-03 (Member 1): UI check confirmed the grid exposes all complaint properties rather than the six planned columns; single-selection, disabled-save and multiline-notes behavior still needs reconciliation with the wireframe.
- QA-04 (team): decide whether to retain or defer administrator account management/summary use cases from the earlier design.
- QA-05 (Member 3/team): draft [activity diagram](../phase-2/ACTIVITY-DIAGRAM.md) and [feasibility/schedule/risk material](../phase-2/FEASIBILITY-SCHEDULE-RISKS.md) are prepared. Human-captured screenshots, video, stakeholder acceptance and assembled report remain outstanding.

Exit criteria: automated cases pass; required manual and acceptance cases have actual results; core defects are fixed/retested or explicitly accepted with limitations; final report links are viewable; team approval is recorded with real names and dates.

Framework references: [Microsoft xUnit tutorial](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit), [xUnit parallel execution](https://xunit.net/docs/running-tests-in-parallel).
