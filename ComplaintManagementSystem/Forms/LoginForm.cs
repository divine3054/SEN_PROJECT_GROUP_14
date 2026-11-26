namespace ComplaintManagementSystem.Forms;

public partial class LoginForm : Form
{
    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(480, 320);
        Name = nameof(LoginForm);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Complaint Management System - Login";
        ResumeLayout(false);
    }
}
