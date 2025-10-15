using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace garage_managemet_frontend
{
    public partial class LoginForm : Form
    {
        private Panel leftPanel, rightPanel;
        private Guna2TextBox txtUsername, txtPassword;
        private Guna2ComboBox cmbRole;
        private Guna2Button btnLogin;
        private static readonly HttpClient httpClient = new HttpClient();

        public LoginForm()
        {
            BuildIN();
        }

        private void BuildIN()
        {
            this.Text = "Garage Management System - Login";
            this.Size = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            int halfWidth = this.ClientSize.Width / 2;

            // Left Panel
            leftPanel = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(halfWidth, this.ClientSize.Height),
                BackColor = Color.White
            };
            this.Controls.Add(leftPanel);

            Label title = new Label()
            {
                Text = "GARAGE",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.Black,
                AutoSize = true,
                Location = new Point(halfWidth / 2 - 70, 50)
            };
            leftPanel.Controls.Add(title);

            Label subtitle = new Label()
            {
                Text = "Sign in",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(halfWidth / 2 - 30, 100),
                AutoSize = true
            };
            leftPanel.Controls.Add(subtitle);

            txtUsername = new Guna2TextBox()
            {
                PlaceholderText = "Enter your username",
                Location = new Point(halfWidth / 2 - 130, 150),
                Size = new Size(260, 40)
            };
            leftPanel.Controls.Add(txtUsername);

            txtPassword = new Guna2TextBox()
            {
                PlaceholderText = "Enter your password",
                PasswordChar = '●',
                Location = new Point(halfWidth / 2 - 130, 200),
                Size = new Size(260, 40)
            };
            leftPanel.Controls.Add(txtPassword);

            cmbRole = new Guna2ComboBox()
            {
                Location = new Point(halfWidth / 2 - 130, 250),
                Size = new Size(260, 40),
                Items = { "Admin", "Mechanic", "Receptionist" },
                SelectedIndex = 0
            };
            leftPanel.Controls.Add(cmbRole);

            btnLogin = new Guna2Button()
            {
                Text = "Login",
                Location = new Point(halfWidth / 2 - 130, 310),
                Size = new Size(260, 45),
                FillColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogin.Click += BtnLogin_Click;
            leftPanel.Controls.Add(btnLogin);

            // Right Panel (image area)
            rightPanel = new Panel()
            {
                Location = new Point(halfWidth, 0),
                Size = new Size(halfWidth, this.ClientSize.Height),
                BackgroundImage = Image.FromFile("D:\\C# Applications\\garage_managemet_frontend\\garage_managemet_frontend\\img\\bg1.jpg"),
                BackgroundImageLayout = ImageLayout.Stretch
            };
            this.Controls.Add(rightPanel);

            Label sloganLine1 = new Label()
            {
                Text = "Software for the automotive industry",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(70, 430)
            };
            rightPanel.Controls.Add(sloganLine1);

            Label sloganLine2 = new Label()
            {
                Text = "By people for people.",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(110, 450)
            };
            rightPanel.Controls.Add(sloganLine2);

            // Handle resizing
            this.Resize += (s, e) =>
            {
                int w = this.ClientSize.Width / 2;
                leftPanel.Size = new Size(w, this.ClientSize.Height);
                rightPanel.Location = new Point(w, 0);
                rightPanel.Size = new Size(w, this.ClientSize.Height);
            };
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text.Trim();

            if (user == "" || pass == "")
            {
                MessageBox.Show("Please enter both username and password.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await LoginAsync();
        }

        private async Task LoginAsync()
        {
            try
            {
                var loginPayload = new
                {
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text.Trim(),
                    UserRole = cmbRole.SelectedItem.ToString()
                };

                var json = JsonSerializer.Serialize(loginPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("http://localhost:5141/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();

                    // Deserialize token response
                    var result = JsonSerializer.Deserialize<LoginResponse>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        MessageBox.Show("✅ Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // ✅ Save token globally
                        GlobalSession.JwtToken = result.Token;

                        // Open main form (like VehicleForm)
                        this.Hide();
                        var vehicleForm = new MainForm();
                        vehicleForm.ShowDialog();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Invalid response from server. No token found.");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"❌ Login failed: {response.StatusCode}\n{error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error logging in: {ex.Message}");
            }
        }

        // Expected JSON format from backend
        public class LoginResponse
        {
            public string Token { get; set; }
        }
    }

    // ✅ Global static class to store token
    public static class GlobalSession
    {
        public static string JwtToken { get; set; }
    }
}
