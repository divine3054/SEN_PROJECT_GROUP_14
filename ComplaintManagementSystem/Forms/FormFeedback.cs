namespace ComplaintManagementSystem.Forms;

/// <summary>Displays form messages; UI checks can record them without modal dialogs.</summary>
public interface IFormFeedback
{
    void Show(IWin32Window owner, string message, string title, MessageBoxIcon icon);
}

public sealed class MessageBoxFeedback : IFormFeedback
{
    public static IFormFeedback Default { get; } = new MessageBoxFeedback();

    public void Show(IWin32Window owner, string message, string title, MessageBoxIcon icon)
    {
        MessageBox.Show(owner, message, title, MessageBoxButtons.OK, icon);
    }
}
