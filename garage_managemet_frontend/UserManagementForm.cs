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
    public partial class UserManagementForm : Form
    {
        private Guna2Panel header;
        private Guna2TextBox txtSearch;
        private Guna2Button btnAdd, btnUpdate, btnDelete;
        private Guna2DataGridView dgvUsers;

        private readonly HttpClient httpClient = new HttpClient();
        private const string API_URL = "http://localhost:5141/api/usermanagement";
        private readonly string TOKEN;
        private readonly string ROLE;

        public UserManagementForm()
        {
            

            
           
                
                
            

            this.Text = "User Management (Admin Only)";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            BuildUI();
            _ = LoadUsersAsync();
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
                Text = "👑 User Management (Admin Only)",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.Black
            };
            header.Controls.Add(lblTitle);

            txtSearch = new Guna2TextBox()
            {
                PlaceholderText = "Search User ID or Username...",
                Location = new Point(this.Width - 750, 20),
                Size = new Size(250, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 8,
                BorderColor = Color.LightGray
            };
            header.Controls.Add(txtSearch);

            var btnSearch = CreateButton("Search", Color.Gray, new Point(this.Width - 480, 20));
            btnSearch.Click += async (s, e) => await SearchUserAsync(txtSearch.Text);
            header.Controls.Add(btnSearch);

            btnAdd = CreateButton("Add", Color.FromArgb(0, 120, 215), new Point(this.Width - 360, 20));
            btnUpdate = CreateButton("Update", Color.Orange, new Point(this.Width - 240, 20));
            btnDelete = CreateButton("Delete", Color.IndianRed, new Point(this.Width - 120, 20));

            btnAdd.Click += async (s, e) => await ShowAddOrUpdateDialogAsync();
            btnUpdate.Click += async (s, e) => await ShowAddOrUpdateDialogAsync(GetSelectedUser());
            btnDelete.Click += async (s, e) => await DeleteSelectedUserAsync();

            header.Controls.Add(btnAdd);
            header.Controls.Add(btnUpdate);
            header.Controls.Add(btnDelete);

            dgvUsers = new Guna2DataGridView()
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

            dgvUsers.ThemeStyle.HeaderStyle.BackColor = Color.Black;
            dgvUsers.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvUsers.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvUsers.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvUsers.ThemeStyle.RowsStyle.ForeColor = Color.Black;
            dgvUsers.ThemeStyle.RowsStyle.SelectionBackColor = Color.LightGray;

            dgvUsers.Columns.Add("Id", "User ID");
            dgvUsers.Columns.Add("UserName", "Username");
            dgvUsers.Columns.Add("Role", "Role");

            this.Controls.Add(dgvUsers);
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

        // ---------------- LOAD USERS ----------------
        private async Task LoadUsersAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string json = await httpClient.GetStringAsync(API_URL);

                var users = JsonSerializer.Deserialize<User[]>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                dgvUsers.Rows.Clear();
                foreach (var u in users)
                {
                    if (u.Is_Delete == 0)
                        dgvUsers.Rows.Add(u.Id, u.UserName, u.Role);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}");
            }
        }

        // ---------------- SEARCH USER ----------------
        private async Task SearchUserAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadUsersAsync();
                return;
            }

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string json = await httpClient.GetStringAsync($"{API_URL}/{query}");

                var user = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                dgvUsers.Rows.Clear();
                if (user != null && user.Is_Delete == 0)
                {
                    dgvUsers.Rows.Add(user.Id, user.UserName, user.Role);
                }
                else
                {
                    MessageBox.Show("User not found or deleted.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching user: {ex.Message}");
            }
        }

        // ---------------- ADD / UPDATE DIALOG ----------------
        private async Task ShowAddOrUpdateDialogAsync(User existing = null)
        {
            bool isUpdate = existing != null;

            var form = new Form()
            {
                Text = isUpdate ? "Update User" : "Add New User",
                Size = new Size(400, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            int y = 30;
            Guna2TextBox CreateInput(string placeholder, string value = "", bool isPassword = false)
            {
                var txt = new Guna2TextBox()
                {
                    PlaceholderText = placeholder,
                    Text = value,
                    Location = new Point(40, y),
                    Size = new Size(300, 40),
                    PasswordChar = isPassword ? '●' : '\0'
                };
                y += 50;
                form.Controls.Add(txt);
                return txt;
            }

            var txtUserName = CreateInput("Username", existing?.UserName);
            var txtPassword = CreateInput("Password", "", true);
            var txtRole = CreateInput("Role (Admin / Mechanic / Receptionist)", existing?.Role);

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
                var user = new User
                {
                    Id = existing?.Id ?? 0,
                    UserName = txtUserName.Text,
                    PasswordHash = txtPassword.Text, // backend hashes it
                    Role = txtRole.Text
                };

                if (isUpdate)
                    await UpdateUserAsync(user);
                else
                    await AddUserAsync(user);

                form.Close();
                await LoadUsersAsync();
            };

            form.ShowDialog();
        }

        // ---------------- ADD USER ----------------
        private async Task AddUserAsync(User user)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                    MessageBox.Show("✅ User added successfully!");
                else
                    MessageBox.Show($"❌ Failed to add user: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding user: {ex.Message}");
            }
        }

        // ---------------- UPDATE USER ----------------
        private async Task UpdateUserAsync(User user)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                var json = JsonSerializer.Serialize(user);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"{API_URL}/{user.Id}", content);

                if (response.IsSuccessStatusCode)
                    MessageBox.Show("✅ User updated successfully!");
                else
                    MessageBox.Show($"❌ Failed to update user: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}");
            }
        }

        // ---------------- DELETE USER ----------------
        private async Task DeleteSelectedUserAsync()
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to delete.");
                return;
            }

            var id = dgvUsers.SelectedRows[0].Cells["Id"].Value.ToString();
            if (MessageBox.Show($"Are you sure you want to delete user #{id}?",
                                "Confirm Delete",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning) == DialogResult.No)
                return;

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                var response = await httpClient.DeleteAsync($"{API_URL}/{id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ User deleted successfully!");
                    await LoadUsersAsync();
                }
                else
                {
                    MessageBox.Show($"❌ Failed to delete user: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting user: {ex.Message}");
            }
        }

        // ---------------- MODEL ----------------
        public class User
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string PasswordHash { get; set; }
            public string Role { get; set; }
            public int Is_Delete { get; set; }
        }

        private User GetSelectedUser()
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a user first.");
                return null;
            }

            var r = dgvUsers.SelectedRows[0];
            return new User
            {
                Id = Convert.ToInt32(r.Cells["Id"].Value),
                UserName = r.Cells["UserName"].Value.ToString(),
                Role = r.Cells["Role"].Value.ToString()
            };
        }
    }
}
