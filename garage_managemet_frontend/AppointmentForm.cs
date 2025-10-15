using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace garage_managemet_frontend
{
    public partial class AppointmentForm : Form
    {
        private Guna2Panel header;
        private Guna2TextBox txtSearch;
        private Guna2Button btnAdd, btnUpdate, btnDelete;
        private Guna2DataGridView dgvAppointments;
        private readonly HttpClient httpClient = new HttpClient();
        private const string API_URL = "http://localhost:5141/api/appointment";

        public AppointmentForm()
        {
            this.Text = "Appointment Management";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            BuildUI();
            _ = LoadAppointmentsAsync();
        }

        // ---------------- UI SETUP ----------------
        private void BuildUI()
        {
            header = new Guna2Panel()
            {
                Dock = DockStyle.Top,
                Height = 70,
                FillColor = Color.White
            };
            this.Controls.Add(header);

            Label lblTitle = new Label()
            {
                Text = "📅 Appointment Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.Black
            };
            header.Controls.Add(lblTitle);

            txtSearch = new Guna2TextBox()
            {
                PlaceholderText = "Search Appointment ID...",
                Location = new Point(this.Width - 750, 20),
                Size = new Size(250, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 8,
                BorderColor = Color.LightGray
            };
            header.Controls.Add(txtSearch);

            var btnSearch = CreateButton("Search", Color.Gray, new Point(this.Width - 480, 20));
            btnSearch.Click += async (s, e) => await SearchAppointmentAsync(txtSearch.Text);
            header.Controls.Add(btnSearch);

           var btnAdd = CreateButton("Add", Color.FromArgb(0, 120, 215), new Point(this.Width - 360, 20));
           var btnUpdate = CreateButton("Update", Color.Orange, new Point(this.Width - 240, 20));
            var btnDelete = CreateButton("Delete", Color.IndianRed, new Point(this.Width - 120, 20));

            btnAdd.Click += async (s, e) => await ShowAddOrUpdateDialogAsync();
            btnUpdate.Click += async (s, e) => await ShowAddOrUpdateDialogAsync(GetSelectedAppointment());
            btnDelete.Click += async (s, e) => await DeleteSelectedAppointmentAsync();

            header.Controls.Add(btnAdd);
            header.Controls.Add(btnUpdate);
            header.Controls.Add(btnDelete);

            // Table
            dgvAppointments = new Guna2DataGridView()
            {
                Location = new Point(30, 90),
                Size = new Size(this.ClientSize.Width - 60, this.ClientSize.Height - 110),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ColumnHeadersHeight = 40,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Theme styling
            dgvAppointments.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            dgvAppointments.ThemeStyle.HeaderStyle.BackColor = Color.Black;
            dgvAppointments.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvAppointments.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvAppointments.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvAppointments.ThemeStyle.RowsStyle.ForeColor = Color.Black;
            dgvAppointments.ThemeStyle.RowsStyle.SelectionBackColor = Color.LightGray;

            this.Controls.Add(dgvAppointments);

            dgvAppointments.Columns.Add("AppointmentID", "Appointment ID");
            dgvAppointments.Columns.Add("CustomerID", "Customer ID");
            dgvAppointments.Columns.Add("VehicleID", "Vehicle ID");
            dgvAppointments.Columns.Add("Date", "Date");
            dgvAppointments.Columns.Add("Time", "Time");
            dgvAppointments.Columns.Add("ServiceType", "Service Type");
            dgvAppointments.Columns.Add("Description", "Description");
            dgvAppointments.Columns.Add("Status", "Status");

            this.Resize += (s, e) =>
            {
                txtSearch.Location = new Point(this.Width - 750, 20);
                btnAdd.Location = new Point(this.Width - 360, 20);
                btnUpdate.Location = new Point(this.Width - 240, 20);
                btnDelete.Location = new Point(this.Width - 120, 20);
                dgvAppointments.Size = new Size(this.ClientSize.Width - 60, this.ClientSize.Height - 110);
            };
        }

        private Guna2Button CreateButton(string text, Color color, Point loc)
        {
            return new Guna2Button()
            {
                Text = text,
                FillColor = color,
                ForeColor = Color.White,
                Size = new Size(100, 35),
                Location = loc,
                BorderRadius = 6,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
        }

        // ---------------- LOAD DATA ----------------
        private async Task LoadAppointmentsAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string json = await httpClient.GetStringAsync(API_URL);

                var appointments = JsonSerializer.Deserialize<Appointment[]>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                dgvAppointments.Rows.Clear();
                foreach (var a in appointments)
                {
                    dgvAppointments.Rows.Add(a.AppointmentID, a.CustomerID, a.VehicleID, a.Date, a.Time, a.ServiceType, a.Description, a.Status);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        // ---------------- SEARCH ----------------
        private async Task SearchAppointmentAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadAppointmentsAsync();
                return;
            }

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string json = await httpClient.GetStringAsync($"{API_URL}/{query}");
                var appointment = JsonSerializer.Deserialize<Appointment>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                dgvAppointments.Rows.Clear();
                if (appointment != null)
                {
                    dgvAppointments.Rows.Add(appointment.AppointmentID, appointment.CustomerID, appointment.VehicleID,
                        appointment.Date, appointment.Time, appointment.ServiceType, appointment.Description, appointment.Status);
                }
                else
                {
                    MessageBox.Show("Appointment not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching appointment: {ex.Message}");
            }
        }

        // ---------------- ADD / UPDATE ----------------
        private async Task ShowAddOrUpdateDialogAsync(Appointment existing = null)
        {
            bool isUpdate = existing != null;

            var form = new Form()
            {
                Text = isUpdate ? "Update Appointment" : "Add Appointment",
                Size = new Size(400, 600),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            int y = 30;

            Guna2TextBox CreateInput(string placeholder, string value = "")
            {
                var txt = new Guna2TextBox()
                {
                    PlaceholderText = placeholder,
                    Text = value,
                    Location = new Point(40, y),
                    Size = new Size(300, 40)
                };
                y += 50;
                form.Controls.Add(txt);
                return txt;
            }

            // Customer & Vehicle
            var txtCustomer = CreateInput("Customer ID", existing?.CustomerID.ToString());
            var txtVehicle = CreateInput("Vehicle ID", existing?.VehicleID.ToString());

            // ✅ Date Picker
            var lblDate = new Label()
            {
                Text = "Appointment Date:",
                Location = new Point(40, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            form.Controls.Add(lblDate);

            var datePicker = new Guna2DateTimePicker()
            {
                Location = new Point(40, y + 20),
                Size = new Size(300, 40),
                Value = existing?.Date ?? DateTime.Now,
                BorderRadius = 6,
                FillColor = Color.White,
                ForeColor = Color.Black,
                Format = DateTimePickerFormat.Short
            };
            form.Controls.Add(datePicker);
            y += 70;

            // ✅ Time input (pre-filled with current time)
            var lblTime = new Label()
            {
                Text = "Appointment Time:",
                Location = new Point(40, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            form.Controls.Add(lblTime);

            var txtTime = new Guna2TextBox()
            {
                PlaceholderText = "HH:mm",
                Text = existing?.Time ?? DateTime.Now.ToString("HH:mm"),
                Location = new Point(40, y + 20),
                Size = new Size(300, 40)
            };
            form.Controls.Add(txtTime);
            y += 70;

            // Other fields
            var txtServiceType = CreateInput("Service Type", existing?.ServiceType);
            var txtDescription = CreateInput("Description", existing?.Description);
            var txtStatus = CreateInput("Status", existing?.Status);

            // ✅ Save Button
            var btnSave = new Guna2Button()
            {
                Text = isUpdate ? "Update" : "Save",
                FillColor = isUpdate ? Color.Orange : Color.Black,
                ForeColor = Color.White,
                Size = new Size(120, 40),
                Location = new Point(140, y + 20),
                BorderRadius = 6
            };
            form.Controls.Add(btnSave);

            // ✅ Button Click
            btnSave.Click += async (s, e) =>
            {
                var appointment = new Appointment
                {
                    AppointmentID = existing?.AppointmentID ?? 0,
                    CustomerID = int.TryParse(txtCustomer.Text, out var c) ? c : 0,
                    VehicleID = int.TryParse(txtVehicle.Text, out var v) ? v : 0,
                    Date = datePicker.Value,
                    Time = txtTime.Text,
                    ServiceType = txtServiceType.Text,
                    Description = txtDescription.Text,
                    Status = txtStatus.Text
                };

                if (isUpdate)
                    await UpdateAppointmentAsync(appointment);
                else
                    await AddAppointmentAsync(appointment);

                form.Close();
                await LoadAppointmentsAsync();
            };

            form.ShowDialog();
        }


        private async Task AddAppointmentAsync(Appointment appointment)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                var json = JsonSerializer.Serialize(appointment);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                    MessageBox.Show("✅ Appointment added successfully!");
                else
                    MessageBox.Show($"❌ Failed to add appointment: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding appointment: {ex.Message}");
            }
        }

        private async Task UpdateAppointmentAsync(Appointment appointment)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                var json = JsonSerializer.Serialize(appointment);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"{API_URL}/{appointment.AppointmentID}", content);

                if (response.IsSuccessStatusCode)
                    MessageBox.Show("✅ Appointment updated successfully!");
                else
                    MessageBox.Show($"❌ Failed to update appointment: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating appointment: {ex.Message}");
            }
        }

        // ---------------- DELETE ----------------
        private async Task DeleteSelectedAppointmentAsync()
        {
            if (dgvAppointments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an appointment to delete.");
                return;
            }

            var id = dgvAppointments.SelectedRows[0].Cells["AppointmentID"].Value.ToString();
            if (MessageBox.Show($"Are you sure you want to delete appointment #{id}?",
                                "Confirm Delete",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning) == DialogResult.No)
                return;

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);

                var body = new { IsDelete = 1 };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{API_URL}/{id}/delete")
                {
                    Content = content
                };

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Appointment deleted successfully!");
                    await LoadAppointmentsAsync();
                }
                else
                {
                    MessageBox.Show($"❌ Failed to delete appointment: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting appointment: {ex.Message}");
            }
        }

        private Appointment GetSelectedAppointment()
        {
            if (dgvAppointments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an appointment first.");
                return null;
            }

            var r = dgvAppointments.SelectedRows[0];
            return new Appointment
            {
                AppointmentID = Convert.ToInt32(r.Cells["AppointmentID"].Value),
                CustomerID = Convert.ToInt32(r.Cells["CustomerID"].Value),
                VehicleID = Convert.ToInt32(r.Cells["VehicleID"].Value),
                Date = Convert.ToDateTime(r.Cells["Date"].Value),
                Time = r.Cells["Time"].Value.ToString(),
                ServiceType = r.Cells["ServiceType"].Value.ToString(),
                Description = r.Cells["Description"].Value.ToString(),
                Status = r.Cells["Status"].Value.ToString()
            };
        }

        // ---------------- MODEL ----------------
        public class Appointment
        {
            public int AppointmentID { get; set; }
            public int CustomerID { get; set; }
            public int VehicleID { get; set; }
            public DateTime Date { get; set; }
            public string Time { get; set; }
            public string ServiceType { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
        }
    }
}
