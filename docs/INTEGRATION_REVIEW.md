# Integrated project review - 27 November 2026

Group 14: Complaint and Feedback Management System. Review scope: fetch and integrate Members 1/3, preserve Member 2 work, debug the combined implementation, verify it and identify remaining assignment deliverables.

## Remote work received and local integration

- Member 1, Eregha Tarilah: 2fea1c6, Complete complaint screens and UI validation, on origin/member1-ui-completion.
- Member 3, Chukwuemeka Okoye: e659b7f, requirements/tests/QA, and 406036b, expanded QA/handoff material, on origin/main.
- Preserved pending work in stash 9f5256021d1a02d8487c7b9ae3a3420b231cc096. Locally merged both histories in 57df846 and restored Member 2's backend changes/additions. The newer Member 3 files were retained instead of restoring older local drafts over them.
- No GitHub pull requests existed when checked. The local merge, pending Member 2 work and this review's follow-up changes have not been pushed. Existing authorship is preserved; no commits were attributed to another member.

## Findings and fixes

1. **Stale success reference after validation failure - fixed.** Member 1's handler cleared the label only after field validation. A successful submission followed by an empty submission left its old reference visible. New UI-14 reproduced the failure. The handler now clears the old result at the start of every submit attempt; UI-14 passes and the rejected attempt creates no additional complaint.
2. **Integration was incomplete on shared main - integrated locally.** Members 1 and 3 had diverged from 8e4b9d5. Their commits now coexist locally with the pending Member 2 cache-safe atomic storage, durable reference counter and explicit backup recovery.
3. **Verification lacked a restarted cross-role flow - added.** UI-15 submits from CustomerForm, reopens with a new service in AgentForm, updates the ticket and reopens CustomerForm to confirm persisted status/category/notes.
4. **Performance evidence was backend-only - expanded.** UI-16 loads and binds 1,000 native grid rows, applies filters and performs tracking through actual WinForms controls. It records this host's environment and timings. This is developer-machine evidence, not designated-PC or stakeholder acceptance.
5. **Documentation described the old implementation - corrected.** Updated SRS, QA issue status, Member 3 push status, activity narrative, feasibility constraints and post-coding checklist. Preserved the historical exploratory results and Member 3's recorded approval; no new human signature or team consensus was inferred.
6. **Design exports were stale/incomplete - refreshed.** Class/sequence diagrams now show the integrated storage/counter behavior. Added an editable activity page and PNG. All nine pages/exports are present; refreshed diagrams were visually inspected.
7. **Generated test reports obscured source state - corrected.** Reports now identify the base revision and whether local uncommitted changes are included. Runtime JSON/backup/counter files are ignored by Git so routine demonstration data is not accidentally staged.

## Final executed verification

- Whole solution Release build: **0 warnings, 0 errors**.
- Member 3's xUnit project: **34 passed, 0 failed, 0 skipped**; includes its 1,000-record service microbenchmark. See [fresh results](testing/TEST_RESULTS.md).
- Member 2's failure/reliability harness: **20 passed, 0 failed**. See [results](review/backend-reliability-results.json).
- Expanded native WinForms harness: **18 passed, 0 failed**. See [results and measurements](review/ui-check-results.json).
- Combined total: **72 passing automated checks**. These mix unit/integration, failure-injection, performance and native-control checks; they are not all isolated unit tests.
- 1,000-record current-host UI measurement: grid binding/open 312.6 ms, filtering 169.0 ms, tracking 25.0 ms. Host: Windows 10.0.26300, .NET 10.0.15, 16 logical CPUs, 192 DPI. Each is below the proposed two-second target on this host. No claim is made about another machine.

Commands, from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Check-BackendReliability.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Run-Project.ps1 -Action UiCheck -Configuration Release
```

All automated checks use isolated sample data. UI checks create native controls and record message requests instead of clicking through actual modal dialogs. Source review/testing found no remaining failing core automated scenario in this baseline; that does not substitute for human acceptance or prove absence of every defect.

## Remaining work before submission

1. **Publish the tested integration.** Review and commit Member 2's backend contribution and the integration fixes/documentation. Push through the team's branch/review workflow so shared main contains all three members' work. Ensure the final test results apply to the published revision. The current remote main still lacks Member 1's completion and pending backend/review changes.
2. **Agree the remaining scope and planning details.** All three members review the updated SRS/design, choose whether administrator account-management/summary features are deferred, and confirm status/authorization limitations and the one-app-process assumption. These extra administrator features are not explicit lecturer core requirements. Confirm the actual deadline; the planning draft's 4 December target is not independently verified.
3. **Complete final human system/recovery and acceptance checks.** On the demonstration computer, run the real Customer/Agent restart workflow, actual popup/keyboard/resize interactions and isolated malformed/unwritable-storage recovery. Retain tester/date, inputs, expected and actual results, pass/fail, environment and evidence filenames. Have Customer/Support Agent representatives record acceptance feedback and decisions. Member 3's recorded Phase 2 approval already exists; do not fabricate additional approvals.
4. **Capture final evidence.** Keep clear running-app screenshots and record a short video covering valid submission/reference tracking, filtering/updating, persisted results after restart and invalid-input handling. Current rendered fixture images and developer measurements can support the report, but do not establish a human acceptance session. Recheck responsiveness on the actual demonstration machine if it differs.
5. **Assemble one final PDF report.** Include the overview, feasibility/schedule/risks, Waterfall lifecycle, reviewed SRS, all required UML/architecture/activity diagrams, implementation/storage description, test case records with inputs/expected/actual/pass-fail, screenshots, individual contributions, all three members' confirmed full names/matriculation numbers, repository URL and viewable video link. No final compiled report PDF or demonstration video/link was found in the inspected repository.
6. **Review and submit.** Reopen the final PDF and recording, check spelling/diagrams/link permissions and member attribution, obtain the team's final review, submit through the required course channel and retain the receipt.

The core application coding and automated baseline are now integrated and passing locally. The main outstanding work is publication, team/manual acceptance evidence and the final report/demo/submission. The more detailed task list is [POST-CODING-CHECKLIST.md](POST-CODING-CHECKLIST.md).
