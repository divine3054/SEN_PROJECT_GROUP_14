# Feasibility, schedule and risk draft

Status: **Proposed planning material; team review and approval pending.**
Prepared: 27 November 2026.

This draft reflects the current Windows Forms/.NET 10 implementation and the proposed first acceptance baseline in the [SRS](../requirements/SRS.md). The team-reported target date is 4 December 2026. This is not a team-approved estimate, budget or delivery commitment. Team availability, cost rates and workday preferences have not been supplied.

## 1. Feasibility

| Area | Assessment | Constraints and actions |
|---|---|---|
| Technical | Feasible for a classroom desktop demonstration. The solution builds with the .NET 10 SDK and the backend has an xUnit test project. The app uses WinForms and JSON files. | Windows-only UI; one local application instance is assumed. There is no database server or cross-process synchronization. The integrated backend has atomic saves, previous-version backups, explicit recovery and a per-process directory lock. Unsalted SHA-256 is demonstration-only and not suitable for production authentication. |
| Operational | The Customer and Support Agent workflows are understandable and have been exercised through the local UI. A single operator can run the app without network access after dependencies are restored. | Users need a Windows computer and a writable working directory. Team members must agree on role scope, status policy, data handling, and how demonstration JSON files are backed up/reset. Manual acceptance remains incomplete. |
| Economic | The selected stack and JSON storage introduce no application hosting or database-service cost for the stated local demonstration. | No labour rate, hardware budget, licensing analysis, or cost of support was supplied. No monetary total is asserted. |
| Schedule | A short, dependency-ordered completion sequence is plausible if the team is available. See the proposed work packages below. | Dates and durations are indicative working-day estimates only. The course deadline, team capacity, and time for lecturer feedback are unknown. Scope decisions or UI/backend defects can extend the estimate. |
| Legal / ethical | The baseline can use synthetic complaint data and demonstration credentials. | Do not put real personal/sensitive complaints into the demo JSON. Confirm any institutional requirements for retention, access, attribution, and report publication with the lecturer/team. No legal review is claimed. |

## 2. Proposed schedule and dependencies

The date windows below are a proposed plan working backward from 4 December, not actual elapsed time or confirmed member availability. The team should adjust them immediately if the deadline or availability differs.

| Order | Target window | Work package | Proposed owner | Estimate | Dependency / exit evidence |
|---|---|---|---|---:|---|
| 1 | 27–28 Nov | Review the SRS, administrator scope, reference assumptions, status/role policy, and this risk register | All members | 0.5 day | Decisions recorded with reviewer names and date |
| 2 | 29–30 Nov | Fix accepted UI/backend defects and agree storage-error behavior | Member 1 (UI), Member 2 (backend) | 1–2 days | Reviewed code changes and focused regression tests |
| 3 | 1 Dec | Run the automated suite and record a clean result | Member 3 | 0.5 day | Fresh TRX/report; failures resolved or documented |
| 4 | 1–2 Dec | Complete manual system scenarios and performance measurement on the demonstration PC | Member 3 with Member 1/2 for fixes | 1 day | Inputs, expected/actual results, pass/fail, environment and evidence references |
| 5 | 2–3 Dec | Run stakeholder acceptance with Customer and Support Agent representatives | Team | 0.5 day | Named reviewers, feedback, decision and date |
| 6 | 3–4 Dec | Assemble the final report, diagrams, screenshots and recorded demo | Team, coordinated by Member 3 | 1–2 days | Reviewed PDF and viewable demo/report links |
| 7 | 4 Dec | Final review and submission | All members | 0.5 day | Confirmed repository state, approved report and submission receipt |

The schedule is sequential where work depends on decisions or fixes. Test writing can proceed in parallel with UI/backend fixes, but final test evidence must be generated against the agreed build.

Recommended scope response for the stated short timeline: defer Administrator account management and system summaries from the first acceptance baseline because they are not implemented. This is a recommendation only; record the team's decision before treating the scope as approved.

## 3. Risk register

Likelihood and impact are preliminary qualitative ratings, not measured probabilities. Owners and responses are proposed and require team confirmation.

| ID | Risk | Likelihood | Impact | Proposed response | Proposed owner |
|---|---|---|---|---|---|
| R-01 | Administrator account management and system-summary requirements are omitted or added late | Medium | High | Decide scope at requirements review; update SRS, use cases, schedule and acceptance cases before sign-off | All members |
| R-02 | Save or JSON parsing failure leaves the UI inconsistent or cached data changed | Medium | High | Define recovery behavior, avoid success-shaped messages, add isolated failure tests, and retest restart/recovery scenarios | Member 2 backend; Member 1 UI |
| R-03 | Reference collisions occur after deletion, external file edits, or concurrent app instances | Medium | Medium | Keep the single-instance/no-deletion assumption explicit or implement a durable, collision-safe sequence and test it | Member 2 |
| R-04 | Unsalted SHA-256 and demonstration credentials are mistaken for production security | Medium | High | Label the app/demo credentials clearly; do not reuse real passwords or real user data; select a password-hashing design before any production use | Team |
| R-05 | Manual UI, performance, or stakeholder acceptance evidence is missing at submission | High | High | Assign a tester and date, run each scenario, retain permitted screenshots/measurements, and reserve report/demo time | Member 3 and team |
| R-06 | The WinForms application cannot run on non-Windows machines or without the .NET 10 SDK/runtime | Medium | Medium | Identify Windows/.NET prerequisites in the report and validate on the actual demonstration machine | Member 1 / team |
| R-07 | Local JSON data is lost, overwritten, or modified by multiple app instances | Medium | High | Use an isolated demo folder, keep backups of any approved demo dataset, and avoid concurrent instances; consider transactional storage if scope expands | Member 2 |
| R-08 | Incorrect member-to-role mapping or unverified individual contribution records delay or misattribute the final report | Medium | High | Confirm each member's role and verify contribution evidence; keep matriculation numbers out of version-controlled docs unless the team explicitly approves publication | All members |
| R-09 | Course deadline or team availability is shorter than the proposed plan | Unknown | High | Confirm the submission date and member availability; re-estimate and prioritize core acceptance criteria | Team lead / all members |

## 4. Decisions and evidence still needed

- Team approval of scope assumptions and the administrator feature decision.
- Team confirmation of schedule dates, owners and the course deadline.
- Actual manual-test and performance evidence from the demonstration environment.
- Stakeholder acceptance records and verified team-role mapping; place matriculation numbers only in the approved final-report location.
- A reviewed final report and a working, viewable demo link.

No phase approval, budget approval, team consensus, or personal commitment is inferred by this draft.


## 5. Integration update - 27 November 2026

The earlier risk ratings and schedule remain proposed. In the integrated working tree, Member 1's UI fixes and Member 2's cache-safe atomic saves, explicit recovery and persisted counter mitigate R-02/R-03/R-07. One app process per data directory remains required. Fresh results and current-host UI measurements are in ../INTEGRATION_REVIEW.md. Final target-PC testing, acceptance, team scope decisions, a confirmed deadline and submission deliverables remain open.
