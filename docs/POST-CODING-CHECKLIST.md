# Post-coding project completion checklist

Target submission date reported by the team: **4 December 2026**. Confirm this against the course brief. This checklist describes work after the current coding baseline; it does not claim that unchecked evidence exists.

## 1. Team review and scope decisions

- [ ] All three members review the [SRS](requirements/SRS.md), diagrams, [test plan](testing/TEST_PLAN.md), [activity diagram](phase-2/ACTIVITY-DIAGRAM.md), and [feasibility/schedule/risk draft](phase-2/FEASIBILITY-SCHEDULE-RISKS.md).
- [ ] Record the Administrator scope decision. Recommended for the short deadline: explicitly defer account management and summaries from this baseline; if retained, implement and test them.
- [ ] Confirm the current unrestricted status/category update policy or agree and implement transition/role restrictions.
- [ ] Agree on the single-instance, no-deletion reference-number assumption or implement a collision-safe sequence.
- [ ] Agree on how malformed/unwritable JSON is recovered and how failed saves affect cached data.
- [ ] Confirm each member's role and contribution evidence. Put matriculation numbers only in the team-approved report location, not public repository documentation unless explicitly required and approved.
- [ ] Update `docs/phase-2/PROJECT_LOG.md` with the other reviewers' names/dates and the final scope decisions. Member 3's Phase 2 approval and Phase 3 authorization are recorded there; they do not represent whole-team approval.

## 2. Fix and retest known issues

- [ ] Member 1: clear the previous success/reference label after a rejected submission and decide whether a confirmation dialog is required.
- [ ] Member 1: give blank tracking input explicit validation feedback; retain no-stale-result behavior for misses.
- [ ] Member 1: agree and implement the agent grid columns, single-row selection, disabled-save state, and notes layout.
- [ ] Member 1 and Member 2: show useful persistence/startup errors without false success.
- [ ] Member 2: prevent or roll back cached changes after failed writes; define startup handling for corrupt JSON.
- [ ] Rerun the relevant unit/integration and manual scenarios after every fix; record actual results in [TEST_PLAN.md](testing/TEST_PLAN.md).

## 3. Complete quality assurance

- [ ] Run `powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Test-Backend.ps1` from the repository root on the final source revision. Review the generated [TEST_RESULTS.md](testing/TEST_RESULTS.md); keep the TRX as local evidence or put it in the team-approved evidence store.
- [ ] Repeat MAN-01 through MAN-06 as a team on the designated Windows demonstration computer. Record tester, date, exact input, expected/actual output, pass/fail, build/revision and screenshot filename.
- [ ] Run MAN-07 using an isolated disposable working directory: malformed JSON and an unwritable complaint file. Never risk the only copy of team data.
- [ ] Measure full UI load/filter/search response with 1,000 synthetic records on the designated computer; record machine/runtime details and measurements. Current PERF01 is a service-level microbenchmark only.
- [ ] Have one Customer and one Support Agent representative complete the SRS acceptance scenarios. Record their names, feedback, decision, date, and unresolved limitations. Do not treat developer/assistant exploratory checks as stakeholder acceptance.

## 4. Assemble the final report and demo

Use the lecturer's required format/template if one exists. A complete report pack normally includes:

- [ ] Cover/title page, course/project identifiers, group number, accurate member names and matriculation numbers in the approved location.
- [ ] Executive summary, problem statement, objectives, scope, assumptions and exclusions.
- [ ] Feasibility analysis, approved schedule, risk register and lifecycle/methodology.
- [ ] Validated requirements and acceptance criteria; requirements-to-test traceability.
- [ ] Architecture and reviewed UML/use-case/class/sequence/activity diagrams. Editable source and exports are in `docs/phase-2/`.
- [ ] Implementation summary, key UI screenshots, data/storage design and user instructions/demo credentials (clearly labelled as classroom-only).
- [ ] Build/test commands and fresh automated, manual, performance and stakeholder acceptance results, including environment and limitations.
- [ ] Known defects, security/privacy limitations, future improvements and conclusion.
- [ ] Individual contribution statements and references/citations required by the course.
- [ ] Create a short screen recording showing a customer submission and tracking, an agent filter/update, and the persisted result after restart. Use synthetic data; do not expose real personal complaint information.
- [ ] Save the final PDF and demo recording in an approved shared location. Reopen/download both to confirm they work; test link permissions from an intended reviewer account.
- [ ] Have every member review the final PDF, spelling, screenshots, links and contribution attributions before submission.

## 5. Repository and submission

- [ ] Review `git diff --check`, `git status`, tests and the final commit history.
- [ ] Push the contribution branch/commit using the team's agreed workflow; open a pull request if required and verify remote attribution.
- [ ] Keep generated build files, local complaint/user JSON, credentials, and machine-specific artifacts out of Git.
- [ ] Submit the PDF, recording/link, repository URL and any required appendices through the lecturer's required channel before the confirmed deadline.
- [ ] Save the submission receipt/confirmation and tell all members where the final artifacts are stored.

## Current status

The backend suite currently reports 34/34 passing tests, and exploratory UI checks are documented. MAN-07, full designated-PC performance acceptance, independent stakeholder acceptance, team-wide review/scope decisions, screenshots/video, final PDF, and submission are still outstanding. Do not mark the project complete until those team-required items are either completed or explicitly accepted as out-of-scope by the lecturer/team.
