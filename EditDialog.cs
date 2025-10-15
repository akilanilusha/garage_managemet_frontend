using System;
using System.Windows.Forms;

namespace garage_managemet_frontend
{
    public class EditDialog : Form
    {
        public string Customer { get; private set; }
        public string Vehicle { get; private set; }
        public string DateTime { get; private set; }
        public string Mechanic { get; private set; }
        public string Status { get; private set; }

        private TextBox txtCustomer, txtVehicle, txtDateTime, txtMechanic, txtStatus;
        private Button btnOk, btnCancel;

        public EditDialog(string customer, string vehicle, string dateTime, string mechanic, string status)
        {
            Customer = customer;
            Vehicle = vehicle;
            DateTime = dateTime;
            Mechanic = mechanic;
            Status = status;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Edit Appointment";
            this.Size = new System.Drawing.Size(400, 300);

            Label lblCustomer = new Label() { Text = "Customer:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            txtCustomer = new TextBox() { Text = Customer, Location = new System.Drawing.Point(120, 20), Width = 200 };

            Label lblVehicle = new Label() { Text = "Vehicle:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            txtVehicle = new TextBox() { Text = Vehicle, Location = new System.Drawing.Point(120, 60), Width = 200 };

            Label lblDateTime = new Label() { Text = "Date & Time:", Location = new System.Drawing.Point(20, 100), AutoSize = true };
            txtDateTime = new TextBox() { Text = DateTime, Location = new System.Drawing.Point(120, 100), Width = 200 };

            Label lblMechanic = new Label() { Text = "Mechanic:", Location = new System.Drawing.Point(20, 140), AutoSize = true };
            txtMechanic = new TextBox() { Text = Mechanic, Location = new System.Drawing.Point(120, 140), Width = 200 };

            Label lblStatus = new Label() { Text = "Status:", Location = new System.Drawing.Point(20, 180), AutoSize = true };
            txtStatus = new TextBox() { Text = Status, Location = new System.Drawing.Point(120, 180), Width = 200 };

            btnOk = new Button() { Text = "OK", Location = new System.Drawing.Point(120, 220), Width = 80 };
            btnOk.Click += (sender, e) =>
            {
                Customer = txtCustomer.Text;
                Vehicle = txtVehicle.Text;
                DateTime = txtDateTime.Text;
                Mechanic = txtMechanic.Text;
                Status = txtStatus.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            };

            btnCancel = new Button() { Text = "Cancel", Location = new System.Drawing.Point(220, 220), Width = 80 };
            btnCancel.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.Add(lblCustomer);
            this.Controls.Add(txtCustomer);
            this.Controls.Add(lblVehicle);
            this.Controls.Add(txtVehicle);
            this.Controls.Add(lblDateTime);
            this.Controls.Add(txtDateTime);
            this.Controls.Add(lblMechanic);
            this.Controls.Add(txtMechanic);
            this.Controls.Add(lblStatus);
            this.Controls.Add(txtStatus);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
        }
    }
}
