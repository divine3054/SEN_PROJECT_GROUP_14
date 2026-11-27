# Phase 2: System and Software Design

Prepared: 26 November 2026 (Africa/Lagos). Model: Waterfall.

**Member 3 approved Phase 2 and explicitly authorized Phase 3 on 27 November 2026. Wider team review and unresolved scope decisions remain pending.**

Open [design.drawio](design.drawio) in diagrams.net/draw.io. Its eight editable pages have matching 2x-resolution PNGs in [exports](exports/). Use these exports in the PDF report. Recreate both formats with `powershell -File docs/phase-2/Generate-Design.ps1` from the repository root (script execution must be permitted by the local PowerShell policy). No application source changes are part of this design pack.

The complaint-lifecycle [activity diagram](ACTIVITY-DIAGRAM.md) and [feasibility, schedule and risk draft](FEASIBILITY-SCHEDULE-RISKS.md) are separate review documents. They are proposed material, not team-approved decisions.

## 1. Architecture

Presentation (`Forms/`) depends on business logic (`Services/`), which depends on data access and domain (`Data/`, `Models/`). Dependency arrows point downwards. Return values travel back to callers; this does not reverse source dependencies. Forms may reference model types for bindings and display, but must not read or write JSON directly.

- Presentation: LoginForm, CustomerForm, AgentForm; optional AdminSummaryForm is future scope. Collect input, populate dropdowns, show validation alerts, display grids and confirmation messages.
- Business logic: ComplaintService authenticates credentials, validates required title/description, creates references, filters and tracks tickets, and updates status/category/notes.
- Data access and domain: User, Complaint, UserRole, ComplaintCategory, ComplaintStatus, StorageService. Records are serialized to `complaints.json` and `users.json` relative to the process working directory. Password hashes use SHA-256 and Base64 encoding, matching the supplied code.

Create one ComplaintService instance for a running application and pass it to forms. Separate instances cache separate lists and can overwrite each other's changes. This is a Phase 3 implementation requirement; existing form placeholders do not yet use services.

## 2. UML conventions and code alignment

The use case boundary is **Complaint and Feedback Management System**. Customer, Support Agent and Administrator each associate with Log In. Submit Complaint/Feedback includes Receive Reference Number; the dashed `<<include>>` arrow points from Submit to Receive. The administrator use cases remain design requirements, not completed backend features.

The class diagram uses `+` for public members and `-` for private fields. The supplied model properties are public C# properties, so they use `+`, correcting the private markers in the proposed blueprint. HashPassword is static. ComplaintService owns its StorageService instance and manages User and Complaint collections. Complaint.SubmittedBy stores a username string; the code contains no User object navigation property or enforced foreign key.

The sequence page covers both submission and tracking, including missing-field, storage-failure and reference-not-found paths. On successful submission, SaveComplaints finishes before the service returns the new Complaint. A save failure can leave the cached list changed even though no success response was returned; see review decisions below.

## 3. Screen contract for Member 1

The wireframes show exact required control names. Text in brackets describes control behavior rather than a literal default value.

### LoginForm

- `txtUsername`: TextBox.
- `txtPassword`: TextBox; `UseSystemPasswordChar = true`.
- `btnLogin`: authenticate through ComplaintService; invalid credentials show an alert and keep the login screen open.
- Successful Customer login opens CustomerForm; SupportAgent or Admin opens AgentForm. Keep the authenticated User and shared service available to the destination form. AdminSummaryForm is optional and not required for this routing.

### CustomerForm

Use a TabControl with Submit Complaint and Track Status tabs (a two-panel layout is also acceptable).

- Submit: `txtTitle`, `cmbCategory` bound to ComplaintCategory, multiline `txtDescription`, `btnSubmit`, `lblGeneratedRef`.
- Submit calls `SubmitComplaint(user.Username, title, category, description)`. Display a confirmation popup and the returned reference in `lblGeneratedRef` only after success. Show missing-field and storage errors without a success confirmation.
- Track: `txtSearchRef`, `btnSearch`, plus proposed read-only outputs `lblTrackedStatus`, `lblTrackedCategory`, `lblTrackedUpdatedAt`, `txtTrackedAgentNotes` (multiline, ReadOnly).
- Track calls `TrackByReference`; reject blank input, show a clear not-found message and clear previous results on a miss. Show Status, Category, UpdatedAt and AgentNotes on a hit.

### AgentForm

- Filter bar: `cmbFilterCategory`, `cmbFilterStatus`, `btnFilter`, `btnRefresh`. Include All options mapped to null filters. Bind remaining options to their enum types.
- Grid: `dgvComplaints`, read-only, full-row selection, single selection, no user-added rows. Display ReferenceNumber, Title, Category, Status, SubmittedBy and CreatedAt. Set AutoGenerateColumns to false and bind explicit columns.
- Action panel: `cmbUpdateStatus`, `cmbUpdateCategory`, multiline `txtAgentNotes`, `btnSaveUpdate`.
- Selecting a row loads its status/category/notes. Disable saving without a selection. Save with the selected complaint's ReferenceNumber; refresh the grid after a successful update. A false return or storage exception must produce a useful alert.
- `btnFilter` uses GetAllComplaints with nullable filters; `btnRefresh` clears filters and reloads the service's cached list. The present API does not reload JSON from disk.

## 4. Backend review decisions required before sign-off

1. **Administrator scope:** Manage Agent Accounts and View System Ticket Summary appear in the requested use cases. No account-management or summary service methods exist. Approve adding them in implementation or explicitly defer these requirements. Routing admins to AgentForm alone does not fulfill them.
2. **Status policy:** UpdateComplaintStatus directly assigns any supplied status/category and does not validate actor permissions or transition order. Decide whether unrestricted enum-to-enum updates are accepted, or specify and implement allowed transitions and role enforcement.
3. **Reference policy:** The actual format is `CMP-{current year}-{Count + 1:D4}`; CMP-2026-0001 is an example for 2026, not a fixed-year constant. It is sequential only while the single cached list remains intact. Deletion, concurrent instances and external file edits can cause duplicate references. Accept the single-process/no-deletion assumption or require a durable sequence.
4. **Persistence failures:** JSON parse and file errors propagate to callers. Submissions/status updates mutate cached records before saving, with no rollback. Define and implement the desired recovery behavior before relying on retry after a failed save.
5. **Authentication and access:** The supplied implementation seeds demonstration accounts and uses unsalted SHA-256 hashes. It has no registration, account-management, role authorization or customer-ownership checks in service methods. Tracking uses reference alone. Record whether this matches the classroom demonstration scope; extra access requirements need implementation work.
6. **Build validation completed:** The project targets net10.0-windows. A workspace-local .NET SDK 10.0.404 was installed for verification. Release solution build succeeded with zero warnings and zero errors; see [build output](BUILD-VERIFICATION.txt). Seven isolated backend smoke checks passed; see [smoke output](BACKEND-SMOKE-VERIFICATION.txt). These are command-line results, not a claim of observing Visual Studio's Output window. No Phase 3 UI behavior was tested.

## 5. Phase 2 sign-off checklist

- [x] Architecture, use case, class, sequence diagrams and screen wireframes prepared as editable draw.io pages and high-resolution PNG exports.
- [ ] Wider team reviews the eight pages and resolves the backend review decisions above.
- [x] All five backend files committed and pushed to GitHub: `bfb18e46acfd88eae3dbd34b649cd214ef31eff9`. GitHub's commit/tree API confirmed their remote paths and blob hashes. The repository page was opened in the user's browser. See [recovery evidence](RECOVERY-VERIFICATION.md).
- [x] Release build and backend smoke checks completed (authentication, submit/persist/reload, track, filter, update).
- [ ] Member 3 signs the [project log](PROJECT_LOG.md), recording approval date, reviewers and accepted scope decisions.
- [ ] Advance to Phase 3 only after the required approvals are recorded.

Five backend files: Models/Enums.cs, Models/User.cs, Models/Complaint.cs, Data/StorageService.cs, Services/ComplaintService.cs.
