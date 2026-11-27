# Automated backend test results

Source base revision at test run: 57df846.
Includes local uncommitted changes; results apply to the tested working tree, not the remote commit alone.
Run start (as recorded by TRX): 2026-11-27T18:34:25.4294760+01:00.
Run finish (as recorded by TRX): 2026-11-27T18:34:28.1866192+01:00.

Total: 34. Executed: 34. Passed: 34. Failed: 0. Not executed: 0.

These results were generated from actual xUnit/VSTest output. They cover backend unit/integration checks, including a filter/lookup microbenchmark. They do not measure WinForms grid rendering or establish stakeholder acceptance or phase approval.

Raw local evidence: TestResults/backend-tests.trx (ignored by Git). Reproduce with scripts/Test-Backend.ps1. Member 3 must rerun on their own machine before committing their verification record.

## Executed cases

- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH01_SeededCredentialsReturnExpectedRole(username: "admin1", password: "admin123", role: Admin) - **Passed**; duration 00:00:00.0071864.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH01_SeededCredentialsReturnExpectedRole(username: "agent1", password: "agent123", role: SupportAgent) - **Passed**; duration 00:00:00.0085931.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH01_SeededCredentialsReturnExpectedRole(username: "customer1", password: "pass123", role: Customer) - **Passed**; duration 00:00:00.0088105.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH02_UsernameComparisonIgnoresCase - **Passed**; duration 00:00:00.0069439.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "", password: "") - **Passed**; duration 00:00:00.0069996.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "customer1", password: "PASS123") - **Passed**; duration 00:00:00.0069743.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "customer1", password: "wrong") - **Passed**; duration 00:00:00.0074485.
- ComplaintManagementSystem.Tests.AuthenticationTests.AUTH03_InvalidCredentialsAreRejected(username: "unknown", password: "pass123") - **Passed**; duration 00:00:00.0060292.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: Billing) - **Passed**; duration 00:00:00.0314401.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: GeneralFeedback) - **Passed**; duration 00:00:00.0261995.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: Service) - **Passed**; duration 00:00:00.0250570.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.CAT01_AllDefinedCategoriesCanBeSubmitted(category: Technical) - **Passed**; duration 00:00:00.0236519.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.FILTER01_CategoryStatusAndCombinedFiltersReturnMatchingRecords - **Passed**; duration 00:00:00.1286254.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.FILTER02_ListIsSortedByNewestCreationTime - **Passed**; duration 00:00:00.0878266.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.PERF01_FilterAndLookupOneThousandComplaintsWithinTwoSeconds - **Passed**; duration 00:00:00.0496115.
  Output: OS=Microsoft Windows 10.0.26300; Runtime=.NET 10.0.15; Logical CPUs=16; Filter1000Ms=0.431; Lookup1000Ms=0.085
- ComplaintManagementSystem.Tests.ComplaintServiceTests.REF01_ReferencesRemainSequentialAfterServiceRestart - **Passed**; duration 00:00:00.0987527.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB01_SubmissionPersistsTrimmedDetailsAndPendingStatus - **Passed**; duration 00:00:00.0271044.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "   ", description: "Details") - **Passed**; duration 00:00:00.0988663.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "", description: "Details") - **Passed**; duration 00:00:00.0091118.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "Title", description: "") - **Passed**; duration 00:00:00.0083600.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.SUB02_MissingRequiredFieldsDoNotCreateComplaint(title: "Title", description: "\t") - **Passed**; duration 00:00:00.0087811.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK01_ReferenceLookupTrimsInputAndIgnoresCase - **Passed**; duration 00:00:00.0176110.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK02_UnknownOrBlankReferenceHasNoMatch(reference: "   ") - **Passed**; duration 00:00:00.0059962.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK02_UnknownOrBlankReferenceHasNoMatch(reference: "") - **Passed**; duration 00:00:00.0057723.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.TRACK02_UnknownOrBlankReferenceHasNoMatch(reference: "missing") - **Passed**; duration 00:00:00.0065737.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.UPDATE01_StatusCategoryNotesAndUpdatedTimeSurviveRestart - **Passed**; duration 00:00:00.0594942.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.UPDATE02_BlankNotesKeepExistingNotes - **Passed**; duration 00:00:00.0747097.
- ComplaintManagementSystem.Tests.ComplaintServiceTests.UPDATE03_UnknownReferenceReturnsFalseWithoutChangingRecords - **Passed**; duration 00:00:00.0367624.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE01_MissingComplaintFileReturnsEmptyList - **Passed**; duration 00:00:00.0026614.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE02_UnicodeAndMultilineComplaintDetailsRoundTrip - **Passed**; duration 00:00:00.0080246.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE03_DefaultUsersArePersistedOnceWithHashedPasswords - **Passed**; duration 00:00:00.0426257.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE04_SavedUsersReplaceSeedAccounts - **Passed**; duration 00:00:00.0253377.
- ComplaintManagementSystem.Tests.StorageServiceTests.STORE05_MalformedJsonRaisesErrorAndPreservesFile - **Passed**; duration 00:00:00.0069870.
- ComplaintManagementSystem.Tests.StorageServiceTests.UNIT01_PasswordHashMatchesKnownSha256Vector - **Passed**; duration 00:00:00.0023123.

## Manual and acceptance evidence

Assisted UI observations, remaining manual scenarios, stakeholder acceptance, and evidence limitations are recorded in TEST_PLAN.md. Member 3 phase approval is recorded separately in docs/phase-2/PROJECT_LOG.md.
