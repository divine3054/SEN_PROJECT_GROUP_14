using ComplaintManagementSystem.Forms;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;
using System.Text.Json;

namespace ComplaintManagementSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            ComplaintService service;
            try
            {
                service = new ComplaintService();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                MessageBox.Show("The application could not open its saved records. Check that the data files are accessible and contain valid JSON, then reopen the application.\n\n" + ex.Message,
                    "Unable to open complaint records", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var loginForm = new LoginForm(service);
            if (loginForm.ShowDialog() == DialogResult.OK && loginForm.LoggedInUser != null)
            {
                User user = loginForm.LoggedInUser;
                if (user.Role == UserRole.Customer)
                {
                    Application.Run(new CustomerForm(service, user));
                }
                else if (user.Role is UserRole.SupportAgent or UserRole.Admin)
                {
                    Application.Run(new AgentForm(service, user));
                }
                else
                {
                    MessageBox.Show("This account has an unsupported role. Contact the project administrator.",
                        "Unable to sign in", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}