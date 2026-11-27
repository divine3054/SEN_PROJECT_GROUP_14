# Group 14 - Complaint and Feedback Management System

C# Windows Forms application with customer complaint/feedback submission, categorisation, reference numbers, tracking, and a support-agent workspace. The team uses the Waterfall model.

## Build and run

Requirements: Windows and the .NET 10 SDK, including the desktop runtime. Open this repository folder in your editor. Ctrl+Shift+B builds the app; Terminal > Run Task includes the C# run/check tasks.

From the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action Run
```

The script uses .tools/dotnet if present, otherwise dotnet on PATH. No Live Preview/web server is needed. A first dependency restore needs internet access.

Demonstration accounts: Customer `customer1` / `pass123`, Support Agent `agent1` / `agent123`, Administrator `admin1` / `admin123`. Admin currently opens the support workspace; separate account-management/summary features await the team's scope decision.

## Verification

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Check-BackendReliability.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action UiCheck -Configuration Release
```

The first suite is Member 3's xUnit project; the other commands check backend reliability and native WinForms controls with isolated sample data. Current integration results and remaining submission work are in [the review](docs/INTEGRATION_REVIEW.md).

## Data and recovery

The run script uses the repository root for local users.json, complaints.json and reference-sequence.json. These generated files are ignored by Git. Use one application process per data folder. Successful replacements retain a previous-version .bak; explicit recovery preserves the damaged original. See [Member 2's instructions](docs/MEMBER_2_HANDOFF.md) before restoring data. Do not reset the reference counter.

## Team and report material

- Member 1: Eregha Tarilah - UI, navigation and validation.
- Member 2: Kailota Igwe - backend, storage and reliability.
- Member 3: Chukwuemeka Okoye - test development, QA and documentation.

See [requirements](docs/requirements/SRS.md), [design](docs/phase-2/README.md), [test plan](docs/testing/TEST_PLAN.md), and [submission checklist](docs/POST-CODING-CHECKLIST.md). Recorded phase approval is separate from stakeholder acceptance. The final PDF, demonstration video and final submission evidence are still to be assembled by the team.
