using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem.Forms
{
    public class AgentForm : Form
    {
        private readonly ComplaintService _service;
        private readonly User _currentUser;

        private readonly ComboBox _cmbFilterCategory = new() { Width = 140, Top = 18, Left = 20, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cmbFilterStatus = new() { Width = 140, Top = 18, Left = 170, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Button _btnFilter = new() { Text = "Apply Filter", Width = 100, Top = 17, Left = 325 };
        private readonly Button _btnRefresh = new() { Text = "Reset", Width = 80, Top = 17, Left = 435 };

        private readonly DataGridView _dgvComplaints = new()
        {
            Top = 55,
            Left = 20,
            Width = 740,
            Height = 250,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        private readonly ComboBox _cmbUpdateCategory = new() { Width = 140, Top = 330, Left = 20, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly ComboBox _cmbUpdateStatus = new() { Width = 140, Top = 330, Left = 175, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _txtAgentNotes = new() { Width = 280, Top = 330, Left = 330, PlaceholderText = "Resolution / agent notes" };
        private readonly Button _btnSaveUpdate = new() { Text = "Save Update", Width = 135, Top = 329, Left = 625 };

        public AgentForm(ComplaintService service, User currentUser)
        {
            _service = service;
            _currentUser = currentUser;
            Text = $"Support Workspace - {_currentUser.Username} ({_currentUser.Role})";
            ClientSize = new Size(780, 385);
            StartPosition = FormStartPosition.CenterScreen;

            _cmbFilterCategory.Items.Add("All Categories");
            foreach (var c in Enum.GetValues(typeof(ComplaintCategory))) _cmbFilterCategory.Items.Add(c!);
            _cmbFilterCategory.SelectedIndex = 0;

            _cmbFilterStatus.Items.Add("All Statuses");
            foreach (var s in Enum.GetValues(typeof(ComplaintStatus))) _cmbFilterStatus.Items.Add(s!);
            _cmbFilterStatus.SelectedIndex = 0;

            _cmbUpdateCategory.DataSource = Enum.GetValues(typeof(ComplaintCategory));
            _cmbUpdateStatus.DataSource = Enum.GetValues(typeof(ComplaintStatus));

            _btnFilter.Click += (_, _) => LoadGrid();
            _btnRefresh.Click += (_, _) =>
            {
                _cmbFilterCategory.SelectedIndex = 0;
                _cmbFilterStatus.SelectedIndex = 0;
                LoadGrid();
            };
            _dgvComplaints.SelectionChanged += DgvComplaints_SelectionChanged;
            _btnSaveUpdate.Click += BtnSaveUpdate_Click;

            Controls.AddRange(new Control[]
            {
                _cmbFilterCategory, _cmbFilterStatus, _btnFilter, _btnRefresh,
                _dgvComplaints,
                _cmbUpdateCategory, _cmbUpdateStatus, _txtAgentNotes, _btnSaveUpdate
            });

            LoadGrid();
        }

        private void LoadGrid()
        {
            ComplaintCategory? cat = _cmbFilterCategory.SelectedIndex > 0
                ? (ComplaintCategory)_cmbFilterCategory.SelectedItem!
                : null;
            ComplaintStatus? status = _cmbFilterStatus.SelectedIndex > 0
                ? (ComplaintStatus)_cmbFilterStatus.SelectedItem!
                : null;

            _dgvComplaints.DataSource = _service.GetAllComplaints(cat, status);
        }

        private void DgvComplaints_SelectionChanged(object? sender, EventArgs e)
        {
            if (_dgvComplaints.CurrentRow?.DataBoundItem is Complaint selected)
            {
                _cmbUpdateCategory.SelectedItem = selected.Category;
                _cmbUpdateStatus.SelectedItem = selected.Status;
                _txtAgentNotes.Text = selected.AgentNotes;
            }
        }

        private void BtnSaveUpdate_Click(object? sender, EventArgs e)
        {
            if (_dgvComplaints.CurrentRow?.DataBoundItem is not Complaint selected)
            {
                MessageBox.Show("Select a complaint row first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var newCat = (ComplaintCategory)_cmbUpdateCategory.SelectedItem!;
            var newStatus = (ComplaintStatus)_cmbUpdateStatus.SelectedItem!;

            if (_service.UpdateComplaintStatus(selected.ReferenceNumber, newStatus, newCat, _txtAgentNotes.Text))
            {
                MessageBox.Show($"Updated {selected.ReferenceNumber}.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGrid();
            }
        }
    }
}