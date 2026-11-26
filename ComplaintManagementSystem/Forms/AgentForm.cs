namespace ComplaintManagementSystem.Forms;

public partial class AgentForm : Form
{
    public AgentForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 600);
        Name = nameof(AgentForm);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Complaint Management System - Agent";
        ResumeLayout(false);
    }
}
