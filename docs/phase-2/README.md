# Phase 2: System and Software Design

Prepared: 26 November 2026 (Africa/Lagos). Model: Waterfall.

**Member 3 approved Phase 2 and explicitly authorized Phase 3 on 27 November 2026. Wider team review and unresolved scope decisions remain pending.**

Open [design.drawio](design.drawio) in diagrams.net/draw.io. Its nine editable pages have matching 2x-resolution PNGs in [exports](exports/), including the activity flow. Class and sequence pages reflect the integrated backend's reference counter and atomic saves. Use these exports in the PDF report. Recreate them with `powershell -File docs/phase-2/Generate-Design.ps1` from the repository root (script execution must be permitted by local PowerShell policy).

The complaint-lifecycle [activity diagram](ACTIVITY-DIAGRAM.md) and [feasibility, schedule and risk draft](FEASIBILITY-SCHEDULE-RISKS.md) are separate review documents. They are proposed material, not team-approved decisions.

## 1. Architecture

Presentation (`Forms/`) depends on business logic (`Services/`), which depends on data access and domain (`Data/`, `Models/`). Dependency arrows point downwards. Return values travel back to callers; this does not reverse source dependencies. Forms may reference model types for bindings and display, but must not read or write JSON directly.

- Presentation: LoginForm, CustomerForm, AgentForm; optional AdminSummaryForm is future scope. Collect input, populate dropdowns, show validation alerts, display grids and confirmation messages.
- Business logic: ComplaintService authenticates credentials, validates required title/description, creates references, filters and tracks tickets, and updates status/category/notes.
- Data access and domain: User, Complaint, UserRole, ComplaintCategory, ComplaintStatus, StorageService. Records are serialized to `complaints.json` and `users.json` relative to the process working directory. Password hashes use SHA-256 and Base64 encoding, matching the supplied code.

Program now passes one shared ComplaintService between the implemented forms. Each service retains a read snapshot; writes reload the persistent list under a directory-scoped in-process lock before saving, so separate instances in that process do not overwrite newer committed tickets. Concurrent application processes are outside the supported baseline.

## 2. UML conventions and code alignment

The use case boundary is **Complaint and Feedback Management System**. Customer, Support Agent and Administrator each associate with Log In. Submit Complaint/Feedback includes Receive Reference Number; the dashed `<<include>>` arrow points from Submit to Receive. The administrator use cases remain design requirements, not completed backend features.

The class diagram uses `+` for public members and `-` for private fields. The supplied model properties are public C# properties, so they use `+`, correcting the private markers in the proposed blueprint. HashPassword is static. ComplaintService owns its StorageService instance and manages User and Complaint collections. Complaint.SubmittedBy stores a username string; the code contains no User object navigation property or enforced foreign key.

The sequence page covers submission and tracking, with validation, save-failure and reference-not-found outcomes. The counter is reserved before persistence; SaveComplaints completes before the cache is published and a Complaint copy is returned. Failed saves preserve the earlier cache and primary; a reserved suffix can leave a gap.

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
3. **Reference policy implemented locally:** The per-year reference-sequence.json counter reserves above its saved watermark and the highest existing reference. It prevents reuse after deletion while retained; failed saves may leave gaps. One application process per data directory is supported.
4. **Persistence handling implemented locally:** Writes use flushed temporary files, atomic replacement and previous-version backups. Cached changes publish only after success. Invalid data is preserved; explicit recovery retains the displaced primary. See Member 2's handoff and the integration review.
5. **Authentication and access:** The supplied implementation seeds demonstration accounts and uses unsalted SHA-256 hashes. It has no registration, account-management, role authorization or customer-ownership checks in service methods. Tracking uses reference alone. Record whether this matches the classroom demonstration scope; extra access requirements need implementation work.
6. **Build validation completed:** The project targets net10.0-windows. The older Phase 2 build/smoke files remain historical evidence. The latest integrated solution builds with zero warnings/errors and passes 34 backend tests, 20 reliability checks and 18 native UI checks. See [integration review](../INTEGRATION_REVIEW.md) for fresh results, current-host measurements and evidence limits. Human acceptance is still separate.

## 5. Phase 2 sign-off checklist

- [x] Architecture, use case, class, sequence diagrams and screen wireframes prepared as editable draw.io pages and high-resolution PNG exports.
- [ ] Wider team reviews the nine pages and resolves the remaining scope decisions above.
- [x] All five backend files committed and pushed to GitHub: `bfb18e46acfd88eae3dbd34b649cd214ef31eff9`. GitHub's commit/tree API confirmed their remote paths and blob hashes. The repository page was opened in the user's browser. See [recovery evidence](RECOVERY-VERIFICATION.md).
- [x] Release build and backend smoke checks completed (authentication, submit/persist/reload, track, filter, update).
- [x] Member 3 approval and Phase 3 authorization recorded on 27 November 2026 in PROJECT_LOG.md. Wider team scope approval remains pending.
- [ ] Complete wider team review and scope decisions before final acceptance. UI implementation and latest QA are integrated locally.

Five backend files: Models/Enums.cs, Models/User.cs, Models/Complaint.cs, Data/StorageService.cs, Services/ComplaintService.cs.
