using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace garage_managemet_frontend
{
    public partial class ServiceForm : Form
    {
        private Guna2Panel header;
        private Guna2DataGridView dgvAppointments, dgvParts;
        private readonly HttpClient httpClient = new HttpClient();
        private const string API_SERVICE_URL = "http://localhost:5141/api/service";
        private const string API_APPOINTMENT_URL = "http://localhost:5141/api/appointment/confirmed";
        private const string API_MECHANIC_URL = "http://localhost:5141/api/mechanic";
        private const string API_PAYMENT_URL = "http://localhost:5141/api/payment";
        private List<ServicePart> currentParts = new List<ServicePart>();

        // Inputs
        private Guna2TextBox txtVehicle, txtApp, txtStart, txtEnd, txtPay, txtDesc;
        private Guna2ComboBox cmbMechanic;
        private Label lblTotalAmount;

        public ServiceForm()
        {
            this.Text = "Service Management";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            BuildUI();
            this.Load += async (s, e) =>
            {
                await LoadConfirmedAppointmentsAsync();
                await LoadMechanicsAsync();
            };
        }

        // ---------------- UI SETUP ----------------
        private void BuildUI()
        {
            // Header
            header = new Guna2Panel()
            {
                Dock = DockStyle.Top,
                Height = 70,
                FillColor = Color.White
            };
            this.Controls.Add(header);

            Label lblTitle = new Label()
            {
                Text = "🧾 Confirmed Appointments → Create Service",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.Black
            };
            header.Controls.Add(lblTitle);

            // --- Appointment Table ---
            dgvAppointments = new Guna2DataGridView()
            {
                Location = new Point(30, 90),
                Size = new Size(this.ClientSize.Width - 60, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnHeadersHeight = 40,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvAppointments.ThemeStyle.HeaderStyle.BackColor = Color.Black;
            dgvAppointments.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvAppointments.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvAppointments.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvAppointments.ThemeStyle.RowsStyle.ForeColor = Color.Black;
            dgvAppointments.ThemeStyle.RowsStyle.SelectionBackColor = Color.LightGray;

            dgvAppointments.Columns.Add("AppointmentID", "Appointment ID");
            dgvAppointments.Columns.Add("CustomerID", "Customer ID");
            dgvAppointments.Columns.Add("VehicleID", "Vehicle ID");
            dgvAppointments.Columns.Add("Date", "Date");
            dgvAppointments.Columns.Add("Time", "Time");
            dgvAppointments.Columns.Add("ServiceType", "Service Type");
            dgvAppointments.Columns.Add("Description", "Description");
            dgvAppointments.Columns.Add("Status", "Status");

            this.Controls.Add(dgvAppointments);

            dgvAppointments.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var row = dgvAppointments.Rows[e.RowIndex];
                    txtApp.Text = row.Cells["AppointmentID"].Value.ToString();
                    txtVehicle.Text = row.Cells["VehicleID"].Value.ToString();
                    txtStart.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    txtEnd.Text = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                    txtDesc.Text = row.Cells["Description"].Value?.ToString() ?? "";
                }
            };

            // --- Service Input Section ---
            int startY = 210;
            Label lblSection = new Label()
            {
                Text = "➕ Add Service for Selected Appointment",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(30, startY),
                AutoSize = true
            };
            this.Controls.Add(lblSection);
            startY += 40;

            int x = 30;
            int gap = 210;

            txtVehicle = CreateTextBox("Vehicle ID", x, startY);
            txtApp = CreateTextBox("Appointment ID", x + gap, startY);

            cmbMechanic = new Guna2ComboBox()
            {
                Location = new Point(x + gap * 2, startY),
                Size = new Size(180, 40),
                BorderRadius = 6,
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            this.Controls.Add(cmbMechanic);

            startY += 60;
            txtStart = CreateTextBox("Start Date (YYYY-MM-DD)", x, startY);
            txtEnd = CreateTextBox("End Date (YYYY-MM-DD)", x + gap, startY);
            txtPay = CreateTextBox("Service ID (after save)", x + gap * 2, startY);
            startY += 60;

            txtDesc = CreateTextBox("Description", x, startY, 400);
            startY += 70;

            // --- Add Part Button ---
            var btnAddPart = new Guna2Button()
            {
                Text = "+ Add Part",
                Location = new Point(x, startY),
                Size = new Size(150, 40),
                FillColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                BorderRadius = 6
            };
            btnAddPart.Click += async (s, e) =>
            {
                var part = await ShowAddPartDialogAsync();
                if (part != null)
                {
                    currentParts.Add(part);
                    dgvParts.Rows.Add(part.PartID, part.PartName, part.Quantity, part.UnitPrice, part.Total);
                    UpdateTotalLabel();
                }
            };
            this.Controls.Add(btnAddPart);
            startY += 50;

            // --- Parts Summary Table ---
            dgvParts = new Guna2DataGridView()
            {
                Location = new Point(x, startY),
                Size = new Size(this.ClientSize.Width - 60, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnHeadersHeight = 35,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            dgvParts.Columns.Add("PartID", "Part ID");
            dgvParts.Columns.Add("PartName", "Part Name");
            dgvParts.Columns.Add("Qty", "Qty");
            dgvParts.Columns.Add("UnitPrice", "Unit Price");
            dgvParts.Columns.Add("Total", "Total");
            this.Controls.Add(dgvParts);

            startY += 200;

            // --- Total Label ---
            // --- Total Label ---
            lblTotalAmount = new Label()
            {
                Text = "Total Amount: $0.00",
                Location = new Point(x, startY),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.DarkGreen
            };
            this.Controls.Add(lblTotalAmount);
            

            // --- Save Service Button ---
            var btnSave = new Guna2Button()
            {
                Text = "Save Service",
                Location = new Point(x + 190, startY),
                Size = new Size(160, 45),
                FillColor = Color.Black,
                ForeColor = Color.White,
                BorderRadius = 6
            };
            btnSave.Click += async (s, e) => await SaveServiceAsync();
            this.Controls.Add(btnSave);

            // --- Pay Button ---
            var btnPay = new Guna2Button()
            {
                Text = "💳 Pay",
                Location = new Point(x + 380, startY), // move right of Save
                Size = new Size(160, 45),
                FillColor = Color.FromArgb(0, 150, 0),
                ForeColor = Color.White,
                BorderRadius = 6
            };
            btnPay.Click += async (s, e) => await ShowPaymentDialogAsync();
            this.Controls.Add(btnPay);

            // 🔧 Force layout update
            this.PerformLayout();

        }



        private void UpdateTotalLabel()
        {
            decimal total = 0;
            foreach (var p in currentParts)
                total += p.Total;

            lblTotalAmount.Text = $"Total Amount: ${total:F2}";
        }

        private Guna2TextBox CreateTextBox(string placeholder, int x, int y, int width = 180)
        {
            var txt = new Guna2TextBox()
            {
                PlaceholderText = placeholder,
                Location = new Point(x, y),
                Size = new Size(width, 40),
                BorderRadius = 6
            };
            this.Controls.Add(txt);
            return txt;
        }

        // ---------------- LOAD CONFIRMED APPOINTMENTS ----------------
        private async Task LoadConfirmedAppointmentsAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);

                string json = await httpClient.GetStringAsync(API_APPOINTMENT_URL);
                var appointments = JsonSerializer.Deserialize<List<Appointment>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                dgvAppointments.Rows.Clear();
                foreach (var a in appointments ?? new List<Appointment>())
                {
                    dgvAppointments.Rows.Add(a.AppointmentID, a.CustomerID, a.VehicleID, a.Date,
                        a.Time, a.ServiceType, a.Description, a.Status);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading appointments: {ex.Message}");
            }
        }

        // ---------------- LOAD MECHANICS ----------------
        private async Task LoadMechanicsAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);

                string json = await httpClient.GetStringAsync(API_MECHANIC_URL);
                var mechanics = JsonSerializer.Deserialize<List<Mechanic>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                cmbMechanic.Items.Clear();
                foreach (var m in mechanics ?? new List<Mechanic>())
                    cmbMechanic.Items.Add($"{m.MechanicID} - {m.Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading mechanics: {ex.Message}");
            }
        }

        // ---------------- SAVE SERVICE ----------------
        private async Task SaveServiceAsync()
        {
            try
            {
                if (cmbMechanic.SelectedItem == null)
                {
                    MessageBox.Show("Please select a mechanic!");
                    return;
                }

                int mechanicID = int.Parse(cmbMechanic.SelectedItem.ToString().Split('-')[0].Trim());

                var svc = new Service
                {
                    VehicleID = int.Parse(txtVehicle.Text),
                    AppointmentID = int.Parse(txtApp.Text),
                    MechanicID = mechanicID,
                    StartDate = DateTime.Parse(txtStart.Text),
                    EndDate = DateTime.Parse(txtEnd.Text),
                    Description = txtDesc.Text,
                    PartsUsed = currentParts
                };

                var json = JsonSerializer.Serialize(svc);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);

                var res = await httpClient.PostAsync(API_SERVICE_URL, content);

                if (res.IsSuccessStatusCode)
                {
                    // ✅ Parse response JSON to get Service ID
                    var responseBody = await res.Content.ReadAsStringAsync();
                    var createdService = JsonSerializer.Deserialize<Service>(
                        responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (createdService != null)
                    {
                        txtPay.Text = createdService.ServiceID.ToString(); // 🧠 Auto-fill Service ID field
                        MessageBox.Show($"✅ Service Created Successfully! (Service ID: {createdService.ServiceID})");
                    }
                    else
                    {
                        MessageBox.Show("✅ Service Created, but failed to retrieve ID from server.");
                    }
                }
                else
                {
                    MessageBox.Show("❌ Failed to create service!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving service: {ex.Message}");
            }
        }



        // ---------------- ADD PART DIALOG ----------------
        private async Task<ServicePart> ShowAddPartDialogAsync()
        {
            var dialog = new Form()
            {
                Text = "Add Part",
                Size = new Size(450, 450),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            var txtSearch = new Guna2TextBox()
            {
                PlaceholderText = "Search item name...",
                Location = new Point(30, 30),
                Size = new Size(250, 35)
            };
            dialog.Controls.Add(txtSearch);

            var btnSearch = new Guna2Button()
            {
                Text = "Search",
                Location = new Point(290, 30),
                Size = new Size(80, 35),
                FillColor = Color.Black,
                ForeColor = Color.White
            };
            dialog.Controls.Add(btnSearch);

            var lstItems = new ListBox()
            {
                Location = new Point(30, 80),
                Size = new Size(340, 150),
                Font = new Font("Segoe UI", 10)
            };
            dialog.Controls.Add(lstItems);

            var txtQty = new Guna2TextBox()
            {
                PlaceholderText = "Quantity",
                Location = new Point(30, 250),
                Size = new Size(120, 35)
            };
            dialog.Controls.Add(txtQty);

            var lblTotal = new Label()
            {
                Text = "Total: $0.00",
                Location = new Point(30, 300),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            dialog.Controls.Add(lblTotal);

            var btnAdd = new Guna2Button()
            {
                Text = "Add Part",
                Location = new Point(250, 350),
                Size = new Size(120, 40),
                FillColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            dialog.Controls.Add(btnAdd);

            Item selectedItem = null;
            ServicePart result = null;

            // 🔍 Search button logic
            btnSearch.Click += async (s, e) =>
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                    string json = await httpClient.GetStringAsync($"http://localhost:5141/api/Item/search?name={txtSearch.Text}");
                    var items = JsonSerializer.Deserialize<List<Item>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    lstItems.Items.Clear();
                    foreach (var i in items)
                        lstItems.Items.Add($"{i.ItemID} - {i.ItemName} (${i.UnitPrice:F2})");
                }
                catch
                {
                    MessageBox.Show("Error searching items. Please check your API connection.");
                }
            };

            // Select item
            lstItems.SelectedIndexChanged += (s, e) =>
            {
                if (lstItems.SelectedItem == null) return;
                var txt = lstItems.SelectedItem.ToString();
                var id = int.Parse(txt.Split('-')[0].Trim());
                var name = txt.Split('-')[1].Split('(')[0].Trim();
                var priceText = txt.Split('$')[1].Replace(")", "").Trim();
                decimal.TryParse(priceText, out var price);
                selectedItem = new Item { ItemID = id, ItemName = name, UnitPrice = price };
            };

            // Quantity & total calculation
            txtQty.TextChanged += (s, e) =>
            {
                if (selectedItem == null) return;
                if (int.TryParse(txtQty.Text, out var q))
                    lblTotal.Text = $"Total: ${(selectedItem.UnitPrice * q):F2}";
            };

            // Add button logic
            btnAdd.Click += (s, e) =>
            {
                if (selectedItem == null || !int.TryParse(txtQty.Text, out var q))
                {
                    MessageBox.Show("Select a part and enter valid quantity.");
                    return;
                }

                result = new ServicePart
                {
                    PartID = selectedItem.ItemID,
                    PartName = selectedItem.ItemName,
                    Quantity = q,
                    UnitPrice = selectedItem.UnitPrice,
                    Total = selectedItem.UnitPrice * q
                };

                dialog.Close();
            };

            dialog.ShowDialog();
            return result;
        }

        // ---------------- PAYMENT DIALOG ----------------
        private async Task ShowPaymentDialogAsync()
        {
            if (currentParts.Count == 0)
            {
                MessageBox.Show("Please add parts before making a payment!");
                return;
            }

            decimal totalAmount = 0;
            foreach (var p in currentParts)
                totalAmount += p.Total;

            var dialog = new Form()
            {
                Text = "Complete Payment",
                Size = new Size(400, 450),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            var lblType = new Label()
            {
                Text = "Payment Type:",
                Location = new Point(40, 40),
                Font = new Font("Segoe UI", 10)
            };
            dialog.Controls.Add(lblType);

            var cmbType = new Guna2ComboBox()
            {
                Location = new Point(40, 65),
                Size = new Size(300, 35),
                BorderRadius = 6,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbType.Items.AddRange(new string[] { "Cash", "Card", "Online" });
            dialog.Controls.Add(cmbType);

            var lblTotal = new Label()
            {
                Text = $"Total Amount: ${totalAmount:F2}",
                Location = new Point(40, 120),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true
            };
            dialog.Controls.Add(lblTotal);

            var txtGiven = new Guna2TextBox()
            {
                PlaceholderText = "Enter amount given...",
                Location = new Point(40, 170),
                Size = new Size(300, 40),
                BorderRadius = 6
            };
            dialog.Controls.Add(txtGiven);

            var lblBalance = new Label()
            {
                Text = "Balance: $0.00",
                Location = new Point(40, 230),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.DarkBlue
            };
            dialog.Controls.Add(lblBalance);

            txtGiven.TextChanged += (s, e) =>
            {
                if (decimal.TryParse(txtGiven.Text, out decimal given))
                {
                    decimal balance = given - totalAmount;
                    lblBalance.Text = $"Balance: ${balance:F2}";
                    lblBalance.ForeColor = balance < 0 ? Color.Red : Color.DarkGreen;
                }
                else lblBalance.Text = "Balance: $0.00";
            };

            var btnPay = new Guna2Button()
            {
                Text = "Confirm Payment",
                Location = new Point(100, 320),
                Size = new Size(180, 45),
                FillColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                BorderRadius = 6
            };
            dialog.Controls.Add(btnPay);

            btnPay.Click += async (s, e) =>
            {
                if (cmbType.SelectedItem == null)
                {
                    MessageBox.Show("Please select a payment type!");
                    return;
                }

                if (!decimal.TryParse(txtGiven.Text, out decimal givenAmount))
                {
                    MessageBox.Show("Please enter a valid amount!");
                    return;
                }

                if (givenAmount < totalAmount)
                {
                    MessageBox.Show("Given amount is less than total!");
                    return;
                }

                var payment = new Payment
                {
                    Amount = totalAmount,
                    Date = DateTime.Now,
                    PaymentType = cmbType.SelectedItem.ToString(),
                    ServiceID = int.Parse(txtPay.Text)
                };

                var json = JsonSerializer.Serialize(payment);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                    var res = await httpClient.PostAsync(API_PAYMENT_URL, content);

                    if (res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ Payment recorded successfully!");
                        dialog.Close();
                    }
                    else
                        MessageBox.Show("❌ Payment failed to record!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing payment: {ex.Message}");
                }
            };

            dialog.ShowDialog();
        }

        // ---------------- MODELS ----------------
        public class Mechanic { public int MechanicID { get; set; } public string Name { get; set; } }
        public class Appointment { public int AppointmentID { get; set; } public int CustomerID { get; set; } public int VehicleID { get; set; } public DateTime Date { get; set; } public string Time { get; set; } public string ServiceType { get; set; } public string Description { get; set; } public string Status { get; set; } }
        public class Item { public int ItemID { get; set; } public string ItemName { get; set; } public decimal UnitPrice { get; set; } }
        public class ServicePart { public int PartID { get; set; } public string PartName { get; set; } public int Quantity { get; set; } public decimal UnitPrice { get; set; } public decimal Total { get; set; } }
        public class Service
        {
            public int ServiceID { get; set; }   // ✅ Match backend property name
            public int VehicleID { get; set; }
            public int AppointmentID { get; set; }
            public int MechanicID { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string Description { get; set; }
            public List<ServicePart> PartsUsed { get; set; }
        }
        public class Payment { public int PaymentID { get; set; } public decimal Amount { get; set; } public DateTime Date { get; set; } public string PaymentType { get; set; } public int ServiceID { get; set; } }
    }
}
