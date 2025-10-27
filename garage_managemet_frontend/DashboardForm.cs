using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace garage_managemet_frontend
{
    public partial class DashboardForm : Form
    {
        private readonly HttpClient httpClient = new HttpClient();
        private const string API_APPOINTMENT_URL = "http://localhost:5141/api/appointment";
        private const string API_PARKING_URL = "http://localhost:5141/api/parking";

        private Guna2Panel topPanel, bookingPanel, activityPanel;
        private Guna2DataGridView dgvBookings;
        private readonly Dictionary<int, Guna2Panel> slotPanels = new Dictionary<int, Guna2Panel>();
        private System.Windows.Forms.Timer parkingTimer;

        public DashboardForm()
        {
            InitializeComponent();
            BuildUI();
            _ = LoadDashboardDataAsync();

            // 🔁 Start 1s interval updates
            parkingTimer = new System.Windows.Forms.Timer();
            parkingTimer.Interval = 1000;
            parkingTimer.Tick += async (s, e) => await LoadParkingStatusAsync();
            parkingTimer.Start();
        }

        // -------------------- BUILD UI --------------------
        private void BuildUI()
        {
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true;

            // -------- TITLE --------
            Label lblTitle = new Label()
            {
                Text = "Garage Dashboard Overview",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(25, 20, 0, 0)
            };

            // -------- ACTIVITY PANEL --------
            activityPanel = new Guna2Panel()
            {
                Dock = DockStyle.Top,
                Height = 180,
                Padding = new Padding(25),
                FillColor = Color.FromArgb(28, 28, 28),
                BorderRadius = 15,
                Margin = new Padding(10)
            };

            Label lblActivity = new Label()
            {
                Text = "Recent Activity Log",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 10)
            };
            activityPanel.Controls.Add(lblActivity);

            Label lblLog = new Label()
            {
                Text = "Vehicle ABC-123 checked in by John.\nSlot 1 changed from FREE to OCCUPIED.\nPayment received from Jane Smith.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Location = new Point(25, 45),
                AutoSize = true
            };
            activityPanel.Controls.Add(lblLog);

            activityPanel.Controls.Add(CreateMetricBox("Revenue", "Rs. 12,500", 450, 40));
            activityPanel.Controls.Add(CreateMetricBox("Key Metrics", "Rs. 520", 620, 40));
            activityPanel.Controls.Add(CreateMetricBox("Checked In Today", "13.2 hrs", 450, 100));
            activityPanel.Controls.Add(CreateMetricBox("Avg Duration", "3.2 hrs", 620, 100));

            // -------- BOOKINGS PANEL --------
            bookingPanel = new Guna2Panel()
            {
                Dock = DockStyle.Top,
                Height = 260,
                Padding = new Padding(25),
                FillColor = Color.Black,
                BorderRadius = 15,
                Margin = new Padding(10)
            };

            Label lblBookings = new Label()
            {
                Text = "Upcoming Bookings",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(1200, 20),
                Location = new Point(25, 10)
            };
            bookingPanel.Controls.Add(lblBookings);

            dgvBookings = new Guna2DataGridView()
            {
                Location = new Point(25, 50),
                Size = new Size(1200, 180),
                ReadOnly = true,
                AllowUserToAddRows = false,
                ColumnHeadersHeight = 35,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ThemeStyle =
                {
                    BackColor = Color.Black,
                    HeaderStyle =
                    {
                        BackColor = Color.Black,
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 10, FontStyle.Bold)
                    },
                    RowsStyle =
                    {
                        BackColor = Color.Black,
                        ForeColor = Color.White
                    }
                }
            };
            dgvBookings.Columns.Add("Time", "Time");
            dgvBookings.Columns.Add("VehiclePlate", "Vehicle Plate");
            dgvBookings.Columns.Add("CustomerName", "Customer");
            dgvBookings.Columns.Add("Slot", "Slot");
            bookingPanel.Controls.Add(dgvBookings);

            // -------- TOP PANEL (Parking + Overview + Quick Actions) --------
            topPanel = CreateTopPanel();

            // ✅ Add in order (top → bottom)
            this.Controls.Add(activityPanel);
            this.Controls.Add(bookingPanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(lblTitle);
        }

        // -------- TOP PANEL CREATION --------
        private Guna2Panel CreateTopPanel()
        {
            var panel = new Guna2Panel()
            {
                Dock = DockStyle.Top,
                Height = 220,
                Padding = new Padding(25),
                FillColor = Color.FromArgb(28, 28, 28),
                BorderRadius = 15,
                Margin = new Padding(10)
            };

            // Left: Real-Time Slots
            var slot1 = CreateStatusCard("SLOT 1", "FREE", Color.FromArgb(0, 200, 0));
            slot1.Location = new Point(30, 30);
            var slot2 = CreateStatusCard("SLOT 2", "CAR", Color.Red);
            slot2.Location = new Point(300, 30);
            panel.Controls.Add(slot1);
            panel.Controls.Add(slot2);

            slotPanels[1] = slot1;
            slotPanels[2] = slot2;

            // Middle: Garage Overview
            var overviewPanel = new Panel()
            {
                Location = new Point(650, 25),
                Size = new Size(230, 150),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            overviewPanel.Controls.Add(CreateOverviewLabel("Total Slots: 100", 10));
            overviewPanel.Controls.Add(CreateOverviewLabel("Occupied: 45", 40));
            overviewPanel.Controls.Add(CreateOverviewLabel("Available: 55", 70));
            overviewPanel.Controls.Add(CreateOverviewLabel("Occupancy: 45%", 100));
            var progressBar = new ProgressBar()
            {
                Value = 45,
                ForeColor = Color.Orange,
                BackColor = Color.Gray,
                Size = new Size(180, 10),
                Location = new Point(20, 130)
            };
            overviewPanel.Controls.Add(progressBar);
            panel.Controls.Add(overviewPanel);

            // Right: Quick Actions
            var quickActions = new Panel()
            {
                Location = new Point(1050, 25),
                Size = new Size(220, 150),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            var btn1 = CreateActionButton("Check In / Out Vehicle");
            btn1.Location = new Point(20, 10);
            var btn2 = CreateActionButton("Create New Booking");
            btn2.Location = new Point(20, 60);
            var btn3 = CreateActionButton("View All Active Vehicles");
            btn3.Location = new Point(20, 110);
            quickActions.Controls.AddRange(new Control[] { btn1, btn2, btn3 });
            panel.Controls.Add(quickActions);

            return panel;
        }

        // -------- REUSABLE COMPONENTS --------
        private Guna2Panel CreateStatusCard(string slot, string status, Color color)
        {
            var panel = new Guna2Panel()
            {
                Size = new Size(140, 140),
                FillColor = color,
                BorderRadius = 15,
                ShadowDecoration = { Enabled = true, Color = color }
            };

            var lblSlot = new Label()
            {
                Text = slot,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(35, 10)
            };

            var icon = new Label()
            {
                Text = "🚗",
                Font = new Font("Segoe UI Emoji", 36, FontStyle.Bold),
                Location = new Point(40, 35)
            };

            var lblStatus = new Label()
            {
                Text = status,
                Name = "lblStatus",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(35, 100)
            };

            panel.Controls.Add(lblSlot);
            panel.Controls.Add(icon);
            panel.Controls.Add(lblStatus);
            return panel;
        }

        private Label CreateOverviewLabel(string text, int y)
        {
            return new Label()
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, y),
                AutoSize = true
            };
        }

        private Guna2Button CreateActionButton(string text)
        {
            return new Guna2Button()
            {
                Text = text,
                Size = new Size(180, 35),
                FillColor = Color.DodgerBlue,
                ForeColor = Color.White,
                BorderRadius = 8,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
        }

        private Guna2Panel CreateMetricBox(string title, string value, int x, int y)
        {
            var box = new Guna2Panel()
            {
                Location = new Point(x, y),
                Size = new Size(150, 50),
                FillColor = Color.FromArgb(35, 35, 35),
                BorderRadius = 8
            };

            var lblTitle = new Label()
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(10, 5)
            };

            var lblValue = new Label()
            {
                Text = value,
                ForeColor = Color.Orange,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 25)
            };

            box.Controls.Add(lblTitle);
            box.Controls.Add(lblValue);
            return box;
        }

        // -------- LOAD BOOKINGS --------
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string json = await httpClient.GetStringAsync(API_APPOINTMENT_URL);

                var appointments = JsonSerializer.Deserialize<List<Appointment>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                dgvBookings.Rows.Clear();
                foreach (var a in appointments.Take(5))
                    dgvBookings.Rows.Add(a.Date.ToString("HH:mm"), $"VEH-{a.VehicleID}", $"Customer {a.CustomerID}", "Slot 1");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}");
            }
        }

        // -------- LOAD PARKING STATUS (every 1s) --------
        private async Task LoadParkingStatusAsync()
        {
            try
            {
                string json = await httpClient.GetStringAsync(API_PARKING_URL);
                var slots = JsonSerializer.Deserialize<List<ParkingSlot>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                foreach (var slot in slots)
                {
                    if (slotPanels.TryGetValue(slot.SlotId, out var panel))
                    {
                        var lblStatus = panel.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblStatus");
                        if (lblStatus == null) continue;

                        if (slot.Status.Equals("Free", StringComparison.OrdinalIgnoreCase))
                        {
                            panel.FillColor = Color.FromArgb(0, 200, 0);
                            lblStatus.Text = "FREE";
                        }
                        else
                        {
                            panel.FillColor = Color.Red;
                            lblStatus.Text = "CAR";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating parking status: {ex.Message}");
            }
        }

        // -------- MODELS --------
        public class Appointment
        {
            public int AppointmentID { get; set; }
            public int CustomerID { get; set; }
            public int VehicleID { get; set; }
            public DateTime Date { get; set; }
            public string Status { get; set; }
        }

        public class ParkingSlot
        {
            public int SlotId { get; set; }
            public string Status { get; set; }
        }
    }
}
