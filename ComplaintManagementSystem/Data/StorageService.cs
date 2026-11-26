using System.Text.Json;
using ComplaintManagementSystem.Models;

namespace ComplaintManagementSystem.Data
{
    public class StorageService
    {
        private readonly string _complaintsFile = "complaints.json";
        private readonly string _usersFile = "users.json";

        public List<Complaint> LoadComplaints()
        {
            if (!File.Exists(_complaintsFile)) return new List<Complaint>();
            string json = File.ReadAllText(_complaintsFile);
            return JsonSerializer.Deserialize<List<Complaint>>(json) ?? new List<Complaint>();
        }

        public void SaveComplaints(List<Complaint> complaints)
        {
            string json = JsonSerializer.Serialize(complaints, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_complaintsFile, json);
        }

        public List<User> LoadUsers()
        {
            if (!File.Exists(_usersFile))
            {
                var defaultUsers = new List<User>
                {
                    new User { Username = "customer1", PasswordHash = HashPassword("pass123"), Role = UserRole.Customer },
                    new User { Username = "agent1", PasswordHash = HashPassword("agent123"), Role = UserRole.SupportAgent },
                    new User { Username = "admin1", PasswordHash = HashPassword("admin123"), Role = UserRole.Admin }
                };
                SaveUsers(defaultUsers);
                return defaultUsers;
            }
            string json = File.ReadAllText(_usersFile);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_usersFile, json);
        }

        public static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
