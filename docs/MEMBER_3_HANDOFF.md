# Member 3 contribution status and remaining work

Updated: 27 November 2026. Project target date supplied by the team: 4 December 2026.

This handoff reflects the current repository and separates verified work from team decisions and evidence that must not be invented.

## Completed in the repository

- Draft requirements and acceptance criteria: [SRS](requirements/SRS.md).
- xUnit project referencing the real application, covering authentication, complaint workflows, storage and a 1,000-record backend filter/lookup measurement.
- Reproducible runner and TRX-based report generator: `scripts/Test-Backend.ps1` and `scripts/Write-TestReport.ps1`.
- Automated results: 34 passed, 0 failed. The runner builds the solution. NuGet vulnerability-metadata retrieval warned because the network request failed; restore/build/test still succeeded.
- Exploratory local UI checks, with limitations recorded in [TEST_PLAN.md](testing/TEST_PLAN.md). These are not independent stakeholder acceptance.
- Draft activity diagram and feasibility/schedule/risk register in [docs/phase-2](phase-2/README.md).
- First contribution commit: `e659b7f` (`Member 3: Add requirements, backend tests, and QA evidence`). It is local and has not been pushed.

## Required team decisions

- Confirm whether Administrator account management and ticket summaries are deferred from this baseline or added to the first release.
- Confirm the status-transition and role-authorization policy with the team. Member 3 selected the current unrestricted enum-to-enum updates for the classroom baseline; the present service has no actor-level role enforcement.
- Review the SRS, reference-number assumptions, storage/recovery behavior, activity diagram and proposed schedule/risk register.
- Confirm the course deadline and member availability. The current planning target is 4 December 2026, but the proposed schedule is not team-approved.
- Member 3's Phase 2 approval and explicit Phase 3 authorization were confirmed by OKOYE Chukwuemeka on 27 November 2026. Wider team review and scope decisions remain pending.

## Work still needed before the project can be called complete

1. **Close UI/backend defects:** decide and fix the missing submit confirmation, blank-reference validation, stale reference label after a rejected submission, grid columns/selection behavior, and storage-error recovery. See QA-01 through QA-04 in the test plan.
2. **Finish manual verification:** run MAN-07 in an isolated data folder; re-run MAN-01 through MAN-06 as a team on the designated PC and retain screenshots with tester/date/input/result.
3. **Finish performance acceptance:** record full WinForms grid/search/filter responsiveness and machine details on the designated demonstration PC. The current `PERF01` result is backend-only.
4. **Run stakeholder acceptance:** Customer and Support Agent reviewers walk through the SRS scenarios and record names, feedback and decisions.
5. **Complete report and demo:** follow the [post-coding checklist](POST-CODING-CHECKLIST.md), use the approved report template, and verify the final PDF and recording links.
6. **Finish team phase review:** record the other members' review and the agreed scope decisions in `docs/phase-2/PROJECT_LOG.md`. Member 3's own approval is recorded; do not infer team consensus from it.
7. **Deliver the contribution:** review `git diff --check`, push the local commit/branch as the team requires, open a pull request if required, and verify remote attribution.

No independent stakeholder acceptance, screenshot/video capture, or team consensus is inferred by this handoff. The local UI verification created and updated the sample reference `CMP-2026-0001`; it remains in the application's local data directory.

## Reproduce the backend suite

From the repository root on Windows with .NET 10:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1
```

Tests use unique temporary working directories and serialize within the test assembly because storage uses relative filenames. Generated `TestResults` output is ignored by Git. Read the freshly generated `docs/testing/TEST_RESULTS.md`; do not copy old counts over new results.
