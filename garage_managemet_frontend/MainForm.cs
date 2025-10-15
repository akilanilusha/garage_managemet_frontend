using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace garage_managemet_frontend
{
    public partial class MainForm : Form
    {
        private Guna2Panel sidebar;
        private Panel topPanel;
        private Guna2Panel containerPanel;
        private Guna2PictureBox titleIcon;
        private Label titleLabel;
        private Guna2Button btnDashboard, btnAppointments, btnVehicles, btnCustomers, btnMechanics, btnReports, btnLogout;

        public MainForm()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            this.Text = "Garage Management System";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = Color.Black;

            Color dark = Color.FromArgb(28, 28, 28);

            containerPanel = new Guna2Panel
            {
                Dock = DockStyle.Fill,
                FillColor = Color.White
            };

            sidebar = new Guna2Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                FillColor = dark,
                Padding = new Padding(0, 0, 0, 0),
                AutoScroll = true
            };

            var sidebarHeader = new Label
            {
                Text = "🚗 GarageSys",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };
            sidebar.Controls.Add(sidebarHeader);

            sidebar.Controls.Add(new Guna2Separator()
            {
                Dock = DockStyle.Top,
                Height = 2,
                FillColor = Color.Black
            });

            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White
            };

            titleIcon = new Guna2PictureBox
            {
                Size = new Size(32, 32),
                Location = new Point(20, 14),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.dashboard_icon
            };
            topPanel.Controls.Add(titleIcon);

            titleLabel = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(60, 18),
                AutoSize = true,
                ForeColor = Color.Black,
                BackColor=Color.White
                
            };
            topPanel.Controls.Add(titleLabel);

            this.Controls.Add(containerPanel);
            this.Controls.Add(sidebar);
            this.Controls.Add(topPanel);

            Panel buttonsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                BackColor = Color.Black
            };
            buttonsPanel.Controls.Add(CreateSidebarButton("User Management", Properties.Resources.dashboard_icon, (s, e) => LoadForm(new UserManagementForm(), "User Management", Properties.Resources.dashboard_icon)));
            buttonsPanel.Controls.Add(CreateSidebarButton("Reports", Properties.Resources.report_icon, (s, e) => LoadForm(new ReportForm(), "Reports", Properties.Resources.report_icon)));
            buttonsPanel.Controls.Add(CreateSidebarButton("Mechanics", Properties.Resources.mechanic_icon, (s, e) => LoadForm(new MechanicForm(), "Mechanics", Properties.Resources.mechanic_icon)));
            buttonsPanel.Controls.Add(CreateSidebarButton("Customers", Properties.Resources.customer_icon, (s, e) => LoadForm(new CustomerForm(), "Customers", Properties.Resources.customer_icon)));
            buttonsPanel.Controls.Add(CreateSidebarButton("Vehicles", Properties.Resources.car_icon, (s, e) => LoadForm(new VehicleForm(), "Vehicles", Properties.Resources.car_icon)));
            buttonsPanel.Controls.Add(CreateSidebarButton("Appointments", Properties.Resources.calendar_icon, (s, e) => LoadForm(new AppointmentForm(), "Appointments", Properties.Resources.calendar_icon)));
            buttonsPanel.Controls.Add(CreateSidebarButton("Dashboard", Properties.Resources.dashboard_icon, (s, e) => LoadForm(new DashboardForm(), "Dashboard", Properties.Resources.dashboard_icon)));
            buttonsPanel.Controls.Add(CreateSidebarButton("User Management", Properties.Resources.dashboard_icon, (s, e) => LoadForm(new UserManagementForm(), "Dashboard", Properties.Resources.dashboard_icon)));

            buttonsPanel.Padding = new Padding(0);
            buttonsPanel.Dock = DockStyle.Fill;
            sidebar.Controls.Add(buttonsPanel);

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 80,
                BackColor = Color.Black
            };

            Label loggedInLabel = new Label()
            {
                Text = "Logged in as: Admin",
                ForeColor = Color.LightGray,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 25,
                Font = new Font("Segoe UI", 9)
            };

            btnLogout = CreateSidebarButton("Logout", Properties.Resources.logout_icon, (s, e) => this.Close());
            btnLogout.Dock = DockStyle.Top;
            btnLogout.Margin = new Padding(10, 5, 10, 10);

            bottomPanel.Controls.Add(btnLogout);
            bottomPanel.Controls.Add(loggedInLabel);
            sidebar.Controls.Add(bottomPanel);

            //LoadForm(new DashboardForm(), "Dashboard", Properties.Resources.dashboard_icon);
        }

        private Guna2Button CreateSidebarButton(string text, Image icon, EventHandler onClick)
        {
            var button = new Guna2Button
            {
                Text = "  " + text,
                Image = icon,
                ImageSize = new Size(20, 20),
                ImageAlign = HorizontalAlignment.Left,
                TextAlign = HorizontalAlignment.Left,
                Dock = DockStyle.Top,
                Height = 60,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Color.FromArgb(38, 38, 38),
                HoverState = {
                    FillColor = Color.FromArgb(55, 55, 55),
                    ForeColor = Color.Cyan
                },
                BorderRadius = 0,
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            button.Click += onClick;
            return button;
        }

        private void LoadForm(Form form, string title, Image icon)
        {
            containerPanel.Controls.Clear();
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            containerPanel.Controls.Add(form);
            form.Show();

            titleLabel.Text = title;
            titleIcon.Image = icon;
        }
    }
}
