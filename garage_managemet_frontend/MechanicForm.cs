using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace garage_managemet_frontend
{
    public partial class MechanicForm : Form
    {
        private Guna2DataGridView dgvMechanics;
        private Guna2Button btnAdd, btnEdit, btnDelete;

        public MechanicForm()
        {
            this.Text = "Mechanic Management";
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            Label title = new Label()
            {
                Text = "🔧 Mechanics",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(title);

            dgvMechanics = new Guna2DataGridView()
            {
                Location = new Point(20, 70),
                Size = new Size(950, 400),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvMechanics.Columns.Add("Id", "ID");
            dgvMechanics.Columns.Add("Name", "Name");
            dgvMechanics.Columns.Add("Specialty", "Specialty");
            dgvMechanics.Columns.Add("Phone", "Phone");
            dgvMechanics.Rows.Add("M01", "Kamal", "Engine Specialist", "0711234567");
            dgvMechanics.Rows.Add("M02", "Ruwan", "Brake Specialist", "0723456789");
            this.Controls.Add(dgvMechanics);

            btnAdd = CreateButton("Add", new Point(20, 490), Color.FromArgb(0, 120, 212));
            btnEdit = CreateButton("Edit", new Point(160, 490), Color.DarkOrange);
            btnDelete = CreateButton("Delete", new Point(300, 490), Color.IndianRed);

            this.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });
        }

        private Guna2Button CreateButton(string text, Point location, Color color)
        {
            return new Guna2Button()
            {
                Text = text,
                Location = location,
                Size = new Size(120, 40),
                FillColor = color,
                ForeColor = Color.White,
                BorderRadius = 6
            };
        }
    }

}    
