using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem.Forms;

public class LoginForm : Form
{
    private readonly ComplaintService _service;
    private readonly TextBox _txtUsername = new() { Name = "txtUsername", Dock = DockStyle.Fill, PlaceholderText = "Username", AccessibleName = "Username" };
    private readonly TextBox _txtPassword = new() { Name = "txtPassword", Dock = DockStyle.Fill, UseSystemPasswordChar = true, AccessibleName = "Password" };
    private readonly Button _btnLogin = new() { Name = "btnLogin", Text = "&Log In", AutoSize = true, Padding = new Padding(16, 5, 16, 5) };
    private readonly Label _lblStatus = new() { Name = "lblLoginStatus", AutoSize = true, MaximumSize = new Size(360, 0), ForeColor = Color.Firebrick };

    public User? LoggedInUser { get; private set; }

    public LoginForm(ComplaintService service)
    {
        SuspendLayout();
        _service = service;
        Name = "LoginForm";
        Text = "Complaint Management System - Login";
        Font = new Font("Segoe UI", 10F);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(450, 340);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(28), ColumnCount = 1, RowCount = 7 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 7; i++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = "Complaint & Feedback Management", UseMnemonic = false, AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 0, 3, 18) }, 0, 0);
        layout.Controls.Add(new Label { Text = "&Username", AutoSize = true }, 0, 1);
        layout.Controls.Add(_txtUsername, 0, 2);
        layout.Controls.Add(new Label { Text = "&Password", AutoSize = true, Margin = new Padding(3, 12, 3, 3) }, 0, 3);
        layout.Controls.Add(_txtPassword, 0, 4);
        layout.Controls.Add(_btnLogin, 0, 5);
        _btnLogin.Margin = new Padding(3, 16, 3, 10);
        layout.Controls.Add(_lblStatus, 0, 6);
        Controls.Add(layout);

        _btnLogin.Click += BtnLogin_Click;
        _txtUsername.TextChanged += (_, _) => _lblStatus.Text = string.Empty;
        _txtPassword.TextChanged += (_, _) => _lblStatus.Text = string.Empty;
        AcceptButton = _btnLogin;
        Shown += (_, _) => _txtUsername.Focus();
        ResumeLayout(true);
    }

    private void BtnLogin_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtUsername.Text) || string.IsNullOrEmpty(_txtPassword.Text))
        {
            _lblStatus.Text = "Enter your username and password.";
            if (string.IsNullOrWhiteSpace(_txtUsername.Text)) _txtUsername.Focus();
            else _txtPassword.Focus();
            return;
        }

        var user = _service.Authenticate(_txtUsername.Text.Trim(), _txtPassword.Text);
        if (user is null)
        {
            _txtPassword.Clear();
            _lblStatus.Text = "Invalid username or password.";
            _txtPassword.Focus();
            return;
        }

        LoggedInUser = user;
        DialogResult = DialogResult.OK;
        Close();
    }
}