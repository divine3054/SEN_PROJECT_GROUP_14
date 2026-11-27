# Member 3 contribution: requirements, automated tests and QA records

Prepared against main commit 8e4b9d5. The application UI/backend already exist at that revision. This package adds documentation and a real xUnit test project; it does not replace application source files or supply a Phase 2 signature.

## 1. Files to copy

The handoff ZIP preserves repository-relative paths. Open its files in your editor to review or copy/paste their contents into the matching paths in your own clone. Do not copy build output, .tools, JSON data or another person's .git folder.

- ComplaintManagementSystem.Tests/ComplaintManagementSystem.Tests.csproj - xUnit packages and reference to the real app.
- ComplaintManagementSystem.Tests/TestWorkspace.cs - isolated temporary folders and sequential execution.
- ComplaintManagementSystem.Tests/AuthenticationTests.cs - valid/invalid login cases.
- ComplaintManagementSystem.Tests/ComplaintServiceTests.cs - submission, categories, references, lookup, filters and updates.
- ComplaintManagementSystem.Tests/StorageServiceTests.cs - JSON round trips, users and hashing/corrupt-file checks.
- docs/requirements/SRS.md - draft requirements and acceptance criteria.
- docs/testing/TEST_PLAN.md - automated cases, manual scenarios and open issues.
- docs/testing/TEST_RESULTS.md - actual prepared run evidence; regenerate with your own run before committing.
- scripts/Test-Backend.ps1 - build, execute tests and refresh result evidence.
- scripts/Write-TestReport.ps1 - convert actual TRX output into the Markdown result record.
- docs/MEMBER_3_HANDOFF.md - these instructions.

Two repository integration changes are also included: ComplaintManagementSystem.sln adds the test project, and .gitignore ignores generated TestResults directories. If the team's main branch has advanced, preserve newer solution/ignore changes and add the project with `dotnet sln ComplaintManagementSystem.sln add ComplaintManagementSystem.Tests/ComplaintManagementSystem.Tests.csproj`; append `**/TestResults/` to .gitignore if absent. Do not overwrite newer work blindly.

## 2. Before copying - run in your own clone

Use Windows with the .NET 10 SDK installed. Ensure you are a collaborator on the shared GitHub repository and signed in to your own GitHub account. If you do not have a clone yet:

```powershell
git clone https://github.com/divine3054/SEN_PROJECT_GROUP_14.git
cd SEN_PROJECT_GROUP_14
```

If you already have the clone, open a terminal in its repository root. Then:

```powershell
git status
git switch main
git pull --ff-only origin main
git switch -c member3-requirements-tests
git config user.name
git config user.email
```

Start from a clean working tree; preserve or finish any existing work before changing branches. Check that the name is yours and the email is an address verified on YOUR GitHub account (or your own GitHub-provided no-reply address). If incorrect, set repository-local identity using your actual details:

```powershell
git config user.name "YOUR FULL NAME"
git config user.email "YOUR VERIFIED GITHUB EMAIL"
```

The two uppercase strings are instructions to replace, not values to commit. Do not reuse a teammate's identity. Repository-local git config does not change source files or the identity on other repositories.

## 3. Review and run

Copy the listed files into your clone. Read each test and requirement. Check the open issues and proposed scope assumptions with Members 1 and 2; make the changes you own before submitting the contribution. Mark the SRS as validated only after the actual team review.

From the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Test-Backend.ps1
```

The runner finds the repository-local SDK if present, otherwise uses dotnet on PATH. It builds the complete solution and runs tests. First restore requires internet. Expected prepared baseline: 33 passing test cases. Use your actual output as the result; investigate any failures rather than copying that count.

The script regenerates docs/testing/TEST_RESULTS.md from your fresh TRX results. Read it. The raw TestResults folder is ignored. The fixture data is created under your temporary directory, not in the live app's data files. No WinForms screens are launched by these backend tests.

## 4. Commit your reviewed work and push your branch

```powershell
git diff --check
git add .gitignore ComplaintManagementSystem.sln ComplaintManagementSystem.Tests docs/requirements/SRS.md docs/testing/TEST_PLAN.md docs/testing/TEST_RESULTS.md docs/MEMBER_3_HANDOFF.md scripts/Test-Backend.ps1 scripts/Write-TestReport.ps1
git diff --cached --stat
git diff --cached
git commit -m "Member 3: Add requirements, backend tests, and QA evidence"
git push -u origin member3-requirements-tests
git status
git log -1 --format=fuller
```

Open the shared repository on GitHub and create a pull request from member3-requirements-tests into main. Describe the tests/documents, your actual results and the still-unrun manual scenarios. Ask a teammate to review and merge. Verify GitHub attributes the commit to your account. A new test project is a coding contribution; the commit should reflect your review, execution and understanding of the files.

## 5. Work still owned by Member 3 after this first commit

- Carry out and record the manual system/acceptance tests in TEST_PLAN.md; include real inputs, expected/actual results, tester/date and screenshots.
- Create the missing activity diagram and complete feasibility/schedule/risk material with the team.
- Gather all three members' full names/matriculation numbers and verify individual GitHub contributions.
- Assemble the final report PDF, screen-recorded demo and viewable links.
- Record a genuine Phase 2 sign-off only when you have reviewed the design and approved its scope. This handoff does not sign on your behalf.

Member 1 should address UI issues from the test plan; Member 2 should address persistence/recovery issues. Neither member needs to rewrite work that already exists. The repository's existing Live Preview task is a separate Member 1 setup fix; use the CLI runner here for QA.
