using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace garage_managemet_frontend
{
    public partial class AppointmentDialog : Form
    {
        public string Customer => txtCustomer.Text;
        public string Vehicle => txtVehicle.Text;
        public DateTime DateTime => dtDateTime.Value;
        public string Mechanic => txtMechanic.Text;
        public string Status => cmbStatus.SelectedItem.ToString();

        private Guna2TextBox txtCustomer, txtVehicle, txtMechanic;
        private Guna2ComboBox cmbStatus;
        private DateTimePicker dtDateTime;

        public AppointmentDialog(string customer = "", string vehicle = "", DateTime? datetime = null, string mechanic = "", string status = "Pending")
        {
            this.Text = "Appointment Details";
            this.Size = new Size(400, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            int y = 30;
            txtCustomer = CreateTextBox("Customer", customer, ref y);
            txtVehicle = CreateTextBox("Vehicle", vehicle, ref y);

            dtDateTime = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd hh:mm tt",
                Location = new Point(40, y),
                Size = new Size(300, 35),
                Value = datetime ?? DateTime.Now
            };
            this.Controls.Add(dtDateTime);
            y += 50;

            txtMechanic = CreateTextBox("Mechanic", mechanic, ref y);

            cmbStatus = new Guna2ComboBox
            {
                Location = new Point(40, y),
                Size = new Size(300, 35),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStatus.Items.AddRange(new[] { "Pending", "Confirmed", "Completed" });
            cmbStatus.SelectedItem = status;
            this.Controls.Add(cmbStatus);
            y += 50;

            var btn = new Guna2Button()
            {
                Text = "Save",
                Location = new Point(120, y),
                Size = new Size(140, 40),
                FillColor = Color.Green,
                ForeColor = Color.White
            };
            btn.Click += (s, e) => { this.DialogResult = DialogResult.OK; Close(); };
            this.Controls.Add(btn);
        }

        private Guna2TextBox CreateTextBox(string placeholder, string value, ref int y)
        {
            var txt = new Guna2TextBox()
            {
                PlaceholderText = placeholder,
                Text = value,
                Location = new Point(40, y),
                Size = new Size(300, 35)
            };
            this.Controls.Add(txt);
            y += 50;
            return txt;
        }
    }
}
