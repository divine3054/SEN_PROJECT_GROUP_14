using System.Text.Json;
using ComplaintManagementSystem.Data;
using ComplaintManagementSystem.Models;
using Xunit;

namespace ComplaintManagementSystem.Tests;

public sealed class StorageServiceTests : IDisposable
{
    private readonly TestWorkspace _workspace = new();

    [Fact]
    public void STORE01_MissingComplaintFileReturnsEmptyList()
    {
        Assert.Empty(new StorageService().LoadComplaints());
    }

    [Fact]
    public void STORE02_UnicodeAndMultilineComplaintDetailsRoundTrip()
    {
        var complaint = new Complaint
        {
            ReferenceNumber = "CMP-2026-0001", SubmittedBy = "customer1",
            Title = "Feedback: caf\u00e9", Description = "Line one\nLine two: \"quoted\"",
            Category = ComplaintCategory.GeneralFeedback, Status = ComplaintStatus.InProgress,
            AgentNotes = "Reviewing feedback", CreatedAt = new DateTime(2026, 1, 1, 9, 0, 0),
            UpdatedAt = new DateTime(2026, 1, 2, 10, 0, 0)
        };
        new StorageService().SaveComplaints(new List<Complaint> { complaint });
        var saved = Assert.Single(new StorageService().LoadComplaints());
        Assert.Equal(complaint.ReferenceNumber, saved.ReferenceNumber);
        Assert.Equal(complaint.SubmittedBy, saved.SubmittedBy);
        Assert.Equal(complaint.Title, saved.Title);
        Assert.Equal(complaint.Description, saved.Description);
        Assert.Equal(complaint.Category, saved.Category);
        Assert.Equal(complaint.Status, saved.Status);
        Assert.Equal(complaint.AgentNotes, saved.AgentNotes);
        Assert.Equal(complaint.CreatedAt, saved.CreatedAt);
        Assert.Equal(complaint.UpdatedAt, saved.UpdatedAt);
    }

    [Fact]
    public void STORE03_DefaultUsersArePersistedOnceWithHashedPasswords()
    {
        var firstLoad = new StorageService().LoadUsers();
        var secondLoad = new StorageService().LoadUsers();
        Assert.Equal(3, firstLoad.Count);
        Assert.Equal(firstLoad.Select(u => u.Id), secondLoad.Select(u => u.Id));
        var customer = firstLoad.Single(u => u.Username == "customer1");
        Assert.Equal(StorageService.HashPassword("pass123"), customer.PasswordHash);
        Assert.DoesNotContain("pass123", File.ReadAllText("users.json"));
    }

    [Fact]
    public void STORE04_SavedUsersReplaceSeedAccounts()
    {
        var user = new User { Username = "qa-agent", PasswordHash = StorageService.HashPassword("sample-password"), Role = UserRole.SupportAgent };
        new StorageService().SaveUsers(new List<User> { user });
        var saved = Assert.Single(new StorageService().LoadUsers());
        Assert.Equal(user.Id, saved.Id);
        Assert.Equal(user.Username, saved.Username);
        Assert.Equal(user.PasswordHash, saved.PasswordHash);
        Assert.Equal(user.Role, saved.Role);
    }

    [Fact]
    public void UNIT01_PasswordHashMatchesKnownSha256Vector()
    {
        // Published SHA-256 digest for the UTF-8 string "abc", encoded as Base64.
        Assert.Equal("ungWv48Bz+pBQUDeXa4iI7ADYaOWF3qctBD/YfIAFa0=", StorageService.HashPassword("abc"));
    }

    [Fact]
    public void STORE05_MalformedJsonRaisesErrorAndPreservesFile()
    {
        const string invalidJson = "{not-valid-json";
        File.WriteAllText("complaints.json", invalidJson);
        Assert.Throws<JsonException>(() => new StorageService().LoadComplaints());
        Assert.Equal(invalidJson, File.ReadAllText("complaints.json"));
    }

    public void Dispose() => _workspace.Dispose();
}
