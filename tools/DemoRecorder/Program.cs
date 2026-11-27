using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.Json;
using ComplaintManagementSystem.Forms;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem.DemoRecorder;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: DemoRecorder <ffmpeg.exe> <output-directory> <isolated-data-directory>");
            return 2;
        }
        ApplicationConfiguration.Initialize();
        string output = Path.GetFullPath(args[1]); string data = Path.GetFullPath(args[2]);
        if (Directory.Exists(data) && Directory.EnumerateFileSystemEntries(data).Any())
        {
            Console.Error.WriteLine("Demo data directory must be empty; live data will not be used."); return 2;
        }
        Directory.CreateDirectory(output); Directory.CreateDirectory(data);
        Environment.CurrentDirectory = data;
        using var host = new DemoWindow(Path.GetFullPath(args[0]), output);
        Application.Run(host);
        return host.Success ? 0 : 1;
    }
}

internal sealed class DemoWindow : Form, IFormFeedback
{
    private readonly string _ffmpeg;
    private readonly string _output;
    private readonly Panel _workspace = new() { Dock = DockStyle.Fill };
    private readonly Label _caption = new() { Dock = DockStyle.Top, Height = 105, Padding = new Padding(16), TextAlign = ContentAlignment.MiddleLeft };
    private readonly Label _footer = new() { Dock = DockStyle.Bottom, Height = 32, Text = "Automated walkthrough | Real WinForms screens | Synthetic data | Not stakeholder acceptance", TextAlign = ContentAlignment.MiddleCenter };
    private readonly List<object> _events = new();
    private Form? _screen;
    private Process? _capture;
    private StreamWriter? _captureLog;
    public bool Success { get; private set; }

    public DemoWindow(string ffmpeg, string output)
    {
        _ffmpeg = ffmpeg; _output = output;
        SuspendLayout();
        Font = new Font("Segoe UI", 10F);
        AutoScaleDimensions = new SizeF(96, 96); AutoScaleMode = AutoScaleMode.Dpi;
        Text = "Group14 Application Demonstration"; ClientSize = new Size(1280, 900);
        StartPosition = FormStartPosition.CenterScreen; TopMost = true; MaximizeBox = false;
        Controls.Add(_workspace); Controls.Add(_caption); Controls.Add(_footer);
        ResumeLayout(true);
        Shown += async (_, _) => await Demonstrate();
    }

    private async Task Demonstrate()
    {
        try
        {
            _caption.Text = "Group 14 - Complaint and Feedback Management System\nAutomated demonstration of the actual application screens using isolated sample data.";
            await Task.Delay(1200);
            StartCapture();
            await Task.Delay(1400);

            var service = new ComplaintService();
            var login = ShowScreen(new LoginForm(service));
            await Step("1. Login validation", "An incorrect password is rejected; password characters are masked.");
            Get<TextBox>(login, "txtUsername").Text = "customer1";
            Get<TextBox>(login, "txtPassword").Text = "incorrect";
            Get<Button>(login, "btnLogin").PerformClick();
            await PauseAndCapture("01-login-validation");
            Get<TextBox>(login, "txtPassword").Text = "pass123";
            Get<Button>(login, "btnLogin").PerformClick();
            var customerUser = login.LoggedInUser ?? throw new InvalidOperationException("Customer login failed.");
            var customer = ShowScreen(new CustomerForm(service, customerUser, this));
            await Step("2. Required-field validation", "A complaint requires a title, category and description.");
            Get<Button>(customer, "btnSubmit").PerformClick();
            await Task.Delay(900);
            await Step("3. Submit a Billing complaint", "The real service saves the ticket and returns its unique reference.");
            Get<TextBox>(customer, "txtTitle").Text = "Incorrect charge on my bill";
            Get<ComboBox>(customer, "cmbCategory").SelectedItem = ComplaintCategory.Billing;
            Get<TextBox>(customer, "txtDescription").Text = "Please review the additional charge on my latest bill.\r\nThis is synthetic demonstration data.";
            await Task.Delay(2000); SaveFrame("02-submission-input");
            Get<Button>(customer, "btnSubmit").PerformClick();
            string reference = service.GetAllComplaints().Single().ReferenceNumber;
            await PauseAndCapture("03-submitted-reference");
            await Step("4. Track the saved reference", "The Customer sees Pending status and initial review notes.");
            Get<TabControl>(customer, "tabCustomer").SelectedIndex = 1;
            Get<TextBox>(customer, "txtSearchRef").Text = reference.ToLowerInvariant();
            Get<Button>(customer, "btnSearch").PerformClick();
            await PauseAndCapture("04-tracking-pending");
            await Step("5. Unknown and blank reference checks", "A miss clears prior details; blank input gets an explicit alert.");
            Get<TextBox>(customer, "txtSearchRef").Text = "unknown-reference";
            Get<Button>(customer, "btnSearch").PerformClick(); await Task.Delay(1800);
            Get<TextBox>(customer, "txtSearchRef").Clear(); Get<Button>(customer, "btnSearch").PerformClick();
            await Task.Delay(1000);

            service = new ComplaintService();
            var agentLogin = ShowScreen(new LoginForm(service));
            await Step("6. Support Agent login", "A fresh service reloads persisted records for the agent workflow.");
            Get<TextBox>(agentLogin, "txtUsername").Text = "agent1";
            Get<TextBox>(agentLogin, "txtPassword").Text = "agent123"; await Task.Delay(1400);
            Get<Button>(agentLogin, "btnLogin").PerformClick();
            var agent = ShowScreen(new AgentForm(service, agentLogin.LoggedInUser ?? throw new InvalidOperationException("Agent login failed."), this));
            await PauseAndCapture("05-agent-workspace");
            await Step("7. Filter and refresh the ticket grid", "A Technical filter has no match; Refresh restores all complaints.");
            Get<ComboBox>(agent, "cmbFilterCategory").SelectedItem = ComplaintCategory.Technical;
            Get<Button>(agent, "btnFilter").PerformClick(); await Task.Delay(2000);
            Get<Button>(agent, "btnRefresh").PerformClick(); await Task.Delay(1400);
            var grid = Get<DataGridView>(agent, "dgvComplaints");
            grid.CurrentCell = grid.Rows[0].Cells[0]; grid.Rows[0].Selected = true;
            await Step("8. Resolve the selected ticket", "One selected row enables status/category editing and multiline notes.");
            Get<ComboBox>(agent, "cmbUpdateStatus").SelectedItem = ComplaintStatus.Resolved;
            Get<ComboBox>(agent, "cmbUpdateCategory").SelectedItem = ComplaintCategory.Service;
            Get<TextBox>(agent, "txtAgentNotes").Text = "Reviewed the sample charge and corrected it.\r\nCustomer informed; issue resolved.";
            await Task.Delay(2200); SaveFrame("06-agent-update");
            Get<Button>(agent, "btnSaveUpdate").PerformClick(); await Task.Delay(1500);

            service = new ComplaintService();
            var returnLogin = ShowScreen(new LoginForm(service));
            await Step("9. Reopen and reload persisted data", "A new service reads the saved update before the Customer signs in again.");
            Get<TextBox>(returnLogin, "txtUsername").Text = "customer1";
            Get<TextBox>(returnLogin, "txtPassword").Text = "pass123"; await Task.Delay(1300);
            Get<Button>(returnLogin, "btnLogin").PerformClick();
            var reopened = ShowScreen(new CustomerForm(service, returnLogin.LoggedInUser ?? throw new InvalidOperationException("Return login failed."), this));
            Get<TabControl>(reopened, "tabCustomer").SelectedIndex = 1;
            Get<TextBox>(reopened, "txtSearchRef").Text = reference;
            Get<Button>(reopened, "btnSearch").PerformClick();
            await Step("10. Persisted resolution confirmed", "The Customer now sees Resolved, Service, the update time and the agent's saved notes.");
            if (Get<Label>(reopened, "lblTrackedStatus").Text != "Resolved") throw new InvalidOperationException("Persisted status was not displayed.");
            await PauseAndCapture("07-persisted-resolution", 4000);
            await Step("Demonstration complete", "Core workflow, validation, filtering and persisted resolution demonstrated. Human acceptance is recorded separately.");
            await Task.Delay(2600);
            Success = true;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); _events.Add(new { Error = ex.Message }); }
        finally
        {
            await StopCapture();
            File.WriteAllText(Path.Combine(_output, "walkthrough-events.json"), JsonSerializer.Serialize(new { RecordedAtUtc = DateTimeOffset.UtcNow, Success, Events = _events }, new JsonSerializerOptions { WriteIndented = true }));
            Close();
        }
    }

    private T ShowScreen<T>(T screen) where T : Form
    {
        _screen?.Dispose(); _workspace.Controls.Clear(); _screen = screen;
        screen.TopLevel = false; screen.FormBorderStyle = FormBorderStyle.None; screen.Dock = DockStyle.Fill;
        _workspace.Controls.Add(screen); screen.Show(); Application.DoEvents();
        return screen;
    }
    private async Task Step(string title, string detail)
    {
        _caption.Text = title + "\n" + detail;
        _events.Add(new { TimeUtc = DateTimeOffset.UtcNow, Title = title, Detail = detail });
        await Task.Delay(1800);
    }
    private async Task PauseAndCapture(string name, int milliseconds = 2400) { await Task.Delay(milliseconds); SaveFrame(name); }
    private void SaveFrame(string name)
    {
        string folder = Path.Combine(_output, "screenshots"); Directory.CreateDirectory(folder);
        using var bitmap = new Bitmap(Width, Height);
        DrawToBitmap(bitmap, new Rectangle(Point.Empty, Size));
        bitmap.Save(Path.Combine(folder, name + ".png"), ImageFormat.Png);
    }
    private static T Get<T>(Control root, string name) where T : Control => (T)root.Controls.Find(name, true).Single();

    public void Show(IWin32Window owner, string message, string title, MessageBoxIcon icon)
    {
        using var notice = new Form { Text = title, Font = Font, StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(570, 230), FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, ShowInTaskbar = false };
        notice.Controls.Add(new Label { Dock = DockStyle.Fill, Padding = new Padding(18), Text = message + "\n\n[Auto-closing demonstration message]" });
        using var timer = new System.Windows.Forms.Timer { Interval = 2100 };
        timer.Tick += (_, _) => notice.Close(); timer.Start();
        notice.ShowDialog(this);
    }
    private void StartCapture()
    {
        _captureLog = new StreamWriter(Path.Combine(_output, "capture.log"));
        var info = new ProcessStartInfo(_ffmpeg) { UseShellExecute = false, RedirectStandardInput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (string argument in new[] { "-y", "-f", "gdigrab", "-draw_mouse", "0", "-framerate", "10", "-i", "title=Group14 Application Demonstration", "-vf", "scale=1280:-2", "-c:v", "libx264", "-preset", "ultrafast", "-crf", "23", "-pix_fmt", "yuv420p", Path.Combine(_output, "Group14-Demonstration.mp4") }) info.ArgumentList.Add(argument);
        _capture = Process.Start(info) ?? throw new InvalidOperationException("Could not start screen recording.");
        _capture.ErrorDataReceived += (_, e) => { if (e.Data is not null) { lock (_events) _captureLog?.WriteLine(e.Data); } };
        _capture.BeginErrorReadLine();
    }
    private async Task StopCapture()
    {
        if (_capture is not null)
        {
            if (!_capture.HasExited)
            {
                await _capture.StandardInput.WriteLineAsync("q");
                try { await _capture.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15)); }
                catch (TimeoutException) { _capture.Kill(); Success = false; }
            }
            if (_capture.HasExited && _capture.ExitCode != 0) Success = false;
            _capture.Dispose();
        }
        _captureLog?.Dispose();
    }
}
