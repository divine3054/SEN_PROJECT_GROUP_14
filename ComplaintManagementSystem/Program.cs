using ComplaintManagementSystem.Forms;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            var service = new ComplaintService();

            using var loginForm = new LoginForm(service);
            if (loginForm.ShowDialog() == DialogResult.OK && loginForm.LoggedInUser != null)
            {
                User user = loginForm.LoggedInUser;
                if (user.Role == UserRole.Customer)
                {
                    Application.Run(new CustomerForm(service, user));
                }
                else
                {
                    Application.Run(new AgentForm(service, user));
                }
            }
        }
    }
}