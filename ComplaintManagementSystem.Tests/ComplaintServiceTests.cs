using System.Diagnostics;
using System.Runtime.InteropServices;
using ComplaintManagementSystem.Data;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;
using Xunit;
using Xunit.Abstractions;

namespace ComplaintManagementSystem.Tests;

public sealed class ComplaintServiceTests : IDisposable
{
    private readonly TestWorkspace _workspace = new();
    private readonly ITestOutputHelper _output;

    public ComplaintServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SUB01_SubmissionPersistsTrimmedDetailsAndPendingStatus()
    {
        var service = new ComplaintService();
        var before = DateTime.Now;
        var created = service.SubmitComplaint("customer1", " Incorrect bill ", ComplaintCategory.Billing, " Extra charge ");
        var saved = Assert.Single(new StorageService().LoadComplaints());

        Assert.Equal(created.ReferenceNumber, saved.ReferenceNumber);
        Assert.Equal("customer1", saved.SubmittedBy);
        Assert.Equal("Incorrect bill", saved.Title);
        Assert.Equal("Extra charge", saved.Description);
        Assert.Equal(ComplaintCategory.Billing, saved.Category);
        Assert.Equal(ComplaintStatus.Pending, saved.Status);
        Assert.Equal("Awaiting review.", saved.AgentNotes);
        Assert.InRange(saved.CreatedAt, before, DateTime.Now);
    }

    [Theory]
    [InlineData("", "Details")]
    [InlineData("   ", "Details")]
    [InlineData("Title", "")]
    [InlineData("Title", "\t")]
    public void SUB02_MissingRequiredFieldsDoNotCreateComplaint(string title, string description)
    {
        var service = new ComplaintService();
        Assert.Throws<ArgumentException>(() => service.SubmitComplaint("customer1", title, ComplaintCategory.Service, description));
        Assert.Empty(service.GetAllComplaints());
        Assert.Empty(new StorageService().LoadComplaints());
    }

    [Theory]
    [InlineData(ComplaintCategory.Billing)]
    [InlineData(ComplaintCategory.Technical)]
    [InlineData(ComplaintCategory.Service)]
    [InlineData(ComplaintCategory.GeneralFeedback)]
    public void CAT01_AllDefinedCategoriesCanBeSubmitted(ComplaintCategory category)
    {
        var created = new ComplaintService().SubmitComplaint("customer1", "Issue or feedback", category, "Details");
        Assert.Equal(category, new StorageService().LoadComplaints().Single().Category);
        Assert.Equal(category, created.Category);
    }

    [Fact]
    public void REF01_ReferencesRemainSequentialAfterServiceRestart()
    {
        var service = new ComplaintService();
        string prefix = $"CMP-{DateTime.Now.Year}-";
        var first = service.SubmitComplaint("customer1", "First", ComplaintCategory.Billing, "Details");
        var second = service.SubmitComplaint("customer1", "Second", ComplaintCategory.Service, "Details");
        var third = new ComplaintService().SubmitComplaint("customer1", "Third", ComplaintCategory.Technical, "Details");

        Assert.Equal(prefix + "0001", first.ReferenceNumber);
        Assert.Equal(prefix + "0002", second.ReferenceNumber);
        Assert.Equal(prefix + "0003", third.ReferenceNumber);
        Assert.Equal(3, new StorageService().LoadComplaints().Select(c => c.ReferenceNumber).Distinct().Count());
    }

    [Fact]
    public void TRACK01_ReferenceLookupTrimsInputAndIgnoresCase()
    {
        var service = new ComplaintService();
        var created = service.SubmitComplaint("customer1", "Issue", ComplaintCategory.Service, "Details");
        var found = service.TrackByReference(" " + created.ReferenceNumber.ToLowerInvariant() + " ");
        Assert.NotNull(found);
        Assert.Equal(created.ReferenceNumber, found.ReferenceNumber);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("")]
    [InlineData("   ")]
    public void TRACK02_UnknownOrBlankReferenceHasNoMatch(string reference)
    {
        Assert.Null(new ComplaintService().TrackByReference(reference));
    }

    [Fact]
    public void FILTER01_CategoryStatusAndCombinedFiltersReturnMatchingRecords()
    {
        var service = new ComplaintService();
        service.SubmitComplaint("customer1", "Pending bill", ComplaintCategory.Billing, "Details");
        var resolved = service.SubmitComplaint("customer1", "Resolved bill", ComplaintCategory.Billing, "Details");
        service.SubmitComplaint("customer1", "Pending technical", ComplaintCategory.Technical, "Details");
        service.UpdateComplaintStatus(resolved.ReferenceNumber, ComplaintStatus.Resolved, ComplaintCategory.Billing, "Fixed");

        Assert.Equal(3, service.GetAllComplaints().Count);
        Assert.Equal(2, service.GetAllComplaints(ComplaintCategory.Billing).Count);
        Assert.Equal(resolved.ReferenceNumber, Assert.Single(service.GetAllComplaints(statusFilter: ComplaintStatus.Resolved)).ReferenceNumber);
        Assert.Equal("Pending bill", Assert.Single(service.GetAllComplaints(ComplaintCategory.Billing, ComplaintStatus.Pending)).Title);
        Assert.Empty(service.GetAllComplaints(ComplaintCategory.Technical, ComplaintStatus.Closed));
    }

    [Fact]
    public void FILTER02_ListIsSortedByNewestCreationTime()
    {
        var older = new Complaint { ReferenceNumber = "OLDER", CreatedAt = new DateTime(2026, 1, 1) };
        var newer = new Complaint { ReferenceNumber = "NEWER", CreatedAt = new DateTime(2026, 2, 1) };
        new StorageService().SaveComplaints(new List<Complaint> { older, newer });
        Assert.Equal(new[] { "NEWER", "OLDER" }, new ComplaintService().GetAllComplaints().Select(c => c.ReferenceNumber));
    }

    [Fact]
    public void UPDATE01_StatusCategoryNotesAndUpdatedTimeSurviveRestart()
    {
        var service = new ComplaintService();
        var created = service.SubmitComplaint("customer1", "Issue", ComplaintCategory.Billing, "Details");
        var before = DateTime.Now;
        Assert.True(service.UpdateComplaintStatus(created.ReferenceNumber, ComplaintStatus.Resolved, ComplaintCategory.Service, " Fixed "));

        var saved = new ComplaintService().TrackByReference(created.ReferenceNumber);
        Assert.NotNull(saved);
        Assert.Equal(ComplaintStatus.Resolved, saved.Status);
        Assert.Equal(ComplaintCategory.Service, saved.Category);
        Assert.Equal("Fixed", saved.AgentNotes);
        Assert.Equal(created.CreatedAt, saved.CreatedAt);
        Assert.InRange(saved.UpdatedAt, before, DateTime.Now);
    }

    [Fact]
    public void UPDATE02_BlankNotesKeepExistingNotes()
    {
        var service = new ComplaintService();
        var created = service.SubmitComplaint("customer1", "Issue", ComplaintCategory.Service, "Details");
        service.UpdateComplaintStatus(created.ReferenceNumber, ComplaintStatus.InProgress, ComplaintCategory.Service, "Investigating");
        service.UpdateComplaintStatus(created.ReferenceNumber, ComplaintStatus.Resolved, ComplaintCategory.Service, "   ");
        Assert.Equal("Investigating", new ComplaintService().TrackByReference(created.ReferenceNumber)!.AgentNotes);
    }

    [Fact]
    public void UPDATE03_UnknownReferenceReturnsFalseWithoutChangingRecords()
    {
        var service = new ComplaintService();
        service.SubmitComplaint("customer1", "Issue", ComplaintCategory.Service, "Details");
        string before = File.ReadAllText("complaints.json");
        Assert.False(service.UpdateComplaintStatus("missing", ComplaintStatus.Closed, ComplaintCategory.Billing, "Notes"));
        Assert.Equal(before, File.ReadAllText("complaints.json"));
    }

    [Fact]
    public void PERF01_FilterAndLookupOneThousandComplaintsWithinTwoSeconds()
    {
        int year = DateTime.Now.Year;
        var complaints = Enumerable.Range(1, 1000)
            .Select(i => new Complaint
            {
                ReferenceNumber = $"CMP-{year}-{i:D4}",
                SubmittedBy = "customer1",
                Title = $"Issue {i}",
                Category = i % 4 == 0 ? ComplaintCategory.Billing : ComplaintCategory.Technical,
                Description = "Performance test data",
                Status = i % 4 == 0 ? ComplaintStatus.Resolved : ComplaintStatus.Pending,
                CreatedAt = DateTime.Now.AddMinutes(-i),
                UpdatedAt = DateTime.Now.AddMinutes(-i)
            })
            .ToList();
        new StorageService().SaveComplaints(complaints);
        var service = new ComplaintService();

        var filterTimer = Stopwatch.StartNew();
        var filtered = service.GetAllComplaints(ComplaintCategory.Billing, ComplaintStatus.Resolved);
        filterTimer.Stop();

        var lookupTimer = Stopwatch.StartNew();
        var found = service.TrackByReference($"CMP-{year}-1000");
        lookupTimer.Stop();

        _output.WriteLine(
            $"OS={RuntimeInformation.OSDescription}; Runtime={RuntimeInformation.FrameworkDescription}; " +
            $"Logical CPUs={Environment.ProcessorCount}; " +
            $"Filter1000Ms={filterTimer.Elapsed.TotalMilliseconds:F3}; " +
            $"Lookup1000Ms={lookupTimer.Elapsed.TotalMilliseconds:F3}");

        Assert.Equal(250, filtered.Count);
        Assert.NotNull(found);
        Assert.True(filterTimer.Elapsed < TimeSpan.FromSeconds(2),
            $"Filtering 1,000 records took {filterTimer.Elapsed.TotalMilliseconds:F3} ms.");
        Assert.True(lookupTimer.Elapsed < TimeSpan.FromSeconds(2),
            $"Looking up a record in 1,000 records took {lookupTimer.Elapsed.TotalMilliseconds:F3} ms.");
    }

    public void Dispose() => _workspace.Dispose();
}
