# Automated backend test results

Source revision before this uncommitted contribution: e659b7f.
Run start (as recorded by TRX): 2026-11-27T04:25:15.4271995+01:00.
Run finish (as recorded by TRX): 2026-11-27T04:25:20.2366718+01:00.

Total: 34. Executed: 34. Passed: 34. Failed: 0. Not executed: 0.

These results were generated from actual xUnit/VSTest output. They cover backend unit/integration checks, including a filter/lookup microbenchmark. They do not measure WinForms grid rendering or establish stakeholder acceptance or phase approval.

Raw local evidence: TestResults/backend-tests.trx (ignored by Git). Reproduce with scripts/Test-Backend.ps1. Member 3 must rerun on their own machine before committing their verification record.

## Executed cases

- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH01_SeededCredentialsReturnExpectedRole(username: "admin1", password: "admin123", role: Admin) - **Passed**; duration 00:00:00.0052589.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH01_SeededCredentialsReturnExpectedRole(username: "agent1", password: "agent123", role: SupportAgent) - **Passed**; duration 00:00:00.0131075.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH01_SeededCredentialsReturnExpectedRole(username: "customer1", password: "pass123", role: Customer) - **Passed**; duration 00:00:00.2301710.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH02_UsernameComparisonIgnoresCase - **Passed**; duration 00:00:00.0523882.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "", password: "") - **Passed**; duration 00:00:00.0393959.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "customer1", password: "PASS123") - **Passed**; duration 00:00:00.0064631.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "customer1", password: "wrong") - **Passed**; duration 00:00:00.0061264.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "unknown", password: "pass123") - **Passed**; duration 00:00:00.0068113.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: Billing) - **Passed**; duration 00:00:00.0057291.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: GeneralFeedback) - **Passed**; duration 00:00:00.0081666.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: Service) - **Passed**; duration 00:00:00.0053378.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: Technical) - **Passed**; duration 00:00:00.0103248.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.FILTER01_CategoryStatusAndCombinedFiltersReturnMatchingRecords - **Passed**; duration 00:00:00.0127095.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.FILTER02_ListIsSortedByNewestCreationTime - **Passed**; duration 00:00:00.0121447.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.PERF01_FilterAndLookupOneThousandComplaintsWithinTwoSeconds - **Passed**; duration 00:00:00.0840417.
  Output: OS=Microsoft Windows 10.0.26200; Runtime=.NET 10.0.15; Logical CPUs=8; Filter1000Ms=1.056; Lookup1000Ms=0.285
- ComplaintManagementSystem.Tests.ComplaintServiceTests.REF01_ReferencesRemainSequentialAfterServiceRestart - **Passed**; duration 00:00:00.0118641.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB01_SubmissionPersistsTrimmedDetailsAndPendingStatus - **Passed**; duration 00:00:00.0071225.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "   ", description: "Details") - **Passed**; duration 00:00:00.0100661.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "", description: "Details") - **Passed**; duration 00:00:00.0048359.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "Title", description: "") - **Passed**; duration 00:00:00.0048068.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "Title", description: "\t") - **Passed**; duration 00:00:00.0046788.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK01_ReferenceLookupTrimsInputAndIgnoresCase - **Passed**; duration 00:00:00.0064788.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK02_UnknownOrBlankReferenceHasNoMatch(reference: "   ") - **Passed**; duration 00:00:00.0051389.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK02_UnknownOrBlankReferenceHasNoMatch(reference: "") - **Passed**; duration 00:00:00.0034993.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK02_UnknownOrBlankReferenceHasNoMatch(reference: "missing") - **Passed**; duration 00:00:00.0126018.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.UPDATE01_StatusCategoryNotesAndUpdatedTimeSurviveRestart - **Passed**; duration 00:00:00.0095769.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.UPDATE02_BlankNotesKeepExistingNotes - **Passed**; duration 00:00:00.0141297.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.UPDATE03_UnknownReferenceReturnsFalseWithoutChangingRecords - **Passed**; duration 00:00:00.0068118.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE01_MissingComplaintFileReturnsEmptyList - **Passed**; duration 00:00:00.0903726.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE02_UnicodeAndMultilineComplaintDetailsRoundTrip - **Passed**; duration 00:00:00.0693464.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE03_DefaultUsersArePersistedOnceWithHashedPasswords - **Passed**; duration 00:00:00.7634440.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE04_SavedUsersReplaceSeedAccounts - **Passed**; duration 00:00:00.1982096.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE05_MalformedJsonRaisesErrorAndPreservesFile - **Passed**; duration 00:00:00.2335561.
- ComplaintManagementSystem.Tests.StorageServiceTests.UNIT01_PasswordHashMatchesKnownSha256Vector - **Passed**; duration 00:00:00.0054666.

## Manual and acceptance evidence

Assisted UI observations, remaining manual scenarios, stakeholder acceptance, and evidence limitations are recorded in TEST_PLAN.md. Member 3 phase approval is recorded separately in docs/phase-2/PROJECT_LOG.md.
