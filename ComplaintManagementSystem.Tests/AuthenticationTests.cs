using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;
using Xunit;

namespace ComplaintManagementSystem.Tests;

public sealed class AuthenticationTests : IDisposable
{
    private readonly TestWorkspace _workspace = new();

    [Theory]
    [InlineData("customer1", "pass123", UserRole.Customer)]
    [InlineData("agent1", "agent123", UserRole.SupportAgent)]
    [InlineData("admin1", "admin123", UserRole.Admin)]
    public void AUTH01_SeededCredentialsReturnExpectedRole(string username, string password, UserRole role)
    {
        var user = new ComplaintService().Authenticate(username, password);
        Assert.NotNull(user);
        Assert.Equal(role, user.Role);
    }

    [Fact]
    public void AUTH02_UsernameComparisonIgnoresCase()
    {
        Assert.NotNull(new ComplaintService().Authenticate("CUSTOMER1", "pass123"));
    }

    [Theory]
    [InlineData("customer1", "wrong")]
    [InlineData("unknown", "pass123")]
    [InlineData("customer1", "PASS123")]
    [InlineData("", "")]
    public void AUTH03_InvalidCredentialsAreRejected(string username, string password)
    {
        Assert.Null(new ComplaintService().Authenticate(username, password));
    }

    public void Dispose() => _workspace.Dispose();
}
