namespace ComplaintManagementSystem.Models
{
    public class Complaint
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ComplaintCategory Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;
        public string AgentNotes { get; set; } = "Awaiting review.";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
