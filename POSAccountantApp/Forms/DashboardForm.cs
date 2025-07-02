using System;
using System.Drawing;
using System.Windows.Forms;
using POSAccountantApp.Models;

namespace POSAccountantApp
{
    public partial class DashboardForm : Form
    {
        private User currentUser;
        private Panel sideMenu;
        private Panel mainContent;
        private Label lblWelcome;
        private Button btnPOS;
        private Button btnInventory;
        private Button btnAccounting;
        private Button btnReports;
        private Button btnUsers;
        private Button btnLogout;

        public DashboardForm(User user)
        {
            currentUser = user;
            InitializeComponent();
            SetupDashboard();
        }

        private void InitializeComponent()
        {
            this.sideMenu = new Panel();
            this.mainContent = new Panel();
            this.lblWelcome = new Label();
            this.btnPOS = new Button();
            this.btnInventory = new Button();
            this.btnAccounting = new Button();
            this.btnReports = new Button();
            this.btnUsers = new Button();
            this.btnLogout = new Button();

            // Form settings
            this.Text = "POS + Accounting System Dashboard";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Side menu panel
            this.sideMenu.Size = new Size(250, this.Height);
            this.sideMenu.BackColor = Color.FromArgb(52, 58, 64);
            this.sideMenu.Dock = DockStyle.Left;

            // Main content panel
            this.mainContent.Dock = DockStyle.Fill;
            this.mainContent.BackColor = Color.FromArgb(248, 249, 250);

            // Welcome label
            this.lblWelcome.Text = $"Welcome, {currentUser.FullName}";
            this.lblWelcome.ForeColor = Color.White;
            this.lblWelcome.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblWelcome.Size = new Size(230, 40);
            this.lblWelcome.Location = new Point(10, 20);
            this.lblWelcome.TextAlign = ContentAlignment.MiddleCenter;

            // Setup menu buttons
            SetupMenuButtons();

            // Add controls
            this.sideMenu.Controls.Add(this.lblWelcome);
            this.Controls.Add(this.sideMenu);
            this.Controls.Add(this.mainContent);
        }

        private void SetupMenuButtons()
        {
            // Common button settings
            Size buttonSize = new Size(230, 40);
            Color buttonColor = Color.FromArgb(73, 80, 87);
            Font buttonFont = new Font("Segoe UI", 12F);
            int startY = 100;
            int padding = 10;

            // POS Button
            SetupButton(btnPOS, "Point of Sale", buttonSize, buttonColor, buttonFont, 
                new Point(padding, startY), BtnPOS_Click);
            startY += 50;

            // Inventory Button
            SetupButton(btnInventory, "Inventory", buttonSize, buttonColor, buttonFont, 
                new Point(padding, startY), BtnInventory_Click);
            startY += 50;

            // Accounting Button
            SetupButton(btnAccounting, "Accounting", buttonSize, buttonColor, buttonFont, 
                new Point(padding, startY), BtnAccounting_Click);
            startY += 50;

            // Reports Button
            SetupButton(btnReports, "Reports", buttonSize, buttonColor, buttonFont, 
                new Point(padding, startY), BtnReports_Click);
            startY += 50;

            // Users Button (Admin only)
            if (currentUser.Role == UserRole.Admin)
            {
                SetupButton(btnUsers, "User Management", buttonSize, buttonColor, buttonFont, 
                    new Point(padding, startY), BtnUsers_Click);
                startY += 50;
            }

            // Logout Button
            SetupButton(btnLogout, "Logout", buttonSize, Color.FromArgb(220, 53, 69), buttonFont, 
                new Point(padding, startY), BtnLogout_Click);

            // Show/hide buttons based on user role
            UpdateButtonVisibility();
        }

        private void SetupButton(Button button, string text, Size size, Color color, Font font, Point location, EventHandler clickHandler)
        {
            button.Text = text;
            button.Size = size;
            button.Location = location;
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = font;
            button.Click += clickHandler;
            this.sideMenu.Controls.Add(button);
        }

        private void UpdateButtonVisibility()
        {
            switch (currentUser.Role)
            {
                case UserRole.Admin:
                    // Admin has access to everything
                    break;
                case UserRole.Cashier:
                    btnInventory.Visible = false;
                    btnAccounting.Visible = false;
                    btnUsers.Visible = false;
                    break;
                case UserRole.Accountant:
                    btnPOS.Visible = false;
                    btnUsers.Visible = false;
                    break;
            }
        }

        private void SetupDashboard()
        {
            // Additional dashboard setup if needed
        }

        private void BtnPOS_Click(object sender, EventArgs e)
        {
            // TODO: Open POS Form
            MessageBox.Show("Opening POS...");
        }

        private void BtnInventory_Click(object sender, EventArgs e)
        {
            // TODO: Open Inventory Form
            MessageBox.Show("Opening Inventory...");
        }

        private void BtnAccounting_Click(object sender, EventArgs e)
        {
            // TODO: Open Accounting Form
            MessageBox.Show("Opening Accounting...");
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            // TODO: Open Reports Form
            MessageBox.Show("Opening Reports...");
        }

        private void BtnUsers_Click(object sender, EventArgs e)
        {
            // TODO: Open User Management Form
            MessageBox.Show("Opening User Management...");
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to logout?", "Logout",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
                new LoginForm().Show();
            }
        }
    }
}
