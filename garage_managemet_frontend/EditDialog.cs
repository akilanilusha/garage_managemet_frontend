using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace garage_managemet_frontend
{
    public partial class EditDialog : Form
    {
        public string AppointmentId => txtId.Text;
        public string Customer => txtCustomer.Text;
        public string Vehicle => txtVehicle.Text;
        public string DateTime => txtDateTime.Text;
        public string Mechanic => txtMechanic.Text;
        public string Status => cmbStatus.SelectedItem.ToString();

        private Guna2TextBox txtId, txtCustomer, txtVehicle, txtDateTime, txtMechanic;
        private Guna2ComboBox cmbStatus;

        public EditDialog(string id, string customer, string vehicle, string date, string mechanic, string status)
        {
            this.Text = "Edit Appointment";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            Label header = new Label
            {
                Text = "Edit Appointment",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(120, 20)
            };
            this.Controls.Add(header);

            int y = 60;

            txtId = CreateTextBox("ID", id, y); y += 50;
            txtCustomer = CreateTextBox("Customer", customer, y); y += 50;
            txtVehicle = CreateTextBox("Vehicle", vehicle, y); y += 50;
            txtDateTime = CreateTextBox("Date & Time", date, y); y += 50;
            txtMechanic = CreateTextBox("Mechanic", mechanic, y); y += 50;

            cmbStatus = new Guna2ComboBox
            {
                Location = new Point(50, y),
                Size = new Size(280, 36),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new string[] { "Pending", "Confirmed", "Completed" });
            cmbStatus.SelectedItem = status;
            this.Controls.Add(cmbStatus);

            var btnSave = new Guna2Button
            {
                Text = "Save",
                FillColor = Color.Green,
                ForeColor = Color.White,
                Location = new Point(70, y + 60),
                Size = new Size(100, 40)
            };
            btnSave.Click += (s, e) => { this.DialogResult = DialogResult.OK; Close(); };

            var btnCancel = new Guna2Button
            {
                Text = "Cancel",
                FillColor = Color.Gray,
                ForeColor = Color.White,
                Location = new Point(200, y + 60),
                Size = new Size(100, 40)
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; Close(); };

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private Guna2TextBox CreateTextBox(string placeholder, string value, int y)
        {
            var txt = new Guna2TextBox()
            {
                PlaceholderText = placeholder,
                Text = value,
                Location = new Point(50, y),
                Size = new Size(280, 36)
            };
            this.Controls.Add(txt);
            return txt;
        }
    }
}
