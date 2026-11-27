using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Data;

/// <summary>Local JSON storage with atomic replacement, backups and explicit recovery.</summary>
public class StorageService
{
    private static readonly ConcurrentDictionary<string, object> Gates = new(StringComparer.OrdinalIgnoreCase);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _directory;
    private readonly object _gate;
    private string ComplaintsFile => Path.Combine(_directory, "complaints.json");
    private string UsersFile => Path.Combine(_directory, "users.json");
    private string SequenceFile => Path.Combine(_directory, "reference-sequence.json");

    public StorageService(string? dataDirectory = null)
    {
        _directory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(dataDirectory ?? Environment.CurrentDirectory));
        Directory.CreateDirectory(_directory);
        _gate = Gates.GetOrAdd(_directory, _ => new object());
    }

    public virtual List<Complaint> LoadComplaints()
    {
        lock (_gate)
        {
            var complaints = ReadList<Complaint>(ComplaintsFile);
            ValidateComplaints(complaints);
            return complaints;
        }
    }

    public virtual void SaveComplaints(List<Complaint> complaints)
    {
        ArgumentNullException.ThrowIfNull(complaints);
        lock (_gate)
        {
            ValidateComplaints(complaints);
            WriteAtomically(ComplaintsFile, complaints);
        }
    }

    public virtual List<User> LoadUsers()
    {
        lock (_gate)
        {
            if (!File.Exists(UsersFile) && !Directory.Exists(UsersFile))
            {
                // A missing primary with a backup signals data loss, not a fresh installation.
                EnsureNoOrphanedBackup(UsersFile);
                var users = new List<User>
                {
                    new() { Username = "customer1", PasswordHash = HashPassword("pass123"), Role = UserRole.Customer },
                    new() { Username = "agent1", PasswordHash = HashPassword("agent123"), Role = UserRole.SupportAgent },
                    new() { Username = "admin1", PasswordHash = HashPassword("admin123"), Role = UserRole.Admin }
                };
                SaveUsers(users);
                return users;
            }
            var saved = ReadList<User>(UsersFile);
            ValidateUsers(saved);
            return saved;
        }
    }

    public virtual void SaveUsers(List<User> users)
    {
        ArgumentNullException.ThrowIfNull(users);
        lock (_gate)
        {
            ValidateUsers(users);
            WriteAtomically(UsersFile, users);
        }
    }

    /// <summary>Restore only after an operator chooses to recover from a validated backup.</summary>
    public bool RestoreComplaintsBackup()
    {
        lock (_gate) return RestoreBackup<Complaint>(ComplaintsFile, ValidateComplaints);
    }

    public bool RestoreUsersBackup()
    {
        lock (_gate) return RestoreBackup<User>(UsersFile, ValidateUsers);
    }

    public static string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
    }

    // A directory-scoped lock prevents read/modify/write races between service
    // instances inside this process. Concurrent application processes are not supported.
    internal T WithComplaintLock<T>(Func<T> operation)
    {
        lock (_gate) return operation();
    }

    internal string ReserveReference(IReadOnlyCollection<Complaint> complaints, int year)
    {
        lock (_gate)
        {
            Dictionary<int, long> sequences;
            if (!File.Exists(SequenceFile))
            {
                if (Directory.Exists(SequenceFile)) throw new IOException("The reference counter path is a directory.");
                EnsureNoOrphanedBackup(SequenceFile);
                sequences = new Dictionary<int, long>();
            }
            else
            {
                sequences = ReadJson<Dictionary<int, long>>(SequenceFile);
                if (sequences.Any(pair => pair.Key is < 1 or > 9999 || pair.Value < 0))
                    throw InvalidData(SequenceFile, "The reference counter contains an invalid year or sequence.");
            }

            string prefix = $"CMP-{year}-";
            long highest = sequences.GetValueOrDefault(year);
            foreach (var complaint in complaints)
            {
                string reference = complaint.ReferenceNumber;
                if (reference.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    && long.TryParse(reference.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out long value))
                    highest = Math.Max(highest, value);
            }

            if (highest == long.MaxValue) throw new InvalidOperationException("The reference counter has reached its maximum value.");
            long next = highest + 1;
            sequences[year] = next;
            // Reserve before writing the ticket. A failed ticket save may leave a
            // gap, but the reserved number is never issued again while the ledger exists.
            WriteAtomically(SequenceFile, sequences);
            return prefix + next.ToString("D4", CultureInfo.InvariantCulture);
        }
    }

    private static List<T> ReadList<T>(string path)
    {
        if (Directory.Exists(path)) throw new IOException($"The storage path is a directory: {path}");
        if (!File.Exists(path))
        {
            EnsureNoOrphanedBackup(path);
            return new List<T>();
        }
        return ReadJson<List<T>>(path);
    }

    private static T ReadJson<T>(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(path))
                ?? throw InvalidData(path, "The file contains null instead of records.");
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Cannot read {Path.GetFileName(path)}. The JSON records are invalid. The file was left unchanged. Restore a validated backup if one is available.", ex);
        }
    }

    private static JsonException InvalidData(string path, string reason) =>
        new($"Invalid data in {Path.GetFileName(path)}. {reason} The file was left unchanged.");

    private static void EnsureNoOrphanedBackup(string path)
    {
        if (File.Exists(path + ".bak"))
            throw new IOException($"Missing {Path.GetFileName(path)}, but a backup exists. Restore the backup before continuing; no empty replacement was created.");
    }

    private static void ValidateComplaints(List<Complaint> complaints)
    {
        if (complaints.Any(c => c is null || string.IsNullOrWhiteSpace(c.ReferenceNumber)
            || c.SubmittedBy is null || c.Title is null || c.Description is null || c.AgentNotes is null
            || !Enum.IsDefined(c.Category) || !Enum.IsDefined(c.Status)))
            throw new JsonException("Complaint records contain null fields, missing references or invalid category/status values.");
        if (complaints.Select(c => c.ReferenceNumber.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != complaints.Count)
            throw new JsonException("Complaint records contain duplicate reference numbers. Review the data before continuing.");
    }

    private static void ValidateUsers(List<User> users)
    {
        if (users.Any(u => u is null || string.IsNullOrWhiteSpace(u.Id) || string.IsNullOrWhiteSpace(u.Username)
            || string.IsNullOrWhiteSpace(u.PasswordHash) || !Enum.IsDefined(u.Role)))
            throw new JsonException("User records contain missing fields or an unsupported role.");
        if (users.Select(u => u.Username.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != users.Count)
            throw new JsonException("User records contain duplicate usernames.");
    }

    private static bool RestoreBackup<T>(string path, Action<List<T>> validate)
    {
        string backup = path + ".bak";
        if (!File.Exists(backup)) return false;
        var records = ReadJson<List<T>>(backup);
        validate(records);
        // Preserve the damaged primary for diagnosis before replacing it.
        if (File.Exists(path))
            File.Copy(path, path + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N"));
        WriteAtomically(path, records, keepBackup: false);
        return true;
    }

    private static void WriteAtomically<T>(string path, T records, bool keepBackup = true)
    {
        string json = JsonSerializer.Serialize(records, JsonOptions);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }
            if (File.Exists(path))
                File.Replace(temporary, path, keepBackup ? path + ".bak" : null);
            else
                File.Move(temporary, path);
        }
        finally
        {
            // Cleanup must not mask the original I/O error or report a committed
            // write as failed just because a temporary-file cleanup was denied.
            try { if (File.Exists(temporary)) File.Delete(temporary); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
