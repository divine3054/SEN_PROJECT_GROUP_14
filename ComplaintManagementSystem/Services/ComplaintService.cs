using ComplaintManagementSystem.Data;
using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Services;

public class ComplaintService
{
    private readonly StorageService _storage;
    private List<Complaint> _complaints;
    private readonly List<User> _users;

    public ComplaintService(StorageService? storage = null)
    {
        _storage = storage ?? new StorageService();
        _complaints = _storage.LoadComplaints();
        _users = _storage.LoadUsers();
    }

    public User? Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password)) return null;
        string hash = StorageService.HashPassword(password);
        var user = _users.FirstOrDefault(u =>
            u.Username.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase) && u.PasswordHash == hash);
        return user is null ? null : new User { Id = user.Id, Username = user.Username, PasswordHash = user.PasswordHash, Role = user.Role };
    }

    public Complaint SubmitComplaint(string username, string title, ComplaintCategory category, string description)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("A submitting username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Title and description are required.");
        if (!Enum.IsDefined(category)) throw new ArgumentException("Choose a defined complaint category.", nameof(category));

        return _storage.WithComplaintLock(() =>
        {
            // Reload before writes so another service instance in the same
            // process cannot silently overwrite tickets committed since startup.
            var candidate = _storage.LoadComplaints();
            DateTime now = DateTime.Now;
            var complaint = new Complaint
            {
                ReferenceNumber = _storage.ReserveReference(candidate, now.Year),
                SubmittedBy = username.Trim(), Title = title.Trim(), Category = category,
                Description = description.Trim(), Status = ComplaintStatus.Pending,
                CreatedAt = now, UpdatedAt = now
            };
            candidate.Add(complaint);
            _storage.SaveComplaints(candidate);
            // Publish to the cache only after the persistent write succeeds.
            _complaints = candidate;
            return Copy(complaint);
        });
    }

    public Complaint? TrackByReference(string referenceNumber)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber)) return null;
        var complaint = _complaints.FirstOrDefault(c =>
            c.ReferenceNumber.Equals(referenceNumber.Trim(), StringComparison.OrdinalIgnoreCase));
        return complaint is null ? null : Copy(complaint);
    }

    public List<Complaint> GetAllComplaints(ComplaintCategory? categoryFilter = null, ComplaintStatus? statusFilter = null)
    {
        if (categoryFilter.HasValue && !Enum.IsDefined(categoryFilter.Value))
            throw new ArgumentException("Choose a defined category.", nameof(categoryFilter));
        if (statusFilter.HasValue && !Enum.IsDefined(statusFilter.Value))
            throw new ArgumentException("Choose a defined status.", nameof(statusFilter));
        return _complaints
            .Where(c => (!categoryFilter.HasValue || c.Category == categoryFilter.Value)
                && (!statusFilter.HasValue || c.Status == statusFilter.Value))
            .OrderByDescending(c => c.CreatedAt)
            .Select(Copy).ToList();
    }

    public bool UpdateComplaintStatus(string referenceNumber, ComplaintStatus newStatus, ComplaintCategory newCategory, string agentNotes)
    {
        if (!Enum.IsDefined(newStatus)) throw new ArgumentException("Choose a defined status.", nameof(newStatus));
        if (!Enum.IsDefined(newCategory)) throw new ArgumentException("Choose a defined category.", nameof(newCategory));
        if (string.IsNullOrWhiteSpace(referenceNumber)) return false;

        return _storage.WithComplaintLock(() =>
        {
            var candidate = _storage.LoadComplaints();
            var complaint = candidate.FirstOrDefault(c =>
                c.ReferenceNumber.Equals(referenceNumber.Trim(), StringComparison.OrdinalIgnoreCase));
            if (complaint is null) return false;
            complaint.Status = newStatus;
            complaint.Category = newCategory;
            if (!string.IsNullOrWhiteSpace(agentNotes)) complaint.AgentNotes = agentNotes.Trim();
            complaint.UpdatedAt = DateTime.Now;
            _storage.SaveComplaints(candidate);
            _complaints = candidate;
            return true;
        });
    }

    // Never expose mutable cached records to forms or other callers.
    private static Complaint Copy(Complaint c) => new()
    {
        ReferenceNumber = c.ReferenceNumber, SubmittedBy = c.SubmittedBy, Title = c.Title,
        Category = c.Category, Description = c.Description, Status = c.Status,
        AgentNotes = c.AgentNotes, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt
    };
}
