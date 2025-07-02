using System;
using System.Drawing;
using System.Windows.Forms;
using POSAccountantApp.Services;

namespace POSAccountantApp
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label lblTitle;

        public LoginForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void InitializeComponent()
        {
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.btnLogin = new Button();
            this.lblTitle = new Label();

            // Form settings
            this.Text = "POS + Accounting System Login";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title Label
            this.lblTitle.Text = "POS + Accounting System";
            this.lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            this.lblTitle.Size = new Size(350, 40);
            this.lblTitle.Location = new Point(25, 50);
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // Username TextBox
            this.txtUsername.Size = new Size(300, 30);
            this.txtUsername.Location = new Point(50, 150);
            this.txtUsername.PlaceholderText = "Username";
            this.txtUsername.Font = new Font("Segoe UI", 12F);

            // Password TextBox
            this.txtPassword.Size = new Size(300, 30);
            this.txtPassword.Location = new Point(50, 200);
            this.txtPassword.PlaceholderText = "Password";
            this.txtPassword.PasswordChar = '•';
            this.txtPassword.Font = new Font("Segoe UI", 12F);

            // Login Button
            this.btnLogin.Text = "Login";
            this.btnLogin.Size = new Size(300, 40);
            this.btnLogin.Location = new Point(50, 270);
            this.btnLogin.BackColor = Color.FromArgb(0, 122, 204);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.Font = new Font("Segoe UI", 12F);
            this.btnLogin.Click += new EventHandler(BtnLogin_Click);

            // Add controls to form
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnLogin);
        }

        private void SetupForm()
        {
            // Additional form setup if needed
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // TODO: Implement authentication service
                // var authService = new AuthService();
                // var user = authService.Login(username, password);
                
                // Temporary demo login
                if (username == "admin" && password == "admin")
                {
                    MessageBox.Show("Login successful!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // TODO: Open dashboard form
                    // var dashboard = new DashboardForm(user);
                    // this.Hide();
                    // dashboard.ShowDialog();
                    // this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
