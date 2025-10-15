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
    public partial class VehicleForm : Form
    {
        private Guna2Panel header;
        private Guna2TextBox txtSearch;
        private Guna2Button btnAdd, btnUpdate, btnDelete;
        private Guna2DataGridView dgvVehicles;
        private readonly HttpClient httpClient = new HttpClient();

        private const string API_URL = "http://localhost:5141/api/vehicle";
       
           

        public VehicleForm()
        {
            this.Text = "Vehicle Management";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            BuildUI();
            _ = LoadVehicleDataAsync();
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
                Text = "🚗 Vehicle Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.Black
            };
            header.Controls.Add(lblTitle);

            txtSearch = new Guna2TextBox()
            {
                PlaceholderText = "Search Vehicle ID or License Plate...",
                Location = new Point(this.Width - 750, 20),
                Size = new Size(250, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 8,
                BorderColor = Color.LightGray
            };
            header.Controls.Add(txtSearch);

            var btnSearch = CreateButton("Search", Color.Gray, new Point(this.Width - 480, 20));
            btnSearch.Click += async (s, e) => await SearchVehicleAsync(txtSearch.Text);
            header.Controls.Add(btnSearch);

            btnAdd = CreateButton("Add", Color.FromArgb(0, 120, 215), new Point(this.Width - 360, 20));
            btnUpdate = CreateButton("Update", Color.Orange, new Point(this.Width - 240, 20));
            btnDelete = CreateButton("Delete", Color.IndianRed, new Point(this.Width - 120, 20));

            btnAdd.Click += async (s, e) => await ShowAddOrUpdateDialogAsync();
            btnUpdate.Click += async (s, e) => await ShowAddOrUpdateDialogAsync(GetSelectedVehicle());
            btnDelete.Click += async (s, e) => await DeleteSelectedVehicleAsync();

            header.Controls.Add(btnAdd);
            header.Controls.Add(btnUpdate);
            header.Controls.Add(btnDelete);

            dgvVehicles = new Guna2DataGridView()
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

            // Restore original clean look
            dgvVehicles.ThemeStyle.AlternatingRowsStyle.BackColor = Color.White;
            dgvVehicles.ThemeStyle.HeaderStyle.BackColor = Color.Black;
            dgvVehicles.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvVehicles.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvVehicles.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvVehicles.ThemeStyle.RowsStyle.ForeColor = Color.Black;
            dgvVehicles.ThemeStyle.RowsStyle.SelectionBackColor = Color.LightGray;
            dgvVehicles.ThemeStyle.RowsStyle.SelectionForeColor = Color.Black;

            this.Controls.Add(dgvVehicles);

            dgvVehicles.Columns.Add("VehicleID", "Vehicle ID");
            dgvVehicles.Columns.Add("LicensePlate", "License Plate");
            dgvVehicles.Columns.Add("Make", "Make");
            dgvVehicles.Columns.Add("Model", "Model");
            dgvVehicles.Columns.Add("Year", "Year");
            dgvVehicles.Columns.Add("CustomerID", "Customer ID");

            this.Resize += (s, e) =>
            {
                txtSearch.Location = new Point(this.Width - 750, 20);
                btnAdd.Location = new Point(this.Width - 360, 20);
                btnUpdate.Location = new Point(this.Width - 240, 20);
                btnDelete.Location = new Point(this.Width - 120, 20);
                dgvVehicles.Size = new Size(this.ClientSize.Width - 60, this.ClientSize.Height - 110);
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
        private async Task LoadVehicleDataAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string json = await httpClient.GetStringAsync(API_URL);

                var vehicles = JsonSerializer.Deserialize<Vehicle[]>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                dgvVehicles.Rows.Clear();
                foreach (var v in vehicles)
                {
                    dgvVehicles.Rows.Add(v.VehicleID, v.LicensePlate, v.Make, v.Model, v.Year, v.CustomerID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        // ---------------- SEARCH ----------------
        private async Task SearchVehicleAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadVehicleDataAsync();
                return;
            }

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string url = $"{API_URL}/{query}";
                string json = await httpClient.GetStringAsync(url);

                var vehicle = JsonSerializer.Deserialize<Vehicle>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                dgvVehicles.Rows.Clear();
                if (vehicle != null)
                {
                    dgvVehicles.Rows.Add(vehicle.VehicleID, vehicle.LicensePlate, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.CustomerID);
                }
                else
                {
                    MessageBox.Show("Vehicle not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching vehicle: {ex.Message}");
            }
        }

        // ---------------- ADD / UPDATE ----------------
        private async Task ShowAddOrUpdateDialogAsync(Vehicle existing = null)
        {
            bool isUpdate = existing != null;

            var form = new Form()
            {
                Text = isUpdate ? "Update Vehicle" : "Add Vehicle",
                Size = new Size(400, 400),
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

            var txtLicense = CreateInput("License Plate", existing?.LicensePlate);
            var txtMake = CreateInput("Make", existing?.Make);
            var txtModel = CreateInput("Model", existing?.Model);
            var txtYear = CreateInput("Year", existing?.Year.ToString());
            var txtCustomer = CreateInput("Customer ID", existing?.CustomerID.ToString());

            var btnSave = new Guna2Button()
            {
                Text = isUpdate ? "Update" : "Save",
                FillColor = isUpdate ? Color.Orange : Color.Black,
                ForeColor = Color.White,
                Size = new Size(120, 40),
                Location = new Point(140, y + 10),
                BorderRadius = 6
            };
            form.Controls.Add(btnSave);

            btnSave.Click += async (s, e) =>
            {
                var vehicle = new Vehicle
                {
                    VehicleID = existing?.VehicleID ?? 0,
                    LicensePlate = txtLicense.Text,
                    Make = txtMake.Text,
                    Model = txtModel.Text,
                    Year = int.TryParse(txtYear.Text, out var year) ? year : 0,
                    CustomerID = int.TryParse(txtCustomer.Text, out var cust) ? cust : 0
                };

                if (isUpdate)
                    await UpdateVehicleAsync(vehicle);
                else
                    await AddVehicleAsync(vehicle);

                form.Close();
                await LoadVehicleDataAsync();
            };

            form.ShowDialog();
        }

        // ---------------- ADD VEHICLE ----------------
        private async Task AddVehicleAsync(Vehicle vehicle)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                var json = JsonSerializer.Serialize(vehicle);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(API_URL, content);

                MessageBox.Show(response.IsSuccessStatusCode
                    ? "✅ Vehicle added successfully!"
                    : $"❌ Failed to add vehicle: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding vehicle: {ex.Message}");
            }
        }

        // ---------------- UPDATE VEHICLE ----------------
        private async Task UpdateVehicleAsync(Vehicle vehicle)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                var json = JsonSerializer.Serialize(vehicle);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"{API_URL}/{vehicle.VehicleID}", content);

                MessageBox.Show(response.IsSuccessStatusCode
                    ? "✅ Vehicle updated successfully!"
                    : $"❌ Failed to update vehicle: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating vehicle: {ex.Message}");
            }
        }

        // ---------------- DELETE VEHICLE ----------------
        private async Task DeleteSelectedVehicleAsync()
        {
            if (dgvVehicles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a vehicle to delete.");
                return;
            }

            var id = dgvVehicles.SelectedRows[0].Cells["VehicleID"].Value.ToString();
            if (MessageBox.Show($"Are you sure you want to delete vehicle #{id}?",
                                "Confirm Delete",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning) == DialogResult.No)
                return;

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);

                var json = JsonSerializer.Serialize(new { IsDelete = 1 });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{API_URL}/{id}/delete")
                {
                    Content = content
                };

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Vehicle marked as deleted successfully!");
                    await LoadVehicleDataAsync();
                }
                else
                {
                    MessageBox.Show($"❌ Failed to delete vehicle: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting vehicle: {ex.Message}");
            }
        }
        private Vehicle GetSelectedVehicle() { if (dgvVehicles.SelectedRows.Count == 0) { MessageBox.Show("Select a vehicle first."); return null; } var r = dgvVehicles.SelectedRows[0]; return new Vehicle { VehicleID = Convert.ToInt32(r.Cells["VehicleID"].Value), LicensePlate = r.Cells["LicensePlate"].Value.ToString(), Make = r.Cells["Make"].Value.ToString(), Model = r.Cells["Model"].Value.ToString(), Year = Convert.ToInt32(r.Cells["Year"].Value), CustomerID = Convert.ToInt32(r.Cells["CustomerID"].Value) }; }

        // ---------------- MODEL ----------------
        public class Vehicle
        {
            public int VehicleID { get; set; }
            public string LicensePlate { get; set; }
            public string Make { get; set; }
            public string Model { get; set; }
            public int Year { get; set; }
            public int CustomerID { get; set; }
        }
    }
}
