using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem.Forms
{
    public class LoginForm : Form
    {
        private readonly ComplaintService _service;
        private readonly TextBox _txtUsername = new() { PlaceholderText = "Username", Width = 240, Top = 35, Left = 40 };
        private readonly TextBox _txtPassword = new() { PlaceholderText = "Password", UseSystemPasswordChar = true, Width = 240, Top = 75, Left = 40 };
        private readonly Button _btnLogin = new() { Text = "Login", Width = 240, Height = 34, Top = 120, Left = 40 };
        private readonly Label _lblStatus = new() { ForeColor = Color.Crimson, Width = 260, Top = 162, Left = 40 };

        public User? LoggedInUser { get; private set; }

        public LoginForm(ComplaintService service)
        {
            _service = service;
            Text = "Complaint System - Login";
            ClientSize = new Size(320, 215);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            _btnLogin.Click += BtnLogin_Click;
            AcceptButton = _btnLogin;

            Controls.AddRange(new Control[] { _txtUsername, _txtPassword, _btnLogin, _lblStatus });
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var user = _service.Authenticate(_txtUsername.Text, _txtPassword.Text);
            if (user == null)
            {
                _lblStatus.Text = "Invalid username or password.";
                return;
            }

            LoggedInUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}