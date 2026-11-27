using Xunit;

// StorageService uses relative filenames, so changing the process directory must
// never happen concurrently with another test in this assembly.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ComplaintManagementSystem.Tests;

public sealed class TestWorkspace : IDisposable
{
    private readonly string _originalDirectory = Directory.GetCurrentDirectory();
    private readonly string _testRoot = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "ComplaintManagementSystem.Tests"));

    public string DirectoryPath { get; }

    public TestWorkspace()
    {
        DirectoryPath = Path.Combine(_testRoot, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DirectoryPath);
        Directory.SetCurrentDirectory(DirectoryPath);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDirectory);
        // Only remove the unique test directory created by this instance.
        string resolvedPath = Path.GetFullPath(DirectoryPath);
        if (!resolvedPath.StartsWith(_testRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Test cleanup path is outside the test root.");

        Directory.Delete(resolvedPath, recursive: true);
    }
}
