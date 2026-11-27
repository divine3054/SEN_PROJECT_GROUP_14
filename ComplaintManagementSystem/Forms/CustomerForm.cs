using System.Text.Json;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem.Forms;

public class CustomerForm : Form
{
    private readonly ComplaintService _service;
    private readonly User _currentUser;
    private readonly IFormFeedback _feedback;
    private readonly TextBox _txtTitle = new() { Name = "txtTitle", Dock = DockStyle.Fill, AccessibleName = "Complaint title" };
    private readonly ComboBox _cmbCategory = new() { Name = "cmbCategory", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Complaint category" };
    private readonly TextBox _txtDescription = new() { Name = "txtDescription", Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical, AcceptsReturn = true, AccessibleName = "Complaint description" };
    private readonly Button _btnSubmit = new() { Name = "btnSubmit", Text = "&Submit Complaint", AutoSize = true, Padding = new Padding(12, 5, 12, 5) };
    private readonly Label _lblGeneratedRef = new() { Name = "lblGeneratedRef", AutoSize = true, ForeColor = Color.DarkGreen, Dock = DockStyle.Fill };
    private readonly TextBox _txtSearchRef = new() { Name = "txtSearchRef", Dock = DockStyle.Fill, PlaceholderText = "CMP-2026-0001", AccessibleName = "Reference number" };
    private readonly Button _btnSearch = new() { Name = "btnSearch", Text = "&Search", AutoSize = true, Padding = new Padding(12, 3, 12, 3) };
    private readonly Label _lblSearchStatus = new() { Name = "lblSearchStatus", AutoSize = true, Dock = DockStyle.Fill };
    private readonly Label _lblTrackedReference = ReadOnlyLabel("lblTrackedReference");
    private readonly Label _lblTrackedTitle = ReadOnlyLabel("lblTrackedTitle");
    private readonly Label _lblTrackedStatus = ReadOnlyLabel("lblTrackedStatus");
    private readonly Label _lblTrackedCategory = ReadOnlyLabel("lblTrackedCategory");
    private readonly Label _lblTrackedUpdatedAt = ReadOnlyLabel("lblTrackedUpdatedAt");
    private readonly TextBox _txtTrackedAgentNotes = new() { Name = "txtTrackedAgentNotes", Dock = DockStyle.Fill, ReadOnly = true, Multiline = true, ScrollBars = ScrollBars.Vertical, AccessibleName = "Agent notes" };

    public CustomerForm(ComplaintService service, User currentUser, IFormFeedback? feedback = null)
    {
        SuspendLayout();
        _service = service;
        _currentUser = currentUser;
        _feedback = feedback ?? MessageBoxFeedback.Default;
        Name = "CustomerForm";
        Text = $"Customer Portal - {currentUser.Username}";
        Font = new Font("Segoe UI", 10F);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(760, 620);
        MinimumSize = new Size(650, 540);
        StartPosition = FormStartPosition.CenterScreen;
        _cmbCategory.DataSource = Enum.GetValues<ComplaintCategory>();

        var tabs = new TabControl { Name = "tabCustomer", Dock = DockStyle.Fill, Padding = new Point(16, 8) };
        var submitTab = new TabPage("Submit Complaint") { Name = "tabSubmit", Padding = new Padding(20) };
        var trackTab = new TabPage("Track Status") { Name = "tabTrack", Padding = new Padding(20) };
        submitTab.Controls.Add(CreateSubmitLayout());
        trackTab.Controls.Add(CreateTrackLayout());
        tabs.TabPages.AddRange(new[] { submitTab, trackTab });
        Controls.Add(tabs);

        _btnSubmit.Click += BtnSubmit_Click;
        _btnSearch.Click += BtnSearch_Click;
        _txtSearchRef.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            _btnSearch.PerformClick();
        };
        ResumeLayout(true);
    }

    private Control CreateSubmitLayout()
    {
        var layout = NewLayout(1, 9);
        for (int i = 0; i < 9; i++) layout.RowStyles.Add(new RowStyle(i == 6 ? SizeType.Percent : SizeType.AutoSize, i == 6 ? 100 : 0));
        layout.Controls.Add(new Label { Text = "Submit a complaint or general feedback", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 3, 3, 18) }, 0, 0);
        layout.Controls.Add(Caption("&Title"), 0, 1);
        layout.Controls.Add(_txtTitle, 0, 2);
        layout.Controls.Add(Caption("&Category"), 0, 3);
        layout.Controls.Add(_cmbCategory, 0, 4);
        layout.Controls.Add(Caption("&Description"), 0, 5);
        layout.Controls.Add(_txtDescription, 0, 6);
        _btnSubmit.Margin = new Padding(3, 14, 3, 12);
        layout.Controls.Add(_btnSubmit, 0, 7);
        layout.Controls.Add(_lblGeneratedRef, 0, 8);
        return layout;
    }

    private Control CreateTrackLayout()
    {
        var layout = NewLayout(2, 9);
        layout.ColumnStyles.Clear();
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 9; i++) layout.RowStyles.Add(new RowStyle(i == 8 ? SizeType.Percent : SizeType.AutoSize, i == 8 ? 100 : 0));

        var search = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 0, 0, 12) };
        search.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        search.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        search.Controls.Add(_txtSearchRef, 0, 0);
        search.Controls.Add(_btnSearch, 1, 0);
        layout.Controls.Add(search, 0, 0);
        layout.SetColumnSpan(search, 2);
        layout.Controls.Add(_lblSearchStatus, 0, 1);
        layout.SetColumnSpan(_lblSearchStatus, 2);
        AddTrackingRow(layout, "Reference", _lblTrackedReference, 2);
        AddTrackingRow(layout, "Title", _lblTrackedTitle, 3);
        AddTrackingRow(layout, "Status", _lblTrackedStatus, 4);
        AddTrackingRow(layout, "Category", _lblTrackedCategory, 5);
        AddTrackingRow(layout, "Updated at", _lblTrackedUpdatedAt, 6);
        var notesCaption = Caption("Agent notes");
        layout.Controls.Add(notesCaption, 0, 7);
        layout.SetColumnSpan(notesCaption, 2);
        layout.Controls.Add(_txtTrackedAgentNotes, 0, 8);
        layout.SetColumnSpan(_txtTrackedAgentNotes, 2);
        return layout;
    }

    private static TableLayoutPanel NewLayout(int columns, int rows)
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = columns, RowCount = rows, AutoScroll = true };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return layout;
    }

    private static Label Caption(string text) => new() { Text = text, AutoSize = true, Margin = new Padding(3, 12, 3, 5) };
    private static Label ReadOnlyLabel(string name) => new() { Name = name, AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(3, 12, 3, 5) };

    private static void AddTrackingRow(TableLayoutPanel layout, string caption, Label value, int row)
    {
        layout.Controls.Add(Caption(caption), 0, row);
        layout.Controls.Add(value, 1, row);
    }

    private void BtnSubmit_Click(object? sender, EventArgs e)
    {
        // A rejected attempt must not display a prior ticket as its success result.
        _lblGeneratedRef.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(_txtTitle.Text) || string.IsNullOrWhiteSpace(_txtDescription.Text))
        {
            _feedback.Show(this, "Enter a title and description before submitting.", "Required fields", MessageBoxIcon.Warning);
            if (string.IsNullOrWhiteSpace(_txtTitle.Text)) _txtTitle.Focus();
            else _txtDescription.Focus();
            return;
        }
        if (_cmbCategory.SelectedItem is not ComplaintCategory category)
        {
            _feedback.Show(this, "Choose a complaint category.", "Required category", MessageBoxIcon.Warning);
            _cmbCategory.Focus();
            return;
        }

        try
        {
            var created = _service.SubmitComplaint(_currentUser.Username, _txtTitle.Text, category, _txtDescription.Text);
            _lblGeneratedRef.Text = $"Reference number: {created.ReferenceNumber}";
            _feedback.Show(this, $"Your complaint has been saved.\n\nReference number: {created.ReferenceNumber}\n\nKeep this number to track its progress.", "Complaint submitted", MessageBoxIcon.Information);
            _txtTitle.Clear();
            _txtDescription.Clear();
            _txtTitle.Focus();
        }
        catch (ArgumentException ex)
        {
            _feedback.Show(this, ex.Message, "Check complaint details", MessageBoxIcon.Warning);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _btnSubmit.Enabled = false;
            _feedback.Show(this, "The complaint could not be saved. Your text is still available to copy.\n\nClose and reopen the application before trying again.\n\n" + ex.Message, "Complaint not saved", MessageBoxIcon.Error);
        }
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        ClearTrackingResults();
        if (string.IsNullOrWhiteSpace(_txtSearchRef.Text))
        {
            _lblSearchStatus.Text = "Enter a reference number to search.";
            _feedback.Show(this, _lblSearchStatus.Text, "Reference required", MessageBoxIcon.Warning);
            _txtSearchRef.Focus();
            return;
        }

        var found = _service.TrackByReference(_txtSearchRef.Text);
        if (found is null)
        {
            _lblSearchStatus.Text = "No complaint found with that reference number.";
            return;
        }
        _lblSearchStatus.Text = "Complaint found.";
        _lblTrackedReference.Text = found.ReferenceNumber;
        _lblTrackedTitle.Text = found.Title;
        _lblTrackedStatus.Text = found.Status.ToString();
        _lblTrackedCategory.Text = found.Category.ToString();
        _lblTrackedUpdatedAt.Text = found.UpdatedAt.ToString("yyyy-MM-dd HH:mm");
        _txtTrackedAgentNotes.Text = found.AgentNotes.ReplaceLineEndings(Environment.NewLine);
    }

    private void ClearTrackingResults()
    {
        foreach (var label in new[] { _lblTrackedReference, _lblTrackedTitle, _lblTrackedStatus, _lblTrackedCategory, _lblTrackedUpdatedAt }) label.Text = string.Empty;
        _txtTrackedAgentNotes.Clear();
        _lblSearchStatus.Text = string.Empty;
    }
}
