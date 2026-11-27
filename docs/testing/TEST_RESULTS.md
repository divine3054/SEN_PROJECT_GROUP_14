# Automated backend test results

Source revision before this uncommitted contribution: 8e4b9d5.
Run start (as recorded by TRX): 2026-11-27T03:29:26.0686595+01:00.
Run finish (as recorded by TRX): 2026-11-27T03:29:29.2369382+01:00.

Total: 33. Executed: 33. Passed: 33. Failed: 0. Not executed: 0.

These results were generated from actual xUnit/VSTest output. They cover backend unit/integration checks, not WinForms system testing, performance, stakeholder acceptance or phase approval.

Raw local evidence: TestResults/backend-tests.trx (ignored by Git). Reproduce with scripts/Test-Backend.ps1. Member 3 must rerun on their own machine before committing their verification record.

## Executed cases

- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH01_SeededCredentialsReturnExpectedRole(username: "admin1", password: "admin123", role: Admin) - **Passed**; duration 00:00:00.0068364.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH01_SeededCredentialsReturnExpectedRole(username: "agent1", password: "agent123", role: SupportAgent) - **Passed**; duration 00:00:00.0063457.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH01_SeededCredentialsReturnExpectedRole(username: "customer1", password: "pass123", role: Customer) - **Passed**; duration 00:00:00.1697104.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH02_UsernameComparisonIgnoresCase - **Passed**; duration 00:00:00.0065593.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "", password: "") - **Passed**; duration 00:00:00.0068831.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "customer1", password: "PASS123") - **Passed**; duration 00:00:00.0062344.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "customer1", password: "wrong") - **Passed**; duration 00:00:00.0044593.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "unknown", password: "pass123") - **Passed**; duration 00:00:00.0049800.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: Billing) - **Passed**; duration 00:00:00.0157864.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: GeneralFeedback) - **Passed**; duration 00:00:00.0084069.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: Service) - **Passed**; duration 00:00:00.0076417.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: Technical) - **Passed**; duration 00:00:00.0180294.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.FILTER01_CategoryStatusAndCombinedFiltersReturnMatchingRecords - **Passed**; duration 00:00:00.0342020.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.FILTER02_ListIsSortedByNewestCreationTime - **Passed**; duration 00:00:00.1476383.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.REF01_ReferencesRemainSequentialAfterServiceRestart - **Passed**; duration 00:00:00.0397912.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB01_SubmissionPersistsTrimmedDetailsAndPendingStatus - **Passed**; duration 00:00:00.0097255.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "   ", description: "Details") - **Passed**; duration 00:00:00.0164773.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "", description: "Details") - **Passed**; duration 00:00:00.0060701.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "Title", description: "") - **Passed**; duration 00:00:00.0058849.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "Title", description: "\t") - **Passed**; duration 00:00:00.0051742.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK01_ReferenceLookupTrimsInputAndIgnoresCase - **Passed**; duration 00:00:00.0105501.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK02_UnknownOrBlankReferenceHasNoMatch(reference: "   ") - **Passed**; duration 00:00:00.0072219.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK02_UnknownOrBlankReferenceHasNoMatch(reference: "") - **Passed**; duration 00:00:00.0060625.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK02_UnknownOrBlankReferenceHasNoMatch(reference: "missing") - **Passed**; duration 00:00:00.0071156.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.UPDATE01_StatusCategoryNotesAndUpdatedTimeSurviveRestart - **Passed**; duration 00:00:00.0226515.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.UPDATE02_BlankNotesKeepExistingNotes - **Passed**; duration 00:00:00.0121129.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.UPDATE03_UnknownReferenceReturnsFalseWithoutChangingRecords - **Passed**; duration 00:00:00.0137378.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE01_MissingComplaintFileReturnsEmptyList - **Passed**; duration 00:00:00.0039274.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE02_UnicodeAndMultilineComplaintDetailsRoundTrip - **Passed**; duration 00:00:00.0068108.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE03_DefaultUsersArePersistedOnceWithHashedPasswords - **Passed**; duration 00:00:00.0538835.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE04_SavedUsersReplaceSeedAccounts - **Passed**; duration 00:00:00.0077314.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE05_MalformedJsonRaisesErrorAndPreservesFile - **Passed**; duration 00:00:00.0126258.
- ComplaintManagementSystem.Tests.StorageServiceTests.UNIT01_PasswordHashMatchesKnownSha256Vector - **Passed**; duration 00:00:00.0039438.

## Manual and acceptance evidence

MAN-01 through MAN-07, PERF-01 and stakeholder acceptance remain NOT RUN. See TEST_PLAN.md. No human tester signature or Member 3 approval is inferred from this automated run.
