# Member 1 handoff - WinForms UI completion

Owner: Eregha Tarilah (Member 1). This implementation completes the UI role work on a dedicated follow-up branch. It is not a Team approval or signature.

## Changes

- Login: labelled inputs, masked password, blank-input feedback, trimmed usernames, cleared failed passwords and keyboard login.
- Customer: submission validation, returned reference confirmation, blank and unknown tracking feedback, read-only result fields and agent notes.
- Agent: six explicit read-only grid columns, combined filters, refresh/reset, selected-row editing and save confirmation.
- Save failure: visible error feedback, retained customer input and blocked unsafe retries in the same service instance.
- Startup: shared service, customer/support/admin routing, unsupported-role rejection and readable startup JSON errors.
- Editor: build, run and UI-check tasks replace Live Preview.
- Testing: native WinForms UI smoke harness with isolated JSON fixtures and rendered form screenshots.

## Verification

Run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action Build -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action UiCheck -Configuration Release
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action Run
```

Actual automated verification and screenshot evidence are in `docs/member-1/VERIFICATION.md` and `docs/member-1/ui-check-results.json`.

## Human walkthrough

- Customer: log in as `customer1/pass123`, test blank submission, submit Billing or GeneralFeedback and retain the returned reference.
- Tracking: test lowercase/padded, unknown and blank references; verify read-only details and notes.
- Agent: log in as `agent1/agent123`, filter/reset, select a ticket, edit status/category/notes and save.
- Admin: verify `admin1/admin123` opens the support workspace.
- Reopen the app and confirm the saved update survives across the customer tracking and agent flows.
- Resize customer and agent windows to their minimum sizes and test keyboard navigation and scrolling.

## Ownership limits

- Member 2: durable reference allocation, save rollback and storage recovery.
- Member 3: SRS review, manual acceptance records, activity diagram, feasibility report and final presentation/video.
