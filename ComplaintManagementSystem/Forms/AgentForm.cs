using System.Text.Json;
using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem.Forms;

public class AgentForm : Form
{
    private readonly ComplaintService _service;
    private readonly IFormFeedback _feedback;
    private bool _bindingGrid;
    private bool _saveFailed;
    private readonly ComboBox _cmbFilterCategory = Dropdown("cmbFilterCategory");
    private readonly ComboBox _cmbFilterStatus = Dropdown("cmbFilterStatus");
    private readonly Button _btnFilter = new() { Name = "btnFilter", Text = "&Filter", AutoSize = true };
    private readonly Button _btnRefresh = new() { Name = "btnRefresh", Text = "&Refresh", AutoSize = true };
    private readonly Label _lblSummary = new() { Name = "lblComplaintCount", AutoSize = true, Margin = new Padding(3, 8, 3, 8) };
    private readonly Label _lblSelectedReference = new() { Name = "lblSelectedReference", AutoSize = true };
    private readonly ComboBox _cmbUpdateCategory = Dropdown("cmbUpdateCategory");
    private readonly ComboBox _cmbUpdateStatus = Dropdown("cmbUpdateStatus");
    private readonly TextBox _txtAgentNotes = new() { Name = "txtAgentNotes", Dock = DockStyle.Fill, Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical, AccessibleName = "Resolution notes" };
    private readonly Button _btnSaveUpdate = new() { Name = "btnSaveUpdate", Text = "&Save Update", AutoSize = true, Padding = new Padding(12, 4, 12, 4) };
    private readonly DataGridView _dgvComplaints = new()
    {
        Name = "dgvComplaints", Dock = DockStyle.Fill, ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        AutoGenerateColumns = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        RowHeadersVisible = false, AllowUserToResizeRows = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = SystemColors.Window, AccessibleName = "Complaints"
    };

    public AgentForm(ComplaintService service, User currentUser, IFormFeedback? feedback = null)
    {
        SuspendLayout();
        _service = service;
        _feedback = feedback ?? MessageBoxFeedback.Default;
        Name = "AgentForm";
        Text = $"Support Workspace - {currentUser.Username} ({currentUser.Role})";
        Font = new Font("Segoe UI", 10F);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1120, 760);
        MinimumSize = new Size(960, 680);
        StartPosition = FormStartPosition.CenterScreen;

        _cmbFilterCategory.Items.Add("All categories");
        foreach (var category in Enum.GetValues<ComplaintCategory>()) _cmbFilterCategory.Items.Add(category);
        _cmbFilterCategory.SelectedIndex = 0;
        _cmbFilterStatus.Items.Add("All statuses");
        foreach (var status in Enum.GetValues<ComplaintStatus>()) _cmbFilterStatus.Items.Add(status);
        _cmbFilterStatus.SelectedIndex = 0;
        _cmbUpdateCategory.DataSource = Enum.GetValues<ComplaintCategory>();
        _cmbUpdateStatus.DataSource = Enum.GetValues<ComplaintStatus>();

        AddColumn(nameof(Complaint.ReferenceNumber), "Reference number", 150);
        AddColumn(nameof(Complaint.Title), "Title", 200);
        AddColumn(nameof(Complaint.Category), "Category", 130);
        AddColumn(nameof(Complaint.Status), "Status", 110);
        AddColumn(nameof(Complaint.SubmittedBy), "Submitted by", 130);
        AddColumn(nameof(Complaint.CreatedAt), "Created at", 155, "yyyy-MM-dd HH:mm");

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), RowCount = 5, ColumnCount = 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 240));
        layout.Controls.Add(new Label { Text = "Complaint management", Font = new Font(Font, FontStyle.Bold), AutoSize = true, Margin = new Padding(3, 0, 3, 14) }, 0, 0);
        var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 12) };
        filters.Controls.AddRange(new Control[] { FilterCaption("Category"), _cmbFilterCategory, FilterCaption("Status"), _cmbFilterStatus, _btnFilter, _btnRefresh });
        layout.Controls.Add(filters, 0, 1);
        layout.Controls.Add(_dgvComplaints, 0, 2);
        layout.Controls.Add(_lblSummary, 0, 3);
        layout.Controls.Add(CreateActionPanel(), 0, 4);
        Controls.Add(layout);

        _btnFilter.Click += (_, _) => LoadGrid();
        _btnRefresh.Click += (_, _) =>
        {
            _cmbFilterCategory.SelectedIndex = 0;
            _cmbFilterStatus.SelectedIndex = 0;
            LoadGrid();
        };
        _dgvComplaints.SelectionChanged += (_, _) => { if (!_bindingGrid) UpdateSelection(); };
        _btnSaveUpdate.Click += BtnSaveUpdate_Click;
        Shown += (_, _) => LoadGrid();
        SetEditingEnabled(false);
        ResumeLayout(true);
    }

    private static ComboBox Dropdown(string name) => new() { Name = name, Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = name };
    private static Label FilterCaption(string text) => new() { Text = text, AutoSize = true, Margin = new Padding(3, 7, 8, 3) };

    private void AddColumn(string property, string title, float weight, string format = "")
    {
        _dgvComplaints.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = property, DataPropertyName = property, HeaderText = title,
            FillWeight = weight, MinimumWidth = property == nameof(Complaint.Title) ? 140 : 100,
            DefaultCellStyle = new DataGridViewCellStyle { Format = format }
        });
    }

    private Control CreateActionPanel()
    {
        var group = new GroupBox { Text = "Update selected complaint", Dock = DockStyle.Fill, Padding = new Padding(12) };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 4 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(_lblSelectedReference, 0, 0);
        layout.SetColumnSpan(_lblSelectedReference, 4);
        layout.Controls.Add(FilterCaption("Status"), 0, 1);
        _cmbUpdateStatus.Dock = DockStyle.Fill;
        layout.Controls.Add(_cmbUpdateStatus, 1, 1);
        layout.Controls.Add(FilterCaption("Category"), 2, 1);
        _cmbUpdateCategory.Dock = DockStyle.Fill;
        layout.Controls.Add(_cmbUpdateCategory, 3, 1);
        layout.Controls.Add(_txtAgentNotes, 0, 2);
        layout.SetColumnSpan(_txtAgentNotes, 4);
        _txtAgentNotes.Margin = new Padding(3, 12, 3, 8);
        _btnSaveUpdate.Anchor = AnchorStyles.Right;
        layout.Controls.Add(_btnSaveUpdate, 3, 3);
        group.Controls.Add(layout);
        return group;
    }

    private void LoadGrid(string? selectedReference = null)
    {
        ComplaintCategory? category = _cmbFilterCategory.SelectedItem is ComplaintCategory c ? c : null;
        ComplaintStatus? status = _cmbFilterStatus.SelectedItem is ComplaintStatus s ? s : null;
        var complaints = _service.GetAllComplaints(category, status);
        _bindingGrid = true;
        try
        {
            _dgvComplaints.DataSource = complaints;
            _dgvComplaints.ClearSelection();
            _dgvComplaints.CurrentCell = null;
            if (selectedReference is not null)
            {
                foreach (DataGridViewRow row in _dgvComplaints.Rows)
                {
                    if (row.DataBoundItem is Complaint ticket && ticket.ReferenceNumber == selectedReference)
                    {
                        _dgvComplaints.CurrentCell = row.Cells[0];
                        row.Selected = true;
                        break;
                    }
                }
            }
        }
        finally { _bindingGrid = false; }
        _lblSummary.Text = complaints.Count == 0 ? "No complaints match the current filters." : $"{complaints.Count} complaint(s). Select a row to edit.";
        UpdateSelection();
    }

    private Complaint? SelectedComplaint => _dgvComplaints.SelectedRows.Count == 1
        ? _dgvComplaints.SelectedRows[0].DataBoundItem as Complaint : null;

    private void UpdateSelection()
    {
        var selected = SelectedComplaint;
        if (selected is null)
        {
            _lblSelectedReference.Text = "No complaint selected.";
            _cmbUpdateStatus.SelectedIndex = -1;
            _cmbUpdateCategory.SelectedIndex = -1;
            _txtAgentNotes.Clear();
            SetEditingEnabled(false);
            return;
        }
        _lblSelectedReference.Text = $"Reference: {selected.ReferenceNumber}";
        _cmbUpdateStatus.SelectedItem = selected.Status;
        _cmbUpdateCategory.SelectedItem = selected.Category;
        _txtAgentNotes.Text = selected.AgentNotes.ReplaceLineEndings(Environment.NewLine);
        SetEditingEnabled(!_saveFailed);
    }

    private void SetEditingEnabled(bool enabled)
    {
        _cmbUpdateStatus.Enabled = enabled;
        _cmbUpdateCategory.Enabled = enabled;
        _txtAgentNotes.Enabled = enabled;
        _btnSaveUpdate.Enabled = enabled;
    }

    private void BtnSaveUpdate_Click(object? sender, EventArgs e)
    {
        var selected = SelectedComplaint;
        if (selected is null || _saveFailed) return;
        if (_cmbUpdateStatus.SelectedItem is not ComplaintStatus status || _cmbUpdateCategory.SelectedItem is not ComplaintCategory category)
        {
            _feedback.Show(this, "Choose a status and category before saving.", "Update required", MessageBoxIcon.Warning);
            return;
        }
        try
        {
            bool saved = _service.UpdateComplaintStatus(selected.ReferenceNumber, status, category, _txtAgentNotes.Text);
            if (!saved)
            {
                _feedback.Show(this, "That complaint is no longer available. Refresh the list and select a complaint.", "Complaint not found", MessageBoxIcon.Warning);
                LoadGrid();
                return;
            }
            _feedback.Show(this, $"Saved changes to {selected.ReferenceNumber}.", "Complaint updated", MessageBoxIcon.Information);
            LoadGrid(selected.ReferenceNumber);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _saveFailed = true;
            SetEditingEnabled(false);
            _dgvComplaints.Enabled = _btnFilter.Enabled = _btnRefresh.Enabled = false;
            _feedback.Show(this, "The update could not be saved. Close and reopen the application before trying again.\n\n" + ex.Message, "Update not saved", MessageBoxIcon.Error);
        }
    }
}