using ComplaintManagementSystem.Models;
using ComplaintManagementSystem.Services;

namespace ComplaintManagementSystem.Forms
{
    public class CustomerForm : Form
    {
        private readonly ComplaintService _service;
        private readonly User _currentUser;

        private readonly TextBox _txtTitle = new() { Width = 360, Top = 40, Left = 20 };
        private readonly ComboBox _cmbCategory = new() { Width = 200, Top = 85, Left = 20, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox _txtDescription = new() { Width = 360, Height = 100, Multiline = true, Top = 130, Left = 20 };
        private readonly Button _btnSubmit = new() { Text = "Submit Complaint", Width = 160, Height = 32, Top = 245, Left = 20 };
        private readonly Label _lblGeneratedRef = new() { Width = 360, Top = 290, Left = 20, ForeColor = Color.DarkGreen };

        private readonly TextBox _txtSearchRef = new() { Width = 220, Top = 35, Left = 20, PlaceholderText = "Enter Ref (e.g. CMP-2026-0001)" };
        private readonly Button _btnSearch = new() { Text = "Track", Width = 100, Top = 33, Left = 250 };
        private readonly Label _lblTrackResult = new() { Width = 380, Height = 180, Top = 80, Left = 20 };

        public CustomerForm(ComplaintService service, User currentUser)
        {
            _service = service;
            _currentUser = currentUser;
            Text = $"Customer Portal - {_currentUser.Username}";
            ClientSize = new Size(440, 380);
            StartPosition = FormStartPosition.CenterScreen;

            _cmbCategory.DataSource = Enum.GetValues(typeof(ComplaintCategory));

            var tabs = new TabControl { Dock = DockStyle.Fill };
            var submitTab = new TabPage("Submit Complaint");
            var trackTab = new TabPage("Track Status");

            submitTab.Controls.AddRange(new Control[]
            {
                new Label { Text = "Title:", Top = 20, Left = 20, AutoSize = true },
                _txtTitle,
                new Label { Text = "Category:", Top = 65, Left = 20, AutoSize = true },
                _cmbCategory,
                new Label { Text = "Description:", Top = 110, Left = 20, AutoSize = true },
                _txtDescription,
                _btnSubmit,
                _lblGeneratedRef
            });

            trackTab.Controls.AddRange(new Control[]
            {
                _txtSearchRef,
                _btnSearch,
                _lblTrackResult
            });

            _btnSubmit.Click += BtnSubmit_Click;
            _btnSearch.Click += BtnSearch_Click;

            tabs.TabPages.Add(submitTab);
            tabs.TabPages.Add(trackTab);
            Controls.Add(tabs);
        }

        private void BtnSubmit_Click(object? sender, EventArgs e)
        {
            try
            {
                var category = (ComplaintCategory)_cmbCategory.SelectedItem!;
                var created = _service.SubmitComplaint(_currentUser.Username, _txtTitle.Text, category, _txtDescription.Text);
                _lblGeneratedRef.Text = $"Submitted! Reference Number: {created.ReferenceNumber}";
                _txtTitle.Clear();
                _txtDescription.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            var found = _service.TrackByReference(_txtSearchRef.Text);
            if (found == null)
            {
                _lblTrackResult.Text = "No complaint found with that reference number.";
                return;
            }

            _lblTrackResult.Text =
                $"Reference: {found.ReferenceNumber}\n" +
                $"Title: {found.Title}\n" +
                $"Category: {found.Category}\n" +
                $"Status: {found.Status}\n" +
                $"Updated: {found.UpdatedAt:yyyy-MM-dd HH:mm}\n" +
                $"Notes: {found.AgentNotes}";
        }
    }
}
