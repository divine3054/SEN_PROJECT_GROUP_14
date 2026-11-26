namespace ComplaintManagementSystem.Forms;

public partial class CustomerForm : Form
{
    public CustomerForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 600);
        Name = nameof(CustomerForm);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Complaint Management System - Customer";
        ResumeLayout(false);
    }
}
