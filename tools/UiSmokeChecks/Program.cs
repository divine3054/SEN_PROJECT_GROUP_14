using System.Drawing.Imaging;
using System.Text.Json;
using ComplaintManagementSystem.Data;
using ComplaintManagementSystem.Forms;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem.UiSmokeChecks;

internal static class Program
{
    private static string _output = string.Empty;
    private static readonly List<CheckResult> Results = new();
    private static readonly User Customer = new() { Username = "customer1", Role = UserRole.Customer };
    private static readonly User Agent = new() { Username = "agent1", Role = UserRole.SupportAgent };

    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        _output = Path.GetFullPath(args.Length == 1 ? args[0] : Path.Combine(Environment.CurrentDirectory, ".tools", "ui-checks"));
        Directory.CreateDirectory(_output);

        Run("UI-01 Blank login is rejected and password is masked", () =>
        {
            using var form = Open(new LoginForm(new ComplaintService()));
            Get<Button>(form, "btnLogin").PerformClick();
            Require(form.LoggedInUser is null && form.DialogResult != DialogResult.OK, "Blank login was accepted.");
            Require(Get<Label>(form, "lblLoginStatus").Text.Contains("Enter"), "Blank-login feedback is missing.");
            Require(Get<TextBox>(form, "txtPassword").UseSystemPasswordChar, "Password is not masked.");
            Capture(form, "01-login");
        });
        Run("UI-02 Invalid login clears password and stays open", () =>
        {
            using var form = Open(new LoginForm(new ComplaintService()));
            Get<TextBox>(form, "txtUsername").Text = "customer1";
            Get<TextBox>(form, "txtPassword").Text = "wrong";
            Get<Button>(form, "btnLogin").PerformClick();
            Require(form.LoggedInUser is null && !form.IsDisposed, "Invalid login closed the form.");
            Require(Get<TextBox>(form, "txtPassword").Text == "", "Failed password was not cleared.");
            Require(Get<Label>(form, "lblLoginStatus").Text.Contains("Invalid"), "Invalid-login feedback is missing.");
        });
        foreach (var credentials in new[] { ("customer1", "pass123", UserRole.Customer), ("agent1", "agent123", UserRole.SupportAgent), ("admin1", "admin123", UserRole.Admin) })
        {
            Run($"UI-03 Valid {credentials.Item3} login returns authenticated role", () =>
            {
                using var form = Open(new LoginForm(new ComplaintService()));
                Get<TextBox>(form, "txtUsername").Text = " " + credentials.Item1.ToUpperInvariant() + " ";
                Get<TextBox>(form, "txtPassword").Text = credentials.Item2;
                Get<Button>(form, "btnLogin").PerformClick();
                Require(form.LoggedInUser?.Role == credentials.Item3 && form.DialogResult == DialogResult.OK, "Expected role was not authenticated.");
            });
        }
        Run("UI-04 Empty complaint shows warning without creating a record", () =>
        {
            var service = new ComplaintService(); var messages = new RecordingFeedback();
            using var form = Open(new CustomerForm(service, Customer, messages));
            Get<Button>(form, "btnSubmit").PerformClick();
            Require(messages.Messages.Single().Icon == MessageBoxIcon.Warning, "Required-field warning was not shown.");
            Require(service.GetAllComplaints().Count == 0, "Invalid submission created a ticket.");
        });
        Run("UI-05 Submission saves ticket and confirms its returned reference", () =>
        {
            var service = new ComplaintService(); var messages = new RecordingFeedback();
            using var form = Open(new CustomerForm(service, Customer, messages));
            FillComplaint(form);
            Get<Button>(form, "btnSubmit").PerformClick();
            var saved = new StorageService().LoadComplaints().Single();
            Require(Get<Label>(form, "lblGeneratedRef").Text.Contains(saved.ReferenceNumber), "Reference label is missing.");
            Require(messages.Messages.Single().Title == "Complaint submitted" && messages.Messages[0].Message.Contains(saved.ReferenceNumber), "Confirmation did not contain the returned reference.");
            Require(Get<TextBox>(form, "txtTitle").Text == "" && Get<TextBox>(form, "txtDescription").Text == "", "Saved form inputs were not cleared.");
            Capture(form, "02-customer-submit");
        });
        Run("UI-06 Tracking shows read-only details and clears them for missing or blank input", () =>
        {
            var service = new ComplaintService(); var messages = new RecordingFeedback();
            var ticket = service.SubmitComplaint(Customer.Username, "Demo billing query", ComplaintCategory.Billing, "Please review the charge.");
            service.UpdateComplaintStatus(ticket.ReferenceNumber, ComplaintStatus.InProgress, ComplaintCategory.Billing, "Review started.\nWe will update this ticket after checking the bill.");
            using var form = Open(new CustomerForm(service, Customer, messages));
            Get<TabControl>(form, "tabCustomer").SelectedIndex = 1;
            Application.DoEvents();
            Get<TextBox>(form, "txtSearchRef").Text = " " + ticket.ReferenceNumber.ToLowerInvariant() + " ";
            Get<Button>(form, "btnSearch").PerformClick();
            Require(Get<Label>(form, "lblTrackedStatus").Text == "InProgress", "Tracked status is incorrect.");
            Require(Get<Label>(form, "lblTrackedCategory").Text == "Billing", "Tracked category is incorrect.");
            Require(Get<Label>(form, "lblTrackedUpdatedAt").Text.Length > 0, "Updated time is missing.");
            Require(Get<TextBox>(form, "txtTrackedAgentNotes").ReadOnly && Get<TextBox>(form, "txtTrackedAgentNotes").Multiline, "Tracking notes must be read-only and multiline.");
            Capture(form, "03-customer-track");
            Get<TextBox>(form, "txtSearchRef").Text = "unknown";
            Get<Button>(form, "btnSearch").PerformClick();
            Require(Get<Label>(form, "lblTrackedStatus").Text == "" && Get<TextBox>(form, "txtTrackedAgentNotes").Text == "", "Stale result survived a miss.");
            Get<TextBox>(form, "txtSearchRef").Text = " ";
            Get<Button>(form, "btnSearch").PerformClick();
            Require(messages.Messages.Last().Title == "Reference required", "Blank tracking input did not show a warning.");
        });
        Run("UI-07 Failed submission retains text and reports no success", () =>
        {
            var service = new ComplaintService(); var messages = new RecordingFeedback();
            Directory.CreateDirectory("complaints.json");
            using var form = Open(new CustomerForm(service, Customer, messages));
            FillComplaint(form); Get<Button>(form, "btnSubmit").PerformClick();
            Require(messages.Messages.Single().Icon == MessageBoxIcon.Error, "Save failure did not show an error.");
            Require(Get<Label>(form, "lblGeneratedRef").Text == "", "Save failure displayed a successful reference.");
            Require(Get<TextBox>(form, "txtTitle").Text.Length > 0 && !Get<Button>(form, "btnSubmit").Enabled, "Save failure did not preserve text/block unsafe retry.");
        });
        Run("UI-08 Empty agent grid has six explicit columns and editing is disabled", () =>
        {
            using var form = Open(new AgentForm(new ComplaintService(), Agent, new RecordingFeedback()));
            var grid = Get<DataGridView>(form, "dgvComplaints");
            Require(grid.Columns.Cast<DataGridViewColumn>().Select(c => c.DataPropertyName).SequenceEqual(new[] { "ReferenceNumber", "Title", "Category", "Status", "SubmittedBy", "CreatedAt" }), "Grid columns differ from the agreed six.");
            Require(grid.ReadOnly && !grid.MultiSelect && !grid.AllowUserToAddRows && !grid.AutoGenerateColumns, "Grid editing/selection settings are incorrect.");
            Require(!Get<Button>(form, "btnSaveUpdate").Enabled && grid.Rows.Count == 0, "Save must be disabled on an empty grid.");
        });
        Run("UI-09 Agent filters combine correctly and refresh clears selection", () =>
        {
            var service = SeedTickets();
            using var form = Open(new AgentForm(service, Agent, new RecordingFeedback()));
            var grid = Get<DataGridView>(form, "dgvComplaints");
            Require(grid.Rows.Count == 3, "Initial list is incomplete.");
            Get<ComboBox>(form, "cmbFilterCategory").SelectedItem = ComplaintCategory.Billing;
            Get<ComboBox>(form, "cmbFilterStatus").SelectedItem = ComplaintStatus.Pending;
            Get<Button>(form, "btnFilter").PerformClick();
            Require(grid.Rows.Count == 1, "Combined filters returned the wrong rows.");
            SelectFirst(grid);
            Require(Get<Button>(form, "btnSaveUpdate").Enabled, "Selecting a ticket did not enable saving.");
            Get<Button>(form, "btnRefresh").PerformClick();
            Require(grid.Rows.Count == 3 && grid.SelectedRows.Count == 0 && !Get<Button>(form, "btnSaveUpdate").Enabled, "Refresh did not reset filters/selection.");
            Get<ComboBox>(form, "cmbFilterStatus").SelectedItem = ComplaintStatus.Closed;
            Get<Button>(form, "btnFilter").PerformClick();
            Require(grid.Rows.Count == 0 && !Get<Button>(form, "btnSaveUpdate").Enabled, "Empty filtered list kept an editable selection.");
        });
        Run("UI-10 Selected-ticket update persists category status and multiline notes", () =>
        {
            var service = SeedTickets(); var messages = new RecordingFeedback();
            using var form = Open(new AgentForm(service, Agent, messages));
            var grid = Get<DataGridView>(form, "dgvComplaints"); SelectFirst(grid);
            var selected = grid.SelectedRows[0].DataBoundItem as Complaint ?? throw new InvalidOperationException("Selected row has no complaint.");
            Require(Get<ComboBox>(form, "cmbUpdateCategory").SelectedItem!.Equals(selected.Category), "Selection did not populate category.");
            Get<ComboBox>(form, "cmbUpdateStatus").SelectedItem = ComplaintStatus.Resolved;
            Get<ComboBox>(form, "cmbUpdateCategory").SelectedItem = ComplaintCategory.Service;
            Get<TextBox>(form, "txtAgentNotes").Text = "Resolved after review.\nCustomer informed.";
            Get<Button>(form, "btnSaveUpdate").PerformClick();
            var saved = new ComplaintService().TrackByReference(selected.ReferenceNumber)!;
            Require(saved.Status == ComplaintStatus.Resolved && saved.Category == ComplaintCategory.Service && saved.AgentNotes.Contains("\n"), "Saved update did not survive reload.");
            Require(messages.Messages.Single().Title == "Complaint updated", "Update confirmation is missing.");
            Require(Get<TextBox>(form, "txtAgentNotes").Multiline, "Agent notes are not multiline.");
            Capture(form, "04-agent");
        });
        Run("UI-11 Updated ticket leaving active filter clears stale selection", () =>
        {
            var service = SeedTickets();
            using var form = Open(new AgentForm(service, Agent, new RecordingFeedback()));
            Get<ComboBox>(form, "cmbFilterCategory").SelectedItem = ComplaintCategory.Technical;
            Get<Button>(form, "btnFilter").PerformClick();
            var grid = Get<DataGridView>(form, "dgvComplaints"); SelectFirst(grid);
            Get<ComboBox>(form, "cmbUpdateCategory").SelectedItem = ComplaintCategory.Service;
            Get<Button>(form, "btnSaveUpdate").PerformClick();
            Require(grid.Rows.Count == 0 && !Get<Button>(form, "btnSaveUpdate").Enabled, "Ticket outside active filters remained selected.");
        });
        Run("UI-12 Failed agent save shows error without success confirmation", () =>
        {
            var service = SeedTickets(); var messages = new RecordingFeedback();
            using var form = Open(new AgentForm(service, Agent, messages));
            var grid = Get<DataGridView>(form, "dgvComplaints"); SelectFirst(grid);
            File.Move("complaints.json", "saved-complaints.json");
            Directory.CreateDirectory("complaints.json");
            Get<ComboBox>(form, "cmbUpdateStatus").SelectedItem = ComplaintStatus.Resolved;
            Get<Button>(form, "btnSaveUpdate").PerformClick();
            Require(messages.Messages.Single().Title == "Update not saved" && messages.Messages[0].Icon == MessageBoxIcon.Error, "Failed save reported success.");
            Require(!Get<Button>(form, "btnSaveUpdate").Enabled && !grid.Enabled, "Unsafe retry remained enabled.");
        });
        Run("UI-13 Customer and agent layouts keep inputs usable at minimum size", () =>
        {
            using var customer = Open(new CustomerForm(new ComplaintService(), Customer, new RecordingFeedback()));
            customer.Size = customer.MinimumSize; Application.DoEvents();
            Capture(customer, "05-customer-minimum");
            Require(Get<TextBox>(customer, "txtDescription").Height >= 80, "Minimum-size description is too small.");
            using var agent = Open(new AgentForm(new ComplaintService(), Agent, new RecordingFeedback()));
            agent.Size = agent.MinimumSize; Application.DoEvents();
            Require(Get<DataGridView>(agent, "dgvComplaints").Height >= 120, "Minimum-size grid is too small.");
            Require(Get<TextBox>(agent, "txtAgentNotes").Height >= 45, "Minimum-size agent notes are too small.");
            Capture(agent, "06-agent-minimum");
        });

        File.WriteAllText(Path.Combine(_output, "results.json"), JsonSerializer.Serialize(new { RecordedAtUtc = DateTimeOffset.UtcNow, Results }, new JsonSerializerOptions { WriteIndented = true }));
        int failed = Results.Count(r => !r.Passed);
        Console.WriteLine($"UI checks: {Results.Count - failed} passed, {failed} failed, {Results.Count} total.");
        return failed == 0 ? 0 : 1;
    }

    private static void Run(string name, Action action)
    {
        string original = Environment.CurrentDirectory;
        string root = Path.GetFullPath(Path.Combine(_output, "data"));
        string folder = Path.Combine(root, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder); Environment.CurrentDirectory = folder;
        try { action(); Results.Add(new(name, true, "Passed")); Console.WriteLine("PASS: " + name); }
        catch (Exception ex) { Results.Add(new(name, false, ex.Message)); Console.WriteLine("FAIL: " + name + " - " + ex.Message); }
        finally
        {
            Environment.CurrentDirectory = original;
            if (!Path.GetFullPath(folder).StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Unsafe fixture cleanup path.");
            Directory.Delete(folder, recursive: true);
        }
    }

    private static T Open<T>(T form) where T : Form
    {
        form.ShowInTaskbar = false; form.Opacity = 0;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.Show(); Application.DoEvents();
        return form;
    }

    private static T Get<T>(Control form, string name) where T : Control => (T)form.Controls.Find(name, true).Single();
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void SelectFirst(DataGridView grid) { grid.CurrentCell = grid.Rows[0].Cells[0]; grid.Rows[0].Selected = true; Application.DoEvents(); }
    private static void FillComplaint(CustomerForm form)
    {
        Get<TextBox>(form, "txtTitle").Text = "Demo billing query";
        Get<ComboBox>(form, "cmbCategory").SelectedItem = ComplaintCategory.Billing;
        Get<TextBox>(form, "txtDescription").Text = "Please review the extra charge on the latest bill.";
    }
    private static ComplaintService SeedTickets()
    {
        var service = new ComplaintService();
        service.SubmitComplaint(Customer.Username, "Demo billing query", ComplaintCategory.Billing, "Review charge");
        var done = service.SubmitComplaint(Customer.Username, "Demo resolved bill", ComplaintCategory.Billing, "Charge corrected");
        service.UpdateComplaintStatus(done.ReferenceNumber, ComplaintStatus.Resolved, ComplaintCategory.Billing, "Resolved");
        service.SubmitComplaint(Customer.Username, "Demo connection issue", ComplaintCategory.Technical, "Connection is unavailable");
        return service;
    }
    private static void Capture(Form form, string name)
    {
        form.PerformLayout(); Application.DoEvents();
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
        bitmap.Save(Path.Combine(_output, name + ".png"), ImageFormat.Png);
    }
    private sealed record CheckResult(string Name, bool Passed, string Message);
    private sealed record FeedbackMessage(string Message, string Title, MessageBoxIcon Icon);
    private sealed class RecordingFeedback : IFormFeedback
    {
        public List<FeedbackMessage> Messages { get; } = new();
        public void Show(IWin32Window owner, string message, string title, MessageBoxIcon icon) => Messages.Add(new(message, title, icon));
    }
}
