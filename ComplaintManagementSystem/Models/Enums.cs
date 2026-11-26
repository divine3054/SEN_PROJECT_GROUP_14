namespace ComplaintManagementSystem.Models
{
    public enum UserRole
    {
        Customer,
        SupportAgent,
        Admin
    }

    public enum ComplaintCategory
    {
        Billing,
        Technical,
        Service,
        GeneralFeedback
    }

    public enum ComplaintStatus
    {
        Pending,
        InProgress,
        Resolved,
        Closed
    }
}
