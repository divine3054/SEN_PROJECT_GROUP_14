using System.Text.Json;
using ComplaintManagementSystem.Data;

namespace ComplaintManagementSystem.StorageRecovery;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length is < 2 or > 3 || args[1] is not ("complaints" or "users")
            || (args.Length == 3 && args[2] != "--restore"))
        {
            Console.WriteLine("Usage: StorageRecovery <data-directory> complaints|users [--restore]");
            Console.WriteLine("Without --restore, only show the backup location. Close the application before restoring.");
            return args.Length == 0 ? 0 : 2;
        }
        try
        {
            string directory = Path.GetFullPath(args[0]);
            if (!Directory.Exists(directory)) throw new DirectoryNotFoundException("The supplied data directory does not exist.");
            string backup = Path.Combine(directory, args[1] + ".json.bak");
            Console.WriteLine("Data directory: " + directory);
            Console.WriteLine("Previous-version backup: " + backup);
            if (!File.Exists(backup))
            {
                Console.Error.WriteLine("No backup exists. No data was changed.");
                return 1;
            }
            if (args.Length == 2)
            {
                Console.WriteLine("Preview only: no data was changed. Inspect the backup before restoring.");
                Console.WriteLine("The previous-version backup may omit the most recent changes.");
                Console.WriteLine("Close the application, then add --restore if you choose to recover this file.");
                return 0;
            }
            var storage = new StorageService(directory);
            bool restored = args[1] == "complaints" ? storage.RestoreComplaintsBackup() : storage.RestoreUsersBackup();
            if (!restored) { Console.Error.WriteLine("Backup no longer exists. No data was changed."); return 1; }
            Console.WriteLine("Validated backup restored. Any existing primary was preserved as a .corrupt-* file.");
            Console.WriteLine("The reference counter was left unchanged. Reopen the application to reload records.");
            return 0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            Console.Error.WriteLine("Recovery failed: " + ex.Message);
            return 1;
        }
    }
}
