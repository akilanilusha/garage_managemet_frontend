using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace garage_managemet_frontend
{
    public partial class VehicleServiceForm : Form
    {
        private Panel sidebar, header;
        private Label lblTitle;
        private Guna2Button btnAddService, btnUpdate, btnDelete;
        private Guna2TextBox txtSearch;
        private Guna2DataGridView dgvServiceHistory;
        private readonly HttpClient httpClient = new HttpClient();

        public VehicleServiceForm()
        {
            this.Text = "Garage Management System - Vehicle Service History";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = Color.White;

            InitUI();
            _ = LoadVehicleDataAsync(); // Load data from URL when form loads
        }

        private void InitUI()
        {
            // Sidebar
            sidebar = new Panel()
            {
                BackColor = Color.FromArgb(25, 25, 25),
                Dock = DockStyle.Left,
                Width = 220
            };
            this.Controls.Add(sidebar);

            string[] menuItems = { "Dashboard", "Appointments", "Vehicles", "Services", "Reports", "Logout" };
            foreach (string item in menuItems)
            {
                var btn = new Guna2Button()
                {
                    Text = item,
                    Dock = DockStyle.Top,
                    FillColor = Color.Transparent,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Height = 45,
                    BorderRadius = 0,
                    HoverState = { FillColor = Color.DimGray }
                };
                sidebar.Controls.Add(btn);
            }

            // Header
            header = new Panel()
            {
                BackColor = Color.White,
                Dock = DockStyle.Top,
                Height = 65
            };
            this.Controls.Add(header);

            lblTitle = new Label()
            {
                Text = "Vehicle Service History",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(240, 18)
            };
            header.Controls.Add(lblTitle);

            txtSearch = new Guna2TextBox()
            {
                PlaceholderText = "Search Vehicle ID or License Plate...",
                Location = new Point(this.Width - 520, 15),
                Size = new Size(240, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            header.Controls.Add(txtSearch);

            btnAddService = new Guna2Button()
            {
                Text = "Add",
                FillColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                Size = new Size(75, 35),
                Location = new Point(this.Width - 260, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnAddService.Click += BtnAddService_Click;
            header.Controls.Add(btnAddService);

            btnUpdate = new Guna2Button()
            {
                Text = "Update",
                FillColor = Color.DarkOrange,
                ForeColor = Color.White,
                Size = new Size(75, 35),
                Location = new Point(this.Width - 175, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            header.Controls.Add(btnUpdate);

            btnDelete = new Guna2Button()
            {
                Text = "Delete",
                FillColor = Color.IndianRed,
                ForeColor = Color.White,
                Size = new Size(75, 35),
                Location = new Point(this.Width - 90, 15),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            header.Controls.Add(btnDelete);

            // DataGridView
            dgvServiceHistory = new Guna2DataGridView()
            {
                Location = new Point(230, 80),
                Size = new Size(this.ClientSize.Width - 250, this.ClientSize.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ColumnHeadersHeight = 40,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ThemeStyle =
                {
                    AlternatingRowsStyle = { BackColor = Color.White },
                    HeaderStyle = {
                        BackColor = Color.Black,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold)
                    },
                    RowsStyle = {
                        BackColor = Color.White,
                        ForeColor = Color.Black,
                        Font = new Font("Segoe UI", 10)
                    }
                }
            };
            this.Controls.Add(dgvServiceHistory);

            dgvServiceHistory.Columns.Add("VehicleId", "Vehicle ID");
            dgvServiceHistory.Columns.Add("LicensePlate", "License Plate");
            dgvServiceHistory.Columns.Add("ServiceDate", "Service Date");
            dgvServiceHistory.Columns.Add("Odometer", "Odometer");
            dgvServiceHistory.Columns.Add("ServicesDone", "Services Done");
            dgvServiceHistory.Columns.Add("PartsReplaced", "Parts Replaced");
            dgvServiceHistory.Columns.Add("TotalCost", "Cost");
            dgvServiceHistory.Columns.Add("NextDue", "Next Due Date");
            dgvServiceHistory.Columns.Add("Mechanic", "Mechanic");

            // Resize handler
            this.Resize += (s, e) =>
            {
                dgvServiceHistory.Size = new Size(this.ClientSize.Width - 250, this.ClientSize.Height - 100);
                txtSearch.Location = new Point(this.Width - 520, 15);
                btnAddService.Location = new Point(this.Width - 260, 15);
                btnUpdate.Location = new Point(this.Width - 175, 15);
                btnDelete.Location = new Point(this.Width - 90, 15);
            };
        }

        private async Task LoadVehicleDataAsync()
        {
            try
            {
                string url = "http://localhost:5141/api/vehicle"; // Replace with your API endpoint
                string json = await httpClient.GetStringAsync(url);

                var vehicles = JsonSerializer.Deserialize<Vehicle[]>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                dgvServiceHistory.Rows.Clear();
                foreach (var v in vehicles)
                {
                    dgvServiceHistory.Rows.Add(
                        v.VehicleId,
                        v.LicensePlate,
                        v.ServiceDate.ToString("yyyy-MM-dd"),
                        v.Odometer,
                        v.ServicesDone,
                        v.PartsReplaced,
                        "$" + v.TotalCost,
                        v.NextDueDate.ToString("yyyy-MM-dd"),
                        v.Mechanic
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddService_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add new service record UI coming soon.");
        }

        public class Vehicle
        {
            public string VehicleId { get; set; }
            public string LicensePlate { get; set; }
            public DateTime ServiceDate { get; set; }
            public int Odometer { get; set; }
            public string ServicesDone { get; set; }
            public string PartsReplaced { get; set; }
            public decimal TotalCost { get; set; }
            public DateTime NextDueDate { get; set; }
            public string Mechanic { get; set; }
        }
    }
}
