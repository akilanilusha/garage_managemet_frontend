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
    public partial class CustomerForm : Form
    {
        private Guna2Panel header;
        private Guna2TextBox txtSearch;
        private Guna2Button btnAdd, btnUpdate, btnDelete;
        private Guna2DataGridView dgvCustomers;

        private readonly HttpClient httpClient = new HttpClient();
        private const string API_URL = "http://localhost:5141/api/Customer";
        private const string TOKEN = "<YOUR_JWT_TOKEN>"; // 🔑 Replace dynamically after login

        public CustomerForm()
        {
            this.Text = "Customer Management";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.White;

            BuildUI();
            _ = LoadCustomersAsync();
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
                Text = "👤 Customer Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = Color.Black
            };
            header.Controls.Add(lblTitle);

            txtSearch = new Guna2TextBox()
            {
                PlaceholderText = "Search Customer ID or Name...",
                Location = new Point(this.Width - 650, 20),
                Size = new Size(250, 35),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BorderRadius = 8,
                BorderColor = Color.LightGray
            };
            header.Controls.Add(txtSearch);

            var btnSearch = CreateButton("Search", Color.Gray, new Point(this.Width - 390, 20));
            btnSearch.Click += async (s, e) => await SearchCustomerAsync(txtSearch.Text);
            header.Controls.Add(btnSearch);

            btnAdd = CreateButton("Add", Color.FromArgb(0, 120, 215), new Point(this.Width - 280, 20));
            btnUpdate = CreateButton("Update", Color.Orange, new Point(this.Width - 170, 20));
            btnDelete = CreateButton("Delete", Color.IndianRed, new Point(this.Width - 60, 20));

            btnAdd.Click += async (s, e) => await ShowAddOrUpdateDialogAsync();
            btnUpdate.Click += async (s, e) => await ShowAddOrUpdateDialogAsync(GetSelectedCustomer());
            btnDelete.Click += async (s, e) => await DeleteSelectedCustomerAsync();

            header.Controls.Add(btnAdd);
            header.Controls.Add(btnUpdate);
            header.Controls.Add(btnDelete);

            // ✅ Styled DataGridView
            dgvCustomers = new Guna2DataGridView()
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

            dgvCustomers.ThemeStyle.HeaderStyle.BackColor = Color.Black;
            dgvCustomers.ThemeStyle.HeaderStyle.ForeColor = Color.White;
            dgvCustomers.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvCustomers.ThemeStyle.RowsStyle.BackColor = Color.White;
            dgvCustomers.ThemeStyle.RowsStyle.ForeColor = Color.Black;
            dgvCustomers.ThemeStyle.RowsStyle.SelectionBackColor = Color.LightGray;

            dgvCustomers.Columns.Add("CustomerID", "Customer ID");
            dgvCustomers.Columns.Add("Name", "Name");
            dgvCustomers.Columns.Add("Phone", "Phone");
            dgvCustomers.Columns.Add("Email", "Email");

            this.Controls.Add(dgvCustomers);

            this.Resize += (s, e) =>
            {
                txtSearch.Location = new Point(this.Width - 650, 20);
                btnAdd.Location = new Point(this.Width - 280, 20);
                btnUpdate.Location = new Point(this.Width - 170, 20);
                btnDelete.Location = new Point(this.Width - 60, 20);
                dgvCustomers.Size = new Size(this.ClientSize.Width - 60, this.ClientSize.Height - 110);
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
        private async Task LoadCustomersAsync()
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string json = await httpClient.GetStringAsync(API_URL);

                var customers = JsonSerializer.Deserialize<Customer[]>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                dgvCustomers.Rows.Clear();
                foreach (var c in customers)
                {
                    dgvCustomers.Rows.Add(c.CustomerID, c.Name, c.Phone, c.Email);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}");
            }
        }

        // ---------------- SEARCH ----------------
        private async Task SearchCustomerAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadCustomersAsync();
                return;
            }

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                string url = $"{API_URL}/{query}";
                string json = await httpClient.GetStringAsync(url);

                var customer = JsonSerializer.Deserialize<Customer>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                dgvCustomers.Rows.Clear();
                if (customer != null)
                {
                    dgvCustomers.Rows.Add(customer.CustomerID, customer.Name, customer.Phone, customer.Email);
                }
                else
                {
                    MessageBox.Show("Customer not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching customer: {ex.Message}");
            }
        }

        // ---------------- ADD / UPDATE ----------------
        private async Task ShowAddOrUpdateDialogAsync(Customer existing = null)
        {
            bool isUpdate = existing != null;

            var form = new Form()
            {
                Text = isUpdate ? "Update Customer" : "Add Customer",
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

            var txtName = CreateInput("Name", existing?.Name);
            var txtPhone = CreateInput("Phone", existing?.Phone);
            var txtEmail = CreateInput("Email", existing?.Email);

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
                var customer = new Customer
                {
                    CustomerID = existing?.CustomerID ?? 0,
                    Name = txtName.Text,
                    Phone = txtPhone.Text,
                    Email = txtEmail.Text
                };

                if (isUpdate)
                    await UpdateCustomerAsync(customer);
                else
                    await AddCustomerAsync(customer);

                form.Close();
                await LoadCustomersAsync();
            };

            form.ShowDialog();
        }

        // ---------------- ADD CUSTOMER ----------------
        private async Task AddCustomerAsync(Customer customer)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                var json = JsonSerializer.Serialize(customer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(API_URL, content);

                if (response.IsSuccessStatusCode)
                    MessageBox.Show("✅ Customer added successfully!");
                else
                    MessageBox.Show($"❌ Failed to add customer: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding customer: {ex.Message}");
            }
        }

        // ---------------- UPDATE CUSTOMER ----------------
        private async Task UpdateCustomerAsync(Customer customer)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);
                var json = JsonSerializer.Serialize(customer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"{API_URL}/{customer.CustomerID}", content);

                if (response.IsSuccessStatusCode)
                    MessageBox.Show("✅ Customer updated successfully!");
                else
                    MessageBox.Show($"❌ Failed to update customer: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating customer: {ex.Message}");
            }
        }

        // ---------------- DELETE CUSTOMER ----------------
        private async Task DeleteSelectedCustomerAsync()
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a customer to delete.");
                return;
            }

            var id = dgvCustomers.SelectedRows[0].Cells["CustomerID"].Value.ToString();
            if (MessageBox.Show($"Are you sure you want to delete customer #{id}?",
                                "Confirm Delete",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning) == DialogResult.No)
                return;

            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GlobalSession.JwtToken);

                var body = new { is_delete = 1 };
                var json = JsonSerializer.Serialize(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{API_URL}/{id}/delete")
                {
                    Content = content
                };

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("✅ Customer deleted successfully!");
                    await LoadCustomersAsync();
                }
                else
                {
                    MessageBox.Show($"❌ Failed to delete customer: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting customer: {ex.Message}");
            }
        }

        // ---------------- MODEL ----------------
        public class Customer
        {
            public int CustomerID { get; set; }
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public int Is_Delete { get; set; }
        }

        private Customer GetSelectedCustomer()
        {
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a customer first.");
                return null;
            }

            var r = dgvCustomers.SelectedRows[0];
            return new Customer
            {
                CustomerID = Convert.ToInt32(r.Cells["CustomerID"].Value),
                Name = r.Cells["Name"].Value.ToString(),
                Phone = r.Cells["Phone"].Value.ToString(),
                Email = r.Cells["Email"].Value.ToString()
            };
        }
    }
}
