using System.Text.Json;
using ComplaintManagementSystem.Data;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem.BackendReliabilityChecks;

internal static class Program
{
    private static readonly List<Result> Results = new();

    private static int Main(string[] args)
    {
        string output = Path.GetFullPath(args.Length == 1 ? args[0] : Path.Combine(Environment.CurrentDirectory, ".tools", "backend-reliability"));
        Directory.CreateDirectory(output);

        Run("REL-01 Failed submission leaves cache and disk unchanged; reserved reference is not reused", () =>
        {
            var storage = new FailingStorage(); var service = new ComplaintService(storage);
            var first = Submit(service);
            string before = File.ReadAllText("complaints.json");
            storage.FailWrites = true;
            Throws<IOException>(() => Submit(service));
            Require(service.GetAllComplaints().Count == 1 && File.ReadAllText("complaints.json") == before, "A failed write changed records.");
            storage.FailWrites = false;
            var next = Submit(service);
            Require(next.ReferenceNumber.EndsWith("0003"), "Reserved failed reference was reused.");
            Require(new ComplaintService().GetAllComplaints().Count == 2, "Retry did not persist exactly two successful tickets.");
        });
        Run("REL-02 Failed status update leaves cached and saved fields unchanged", () =>
        {
            var storage = new FailingStorage(); var service = new ComplaintService(storage);
            var ticket = Submit(service); string before = File.ReadAllText("complaints.json");
            storage.FailWrites = true;
            Throws<IOException>(() => service.UpdateComplaintStatus(ticket.ReferenceNumber, ComplaintStatus.Resolved, ComplaintCategory.Service, "Unsaved notes"));
            var cached = service.TrackByReference(ticket.ReferenceNumber)!;
            Require(cached.Status == ComplaintStatus.Pending && cached.Category == ComplaintCategory.Billing && cached.AgentNotes == "Awaiting review.", "Failed update leaked into cache.");
            Require(File.ReadAllText("complaints.json") == before && cached.UpdatedAt == ticket.UpdatedAt, "Failed update changed persistent data or timestamp.");
        });
        Run("REL-03 Migration allocates above largest existing reference instead of record count", () =>
        {
            var storage = new StorageService();
            storage.SaveComplaints(new List<Complaint> { Ticket(2), Ticket(19) });
            Require(Submit(new ComplaintService()).ReferenceNumber.EndsWith("0020"), "Migration ignored the highest suffix.");
        });
        Run("REL-04 Deleting the highest ticket does not reuse a previously issued reference", () =>
        {
            var service = new ComplaintService(); var first = Submit(service); Submit(service);
            new StorageService().SaveComplaints(new List<Complaint> { first });
            Require(Submit(new ComplaintService()).ReferenceNumber.EndsWith("0003"), "Deleted highest reference was reused.");
        });
        Run("REL-05 Reference sequence starts independently for a new year", () =>
        {
            new StorageService().SaveComplaints(new List<Complaint> { new() { ReferenceNumber = $"CMP-{DateTime.Now.Year - 1}-9999" } });
            Require(Submit(new ComplaintService()).ReferenceNumber == $"CMP-{DateTime.Now.Year}-0001", "Previous year affected this year's suffix.");
        });
        Run("REL-06 Reference formatting expands beyond four digits without wrapping", () =>
        {
            new StorageService().SaveComplaints(new List<Complaint> { Ticket(9999) });
            Require(Submit(new ComplaintService()).ReferenceNumber.EndsWith("10000"), "Sequence wrapped at four digits.");
        });
        Run("REL-07 Concurrent service instances in one process preserve all distinct submissions", () =>
        {
            var services = Enumerable.Range(0, 12).Select(_ => new ComplaintService()).ToArray();
            var tasks = services.Select(service => Task.Run(() => Submit(service))).ToArray();
            Task.WaitAll(tasks);
            var saved = new StorageService().LoadComplaints();
            Require(saved.Count == 12 && saved.Select(c => c.ReferenceNumber).Distinct().Count() == 12, "Concurrent service writes lost or duplicated records.");
        });
        Run("REL-08 Returned complaints and users cannot mutate private cached records", () =>
        {
            var service = new ComplaintService(); var ticket = Submit(service);
            ticket.Title = "Changed outside service";
            service.GetAllComplaints()[0].Status = ComplaintStatus.Closed;
            service.TrackByReference(ticket.ReferenceNumber)!.AgentNotes = "External edit";
            var user = service.Authenticate("customer1", "pass123")!; user.Username = "Changed";
            var actual = service.TrackByReference(ticket.ReferenceNumber)!;
            Require(actual.Title == "Billing query" && actual.Status == ComplaintStatus.Pending && actual.AgentNotes == "Awaiting review.", "Mutable return values leaked into cached complaints.");
            Require(service.Authenticate("customer1", "pass123") is not null, "Mutable return value leaked into user cache.");
        });
        Run("REL-09 Empty submitter and undefined enum values are rejected without writes", () =>
        {
            var service = new ComplaintService();
            Throws<ArgumentException>(() => service.SubmitComplaint(" ", "Title", ComplaintCategory.Service, "Details"));
            Throws<ArgumentException>(() => service.SubmitComplaint("customer1", "Title", (ComplaintCategory)999, "Details"));
            Throws<ArgumentException>(() => service.UpdateComplaintStatus("missing", (ComplaintStatus)999, ComplaintCategory.Service, "Notes"));
            Throws<ArgumentException>(() => service.GetAllComplaints(statusFilter: (ComplaintStatus)999));
            Require(service.GetAllComplaints().Count == 0 && !File.Exists("complaints.json") && !File.Exists("reference-sequence.json"), "Validation performed unexpected persistent writes.");
        });
        Run("REL-10 Null or blank authentication/tracking inputs fail safely", () =>
        {
            var service = new ComplaintService();
            Require(service.Authenticate(null!, "pass123") is null && service.Authenticate("customer1", null!) is null, "Null login was accepted or crashed.");
            Require(service.TrackByReference(null!) is null && !service.UpdateComplaintStatus(" ", ComplaintStatus.Resolved, ComplaintCategory.Service, "Notes"), "Blank reference did not fail safely.");
        });
        Run("REL-11 Atomic replacement keeps a readable previous-version backup", () =>
        {
            var service = new ComplaintService(); Submit(service); Submit(service);
            Require(new StorageService().LoadComplaints().Count == 2, "Latest primary was not saved.");
            var backup = JsonSerializer.Deserialize<List<Complaint>>(File.ReadAllText("complaints.json.bak"))!;
            Require(backup.Count == 1 && Directory.GetFiles(Environment.CurrentDirectory, "*.tmp").Length == 0, "Backup or temporary-file cleanup is wrong.");
        });
        Run("REL-12 A blocked replacement preserves the old primary and cleans its temporary file", () =>
        {
            var service = new ComplaintService(); Submit(service);
            string before = File.ReadAllText("complaints.json");
            Directory.CreateDirectory("complaints.json.bak");
            ThrowsIo(() => Submit(service));
            Require(File.ReadAllText("complaints.json") == before && service.GetAllComplaints().Count == 1, "Failed replacement damaged the prior primary or cache.");
            Require(Directory.GetFiles(Environment.CurrentDirectory, "*.tmp").Length == 0, "Temporary save file was left behind.");
        });
        Run("REL-13 Malformed/null/list-shape records are reported and left untouched", () =>
        {
            foreach (string json in new[] { "{broken", "null", "{}", "[null]" })
            {
                File.WriteAllText("complaints.json", json);
                Throws<JsonException>(() => new StorageService().LoadComplaints());
                Require(File.ReadAllText("complaints.json") == json, "Invalid source file was changed.");
            }
        });
        Run("REL-14 Backup restoration validates data and preserves the damaged primary", () =>
        {
            var service = new ComplaintService(); Submit(service); Submit(service);
            File.WriteAllText("complaints.json", "{damaged");
            var storage = new StorageService();
            Throws<JsonException>(() => storage.LoadComplaints());
            Require(storage.RestoreComplaintsBackup() && storage.LoadComplaints().Count == 1, "Validated backup was not restored.");
            Require(File.ReadAllText(Directory.GetFiles(Environment.CurrentDirectory, "complaints.json.corrupt-*").Single()) == "{damaged", "Damaged primary was not retained for diagnosis.");
            Require(JsonSerializer.Deserialize<List<Complaint>>(File.ReadAllText("complaints.json.bak"))!.Count == 1, "Restoration overwrote the validated backup.");
            Require(Submit(new ComplaintService()).ReferenceNumber.EndsWith("0003"), "Backup recovery rewound the reference counter.");
        });
        Run("REL-15 Invalid backup cannot overwrite a primary; absent backup reports false", () =>
        {
            var storage = new StorageService(); Require(!storage.RestoreComplaintsBackup(), "Missing backup reported success.");
            var service = new ComplaintService(storage); Submit(service);
            string before = File.ReadAllText("complaints.json");
            File.WriteAllText("complaints.json.bak", "null");
            Throws<JsonException>(() => storage.RestoreComplaintsBackup());
            Require(File.ReadAllText("complaints.json") == before, "Invalid backup changed the primary.");
        });
        Run("REL-16 Missing primary with an existing backup is never silently replaced with empty data", () =>
        {
            var service = new ComplaintService(); Submit(service); Submit(service);
            File.Move("complaints.json", "primary-saved.json");
            Throws<IOException>(() => new StorageService().LoadComplaints());
            Require(!File.Exists("complaints.json") && new StorageService().RestoreComplaintsBackup(), "Missing primary was silently replaced.");
        });
        Run("REL-17 Users backup restores custom accounts without reseeding demonstration accounts", () =>
        {
            var storage = new StorageService();
            storage.SaveUsers(new List<User> { new() { Username = "custom-agent", PasswordHash = StorageService.HashPassword("test-password"), Role = UserRole.SupportAgent } });
            storage.SaveUsers(new List<User> { new() { Username = "another-agent", PasswordHash = StorageService.HashPassword("test-password"), Role = UserRole.SupportAgent } });
            File.Move("users.json", "primary-users-saved.json");
            Throws<IOException>(() => storage.LoadUsers());
            Require(!File.Exists("users.json") && storage.RestoreUsersBackup(), "Missing users primary was reseeded.");
            Require(storage.LoadUsers().Single().Username == "custom-agent", "Custom account backup was not preserved.");
        });
        Run("REL-18 Duplicate references/usernames and undefined persisted enum values are rejected", () =>
        {
            var storage = new StorageService();
            Throws<JsonException>(() => storage.SaveComplaints(new List<Complaint> { Ticket(1), Ticket(1) }));
            var invalid = Ticket(1); invalid.Status = (ComplaintStatus)999;
            Throws<JsonException>(() => storage.SaveComplaints(new List<Complaint> { invalid }));
            var first = new User { Username = "agent", PasswordHash = "hash", Role = UserRole.SupportAgent };
            var second = new User { Username = "AGENT", PasswordHash = "hash", Role = UserRole.SupportAgent };
            Throws<JsonException>(() => storage.SaveUsers(new List<User> { first, second }));
            File.WriteAllText("complaints.json", JsonSerializer.Serialize(new[] { Ticket(1), Ticket(1) }));
            Throws<JsonException>(() => storage.LoadComplaints());
        });
        Run("REL-19 Storage captures its directory instead of following later working-directory changes", () =>
        {
            string original = Environment.CurrentDirectory;
            var storage = new StorageService(); var service = new ComplaintService(storage);
            Directory.CreateDirectory("other");
            try { Environment.CurrentDirectory = Path.Combine(original, "other"); Submit(service); }
            finally { Environment.CurrentDirectory = original; }
            Require(File.Exists("complaints.json") && !File.Exists(Path.Combine("other", "complaints.json")), "Storage followed an unrelated current-directory change.");
        });
        Run("REL-20 Corrupt or exhausted reference counter prevents submission without changing tickets", () =>
        {
            var service = new ComplaintService(); Submit(service); string before = File.ReadAllText("complaints.json");
            File.WriteAllText("reference-sequence.json", "{broken");
            Throws<JsonException>(() => Submit(service));
            File.WriteAllText("reference-sequence.json", JsonSerializer.Serialize(new Dictionary<int, long> { [DateTime.Now.Year] = long.MaxValue }));
            Throws<InvalidOperationException>(() => Submit(service));
            Require(File.ReadAllText("complaints.json") == before && service.GetAllComplaints().Count == 1, "Counter failure changed complaints.");
        });

        int failed = Results.Count(r => !r.Passed);
        File.WriteAllText(Path.Combine(output, "results.json"), JsonSerializer.Serialize(new { RecordedAtUtc = DateTimeOffset.UtcNow, Passed = Results.Count - failed, Failed = failed, Results }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Backend reliability: {Results.Count - failed} passed, {failed} failed, {Results.Count} total.");
        return failed == 0 ? 0 : 1;
    }

    private static void Run(string name, Action check)
    {
        string original = Environment.CurrentDirectory;
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "ComplaintManagementSystem.BackendChecks"));
        string folder = Path.Combine(root, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder); Environment.CurrentDirectory = folder;
        try { check(); Results.Add(new(name, true, "Passed")); Console.WriteLine("PASS: " + name); }
        catch (Exception ex) { Results.Add(new(name, false, ex.ToString())); Console.WriteLine("FAIL: " + name + " - " + ex.Message); }
        finally
        {
            Environment.CurrentDirectory = original;
            if (!Path.GetFullPath(folder).StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Unsafe fixture cleanup path.");
            Directory.Delete(folder, recursive: true);
        }
    }
    private static Complaint Submit(ComplaintService service) => service.SubmitComplaint("customer1", "Billing query", ComplaintCategory.Billing, "Details");
    private static Complaint Ticket(long number) => new() { ReferenceNumber = $"CMP-{DateTime.Now.Year}-{number:D4}" };
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }
    private static void ThrowsIo(Action action)
    {
        try { action(); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return; }
        throw new InvalidOperationException("Expected a file-access error.");
    }
    private sealed class FailingStorage : StorageService
    {
        public bool FailWrites { get; set; }
        public override void SaveComplaints(List<Complaint> complaints)
        {
            if (FailWrites) throw new IOException("Deliberate test write failure.");
            base.SaveComplaints(complaints);
        }
    }
    private sealed record Result(string Name, bool Passed, string Message);
}
