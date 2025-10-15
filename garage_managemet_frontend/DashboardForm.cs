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
using System.Windows.Forms.DataVisualization.Charting;

namespace garage_managemet_frontend
{
    public partial class DashboardForm : Form
    {
        private Label lblTitle;
        private Guna2DataGridView dgvAppointments;
        private Chart statusChart;
        private TableLayoutPanel cardPanel;

        private readonly HttpClient httpClient = new HttpClient();
        private const string API_URL = "http://localhost:5141/api/appointment";
        

        public DashboardForm()
        {
            
            InitializeComponent();
            BuildDashboard();
            _ = LoadDashboardDataAsync();
        }

        private void BuildDashboard()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.AutoScroll = true;

            lblTitle = new Label()
            {
                Text = "📊 Garage Dashboard Overview",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Black,
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 15, 0, 0)
            };
            this.Controls.Add(lblTitle);

            // Stat cards panel
            cardPanel = new TableLayoutPanel()
            {
                Dock = DockStyle.Top,
                Height = 130,
                ColumnCount = 3,
                Padding = new Padding(20, 10, 20, 10),
                AutoSize = true
            };
            cardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            cardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            cardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
            this.Controls.Add(cardPanel);

            // Content area (Table + Chart)
            var contentPanel = new Panel()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40),
                AutoScroll = true
            };
            this.Controls.Add(contentPanel);

            dgvAppointments = new Guna2DataGridView()
            {
                Dock = DockStyle.Left,
                Width = this.Width / 2 - 40,
                ReadOnly = true,
                AllowUserToAddRows = false,
                ColumnHeadersHeight = 35,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None
            };
            dgvAppointments.ThemeStyle.HeaderStyle.BackColor = Color.Black;
            dgvAppointments.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvAppointments.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvAppointments.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvAppointments.ThemeStyle.RowsStyle.ForeColor = Color.Black;
            dgvAppointments.ThemeStyle.RowsStyle.SelectionBackColor = Color.LightGray;
            dgvAppointments.ThemeStyle.RowsStyle.Font = new Font("Segoe UI", 10);

            dgvAppointments.Columns.Add("CustomerID", "Customer");
            dgvAppointments.Columns.Add("VehicleID", "Vehicle");
            dgvAppointments.Columns.Add("DateTime", "Date & Time");
            dgvAppointments.Columns.Add("ServiceType", "Service Type");
            dgvAppointments.Columns.Add("Status", "Status");

            contentPanel.Controls.Add(dgvAppointments);

            statusChart = new Chart()
            {
                Dock = DockStyle.Right,
                Width = this.Width / 3,
                Palette = ChartColorPalette.Bright
            };
            var chartArea = new ChartArea("Main");
            statusChart.ChartAreas.Add(chartArea);
            statusChart.Legends.Add(new Legend("Legend"));
            contentPanel.Controls.Add(statusChart);

            this.Resize += (s, e) =>
            {
                int width = this.ClientSize.Width / 2 - 40;
                dgvAppointments.Width = Math.Max(width, 400);
                statusChart.Width = Math.Max(width - 100, 300);
            };
        }

        private Panel CreateCard(string title, string count, Color color)
        {
            var panel = new Panel()
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BackColor = color,
                Padding = new Padding(10),
                Height = 100
            };

            var lblTitle = new Label()
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top
            };

            var lblCount = new Label()
            {
                Text = count,
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Bottom,
                Height = 45,
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.Add(lblTitle);
            panel.Controls.Add(lblCount);
            return panel;
        }

        // ---------------- Load Dashboard Data ----------------
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string json = await httpClient.GetStringAsync(API_URL);

                var appointments = JsonSerializer.Deserialize<List<Appointment>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (appointments == null) return;

                int pending = appointments.Count(a => a.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
                int confirmed = appointments.Count(a => a.Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase));
                int completed = appointments.Count(a => a.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase));

                // Load Stat Cards
                cardPanel.Controls.Clear();
                cardPanel.Controls.Add(CreateCard("Pending", pending.ToString(), Color.Orange));
                cardPanel.Controls.Add(CreateCard("Confirmed", confirmed.ToString(), Color.DodgerBlue));
                cardPanel.Controls.Add(CreateCard("Completed", completed.ToString(), Color.MediumSeaGreen));

                // Load Table
                dgvAppointments.Rows.Clear();
                foreach (var a in appointments.OrderByDescending(a => a.Date).Take(10))
                {
                    dgvAppointments.Rows.Add(a.CustomerID, a.VehicleID, a.Date.ToString("yyyy-MM-dd HH:mm"), a.ServiceType, a.Status);
                }

                // Load Chart
                statusChart.Series.Clear();
                var series = new Series("Status Distribution")
                {
                    ChartType = SeriesChartType.Pie,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold)
                };
                series.Points.AddXY("Pending", pending);
                series.Points.AddXY("Confirmed", confirmed);
                series.Points.AddXY("Completed", completed);
                statusChart.Series.Add(series);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}");
            }
        }

        // ---------------- Model ----------------
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
            public int Is_Delete { get; set; }
        }
    }
}
