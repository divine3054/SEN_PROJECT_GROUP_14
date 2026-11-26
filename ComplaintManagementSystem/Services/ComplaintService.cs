using ComplaintManagementSystem.Data;
using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Services
{
    public class ComplaintService
    {
        private readonly StorageService _storage = new();
        private List<Complaint> _complaints;
        private List<User> _users;

        public ComplaintService()
        {
            _complaints = _storage.LoadComplaints();
            _users = _storage.LoadUsers();
        }

        public User? Authenticate(string username, string password)
        {
            string hash = StorageService.HashPassword(password);
            return _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.PasswordHash == hash);
        }

        public Complaint SubmitComplaint(string username, string title, ComplaintCategory category, string description)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Title and description are required.");

            string refNumber = $"CMP-{DateTime.Now.Year}-{(_complaints.Count + 1):D4}";

            var complaint = new Complaint
            {
                ReferenceNumber = refNumber,
                SubmittedBy = username,
                Title = title.Trim(),
                Category = category,
                Description = description.Trim(),
                Status = ComplaintStatus.Pending
            };

            _complaints.Add(complaint);
            _storage.SaveComplaints(_complaints);
            return complaint;
        }

        public Complaint? TrackByReference(string referenceNumber)
        {
            return _complaints.FirstOrDefault(c =>
                c.ReferenceNumber.Equals(referenceNumber.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public List<Complaint> GetAllComplaints(ComplaintCategory? categoryFilter = null, ComplaintStatus? statusFilter = null)
        {
            return _complaints
                .Where(c => (!categoryFilter.HasValue || c.Category == categoryFilter.Value) &&
                            (!statusFilter.HasValue || c.Status == statusFilter.Value))
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        public bool UpdateComplaintStatus(string referenceNumber, ComplaintStatus newStatus, ComplaintCategory newCategory, string agentNotes)
        {
            var complaint = TrackByReference(referenceNumber);
            if (complaint == null) return false;

            complaint.Status = newStatus;
            complaint.Category = newCategory;
            complaint.AgentNotes = string.IsNullOrWhiteSpace(agentNotes) ? complaint.AgentNotes : agentNotes.Trim();
            complaint.UpdatedAt = DateTime.Now;

            _storage.SaveComplaints(_complaints);
            return true;
        }
    }
}
