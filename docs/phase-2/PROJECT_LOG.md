# Waterfall Project Log

## 26 November 2026 — Phase 2 design baseline prepared

Architecture, UML diagrams, and named-control WinForms wireframes prepared in `docs/phase-2/`. Backend source reviewed against the proposed blueprint. No Phase 3 UI implementation performed.

Status: **Awaiting review and sign-off**.

- Wider team diagram review: Pending.
- Member 2 backend commit/push verification: Pending. The five backend files are locally untracked; no verified GitHub commit SHA recorded.
- Build and runtime checks: Pending; local SDK unavailable.
- Administrator scope decision: Pending.
- Status/role policy decision: Pending.
- Reference generation and persistence assumptions: Pending.
- Member 3 approval: Pending.

To approve, record reviewer names, date, backend commit SHA, validation evidence, scope decisions and the explicit Phase 3 authorization here. This entry does not represent approval by any team member.

## 26 November 2026 - Phase 2 recovery verification

This entry supersedes the earlier untracked/backend-build findings. It records technical verification, not a team member's signature.

- Backend commit: `bfb18e46acfd88eae3dbd34b649cd214ef31eff9`.
- Push: succeeded to `origin/main` at `https://github.com/divine3054/SEN_PROJECT_GROUP_14`.
- GitHub verification: commit and recursive tree API confirmed all five backend paths and blob hashes; repository page opened in the user's browser.
- Build: `dotnet build ComplaintManagementSystem.sln --configuration Release --verbosity minimal --nologo` using workspace-local .NET SDK 10.0.404; succeeded with 0 warnings and 0 errors. Full output saved in BUILD-VERIFICATION.txt.
- Backend smoke checks: all 7 passed, covering seeded role authentication, required fields, sequential references, lookup, filtering, updates and persisted reload. Output saved in BACKEND-SMOKE-VERIFICATION.txt.
- Diagram verification: design.drawio parses as XML and contains 8 editable pages. All 8 PNG exports were visually checked; the architecture/use case/class/sequence exports are present and viewable.
- Wider team scope review: not attested by a team member.
- Member 3 formal approval: **Pending direct confirmation**. No approval is inferred from the instruction to obtain approval.
- Master report confirmation: no connected document session is available; this repository log is the current documentation record.

### Waterfall Gate Sign-Off - Phase 2 (System & Software Design)

- **Approved By:** Pending confirmation from Member 3 (Requirements, QA & Documentation Lead).
- **Verified Deliverables:** 3-Tier Architecture & UML Diagrams (Use Case, Class, Sequence), WinForms Screen Wireframes, and committed C# Backend (Models/, Data/, Services/) verified on GitHub with a clean command-line build.
- **Status:** Awaiting Member 3 approval. Phase 3 authorization has not been recorded.
- **Approval date and confirming name:** Pending.

Upon receiving Member 3's actual confirmation, record their name/date and change the status to: **Approved to advance to Phase 3: Implementation (WinForms UI)**. Member 3 should also confirm that the entry is saved in the team's master report, or designate this repository log as that record.

## 27 November 2026 — Member 3 requirements and QA work

- The requirements SRS, xUnit backend test project, test plan, test runner and generated test evidence were integrated in local commit `e659b7f`. The commit has not been pushed.
- The expanded automated suite passed 34/34 cases, including a 1,000-record backend filter/lookup measurement. The local UI exploratory results and their limitations are recorded in [TEST_PLAN.md](../testing/TEST_PLAN.md); they are not stakeholder acceptance.
- Exploratory UI checks confirmed both demo roles route to their expected screens, submission and reference lookup work, the Agent can filter/reset and update a ticket, and the update is visible to the Customer after restart. The tested complaint was `CMP-2026-0001`; these checks were operator-assisted and no screenshot files were retained.
- A draft complaint-lifecycle activity diagram and proposed feasibility/schedule/risk register are now available in this folder. Team review and decisions remain pending.
- Manual malformed-storage recovery, full UI performance measurement, stakeholder acceptance, screenshot/video capture, and final report assembly remain outstanding.
- **Member 3 sign-off:** OKOYE Chukwuemeka confirmed review and approval of the Phase 2 design on 27 November 2026, and explicitly authorized advancing to Phase 3. This records Member 3's approval only; wider team review, administrator scope decision, and other open requirements remain pending.

## 27 November 2026 - Integration review and regression fixes

- Fetched Member 3 commits e659b7f and 406036b from remote main and Member 1 completion commit 2fea1c6 from member1-ui-completion. Merged their histories locally in 57df846, preserving both authors' commits.
- Preserved pending Member 2 source/additions via backup stash 9f5256021d1a02d8487c7b9ae3a3420b231cc096, then restored them over the integrated work. Older local Member 3 drafts remain in the backup rather than overwriting the newer remote contribution.
- Reproduced and fixed the stale successful reference remaining visible after an invalid resubmission. Added native-control regressions for that case, reopened cross-role persistence and 1,000-record UI responsiveness.
- Corrected stale unpushed status text and current persistence/reference requirements. Refreshed class/sequence diagrams and added the activity PNG/editable page.
- Fresh results, evidence scope and remaining work are in ../INTEGRATION_REVIEW.md. This entry records technical work only; no stakeholder acceptance or broader team approval is inferred.
- The merge is local. Pending backend/review changes and the local integration still need review and publication to the shared remote repository.
