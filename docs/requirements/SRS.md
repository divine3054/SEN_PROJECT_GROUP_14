# Software Requirements Specification

Project: Group 14 - Complaint and Feedback Management System.

Version: 0.1, prepared 26 November 2026. Owner: Member 3 (QA, test development and documentation). **Draft for team review; not a signed approval.** Full names and matriculation numbers must be supplied by the team for the final report.

## 1. Purpose and sources

The lecturer's Group Topics brief assigns Group 14: submit complaints, categorise issues, assign reference numbers and track resolution status (Group Topics.pdf, Group 14, pages 2-3). The Group Project Questions brief additionally requires lifecycle planning, requirements, UML/design, OOP C#, version control, staged testing and one final PDF report.

This SRS was prepared after existing implementation work. It records the current scope and proposed acceptance criteria; it must not be presented as evidence of an earlier review that did not occur.

## 2. Scope and users

The application is a Windows Forms desktop application using JSON file storage. It supports one running application instance on one Windows computer. The Customer submits a complaint or general feedback and checks its progress. The Support Agent reviews, categorises and updates tickets. The Administrator currently uses the same support workspace.

Authentication, filtering and agent notes are team-selected supporting features. Administrator account management, summary dashboards, email notifications, attachments, a web portal and simultaneous multi-user access are outside this first acceptance baseline. Account management and summaries were listed in the earlier Phase 2 blueprint; the team must approve this proposed scope reduction or schedule those features explicitly.

## 3. Functional requirements

- **FR-01 - Authenticate users (team scope).** Accept valid seeded credentials; reject an unknown username or incorrect password. Username matching is case-insensitive; passwords are case-sensitive. Route Customers to CustomerForm and SupportAgent/Admin to AgentForm. Backend evidence: AUTH01-AUTH03; routing evidence: MAN-01.
- **FR-02 - Submit a complaint or feedback (lecturer core).** Accept the authenticated customer's username, nonblank title, defined category and nonblank description. Trim title/description; create a Pending record and persist it. Reject blank required fields without creating a ticket. Evidence: SUB01-SUB02 and MAN-02/MAN-03.
- **FR-03 - Categorise issues (lecturer core).** Offer Billing, Technical, Service and GeneralFeedback. Allow support staff to update the category of a selected complaint. Evidence: CAT01, UPDATE01 and MAN-04.
- **FR-04 - Assign references (lecturer core).** Return CMP-{current year}-{sequence padded to at least four digits}. A saved per-year counter reconciles with the highest existing reference and prevents reuse after ticket deletion while the healthy counter is retained. Failed saves may leave reserved gaps. Existing installations migrate on first allocation. One application process per data directory is supported; resetting/deleting the counter or running independent processes is outside the guarantee. Evidence: REF01, REL-01/03-07/14/20 and MAN-02.
- **FR-05 - Track resolution status (lecturer core).** Look up a complaint by reference, ignoring letter case and surrounding spaces. Display category, status, most recent update time and agent notes. An unknown reference must show no-match feedback rather than stale results. Evidence: TRACK01-TRACK02 and MAN-05. The integrated UI rejects blank references and clears prior details before each search; UI-06 verifies this automatically.
- **FR-06 - Review/filter complaints (team scope).** Support staff can view all complaints or filter by category, status or both. All maps to no filter; Reset clears filters. Order the returned list by newest creation time. Evidence: FILTER01-FILTER02 and MAN-04.
- **FR-07 - Update resolution (lecturer core workflow).** For an existing selected ticket, save the chosen status/category, trimmed nonblank notes and a new UpdatedAt while preserving CreatedAt. Empty notes preserve existing notes. An unknown reference returns failure without changing records. Evidence: UPDATE01-UPDATE03 and MAN-04. The current service permits any enum-to-enum transition; a stricter transition policy needs a separate approved requirement.
- **FR-08 - Persist and reload records (team design).** Save complaints/users as JSON in the application's working directory; preserve submitted details, reference, category, status and notes when the service/application restarts. Seed demonstration users only when users.json is absent. Evidence: STORE01-STORE04, REF01, UPDATE01 and MAN-06.

## 4. Non-functional requirements and acceptance targets

- **NFR-01 - Usability.** Password text must be masked, category/status choices must use dropdowns, tracking results must be read-only, and agent edits must target a selected record. Verify manually with MAN-01/MAN-04/MAN-05; a backend test cannot establish screen usability.
- **NFR-02 - Validation and reliability.** Reject empty required fields and undefined enum values. Successful writes use a flushed temporary file and atomic replacement with a previous-version backup. Failed writes leave the service cache and prior primary unchanged. Malformed/missing-primary-with-backup data causes a clear error; explicit validated recovery preserves the damaged primary. SUB02/STORE05, REL-01/02/11-18 and UI-07/12 provide automated evidence. Human startup/recovery confirmation remains pending.
- **NFR-03 - Performance (proposed target).** On the demonstration Windows computer, search/filter 1,000 locally stored records within 2 seconds. The latest backend-only PERF-01 run on the current test host measured category/status filtering at 1.056 ms and last-reference lookup at 0.285 ms (Windows `10.0.26200`, .NET `10.0.15`, 8 logical CPUs). This does not measure WinForms grid rendering or confirm the designated demonstration PC; full performance acceptance remains pending. See [test plan](../testing/TEST_PLAN.md) and [results](../testing/TEST_RESULTS.md).
- **NFR-04 - Maintainability.** Forms call ComplaintService; business rules reside in Services; JSON access resides in Data; reusable data types reside in Models. Tests reference the real application project. No duplicate backend implementation is maintained in the tests.
- **NFR-05 - Buildability and portability.** Build the app and test project on Windows with the .NET 10 SDK. Test fixtures must not depend on a developer username or modify real application JSON. Verify the solution build and automated suite on each contributor's machine.
- **NFR-06 - Demonstration credential handling.** Store password hashes rather than plaintext passwords. The supplied implementation uses unsalted SHA-256/Base64 and known demo credentials. This is a classroom demonstration baseline, not a production authentication claim. Evidence: UNIT01/STORE03 and AUTH01-AUTH03.

## 5. Data and constraints

- User: Id, Username, PasswordHash, Role.
- Complaint: ReferenceNumber, SubmittedBy (username), Title, Category, Description, Status, AgentNotes, CreatedAt, UpdatedAt.
- ComplaintStatus values: Pending, InProgress, Resolved, Closed. A return to another status is currently permitted.
- Files: users.json, complaints.json and reference-sequence.json, with previous-version .bak files after replacement. One shared ComplaintService is passed between forms. Storage captures its data directory at construction; backups are restored only by explicit operator action.
- The first demonstration accounts are customer1/pass123, agent1/agent123 and admin1/admin123. These are sample classroom accounts.
- Internet is needed for the first dependency restore and GitHub collaboration; normal local application use does not require a database server or network service.

## 6. Scenarios and acceptance

1. A Customer logs in, submits a Billing complaint and records the returned reference. The same reference finds the saved complaint with Pending status.
2. A Support Agent logs in, filters the ticket list, selects that complaint, changes its category/status and saves resolution notes.
3. The Customer tracks the reference again after reopening the application and sees the persisted update.
4. Invalid credentials, blank submission fields and unknown references produce understandable feedback without creating or modifying unrelated tickets.

Automated backend results, manual UI results and stakeholder acceptance must be recorded separately. All core scenarios need actual system-test evidence before the application is declared complete. See ../testing/TEST_PLAN.md.

## 7. Review record

- Requirements validation by the three members: Pending.
- Approval of the administrator scope adjustment and single-instance assumptions: Pending.
- Member 3 selected the current unrestricted enum-to-enum status/category update policy for the classroom baseline; wider team review remains pending.
- Formal Phase 2 approval and explicit Phase 3 authorization by Member 3: OKOYE Chukwuemeka, 27 November 2026; wider team review and scope decisions remain pending. See ../phase-2/PROJECT_LOG.md.
- No stakeholder acceptance or signature is inferred by this document; the Member 3 approval is recorded as a direct confirmation in ../phase-2/PROJECT_LOG.md.
