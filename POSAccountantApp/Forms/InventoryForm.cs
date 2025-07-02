using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using POSAccountantApp.Models;
using POSAccountantApp.Services;

namespace POSAccountantApp
{
    public partial class InventoryForm : Form
    {
        private readonly InventoryService inventoryService;
        private readonly User currentUser;
        private DataGridView dgvProducts;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnStockIn;
        private Button btnStockOut;
        private TextBox txtSearch;
        private CheckBox chkShowInactive;

        public InventoryForm(User user)
        {
            currentUser = user;
            inventoryService = new InventoryService();
            InitializeComponent();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.dgvProducts = new DataGridView();
            this.btnAdd = new Button();
            this.btnEdit = new Button();
            this.btnDelete = new Button();
            this.btnRefresh = new Button();
            this.btnStockIn = new Button();
            this.btnStockOut = new Button();
            this.txtSearch = new TextBox();
            this.chkShowInactive = new CheckBox();

            // Form settings
            this.Text = "Inventory Management";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Search TextBox
            this.txtSearch.Size = new Size(200, 23);
            this.txtSearch.Location = new Point(10, 10);
            this.txtSearch.PlaceholderText = "Search products...";
            this.txtSearch.TextChanged += TxtSearch_TextChanged;

            // Show Inactive CheckBox
            this.chkShowInactive.Text = "Show Inactive Products";
            this.chkShowInactive.Location = new Point(220, 10);
            this.chkShowInactive.AutoSize = true;
            this.chkShowInactive.CheckedChanged += ChkShowInactive_CheckedChanged;

            // Buttons
            SetupButtons();

            // DataGridView
            SetupDataGridView();

            // Add controls
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.chkShowInactive);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnStockIn);
            this.Controls.Add(this.btnStockOut);
            this.Controls.Add(this.dgvProducts);
        }

        private void SetupButtons()
        {
            // Common button settings
            Size buttonSize = new Size(100, 30);
            Font buttonFont = new Font("Segoe UI", 9F);

            // Add Button
            this.btnAdd.Text = "Add New";
            this.btnAdd.Size = buttonSize;
            this.btnAdd.Location = new Point(10, 40);
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Click += BtnAdd_Click;

            // Edit Button
            this.btnEdit.Text = "Edit";
            this.btnEdit.Size = buttonSize;
            this.btnEdit.Location = new Point(120, 40);
            this.btnEdit.BackColor = Color.FromArgb(0, 123, 255);
            this.btnEdit.ForeColor = Color.White;
            this.btnEdit.FlatStyle = FlatStyle.Flat;
            this.btnEdit.Click += BtnEdit_Click;

            // Delete Button
            this.btnDelete.Text = "Delete";
            this.btnDelete.Size = buttonSize;
            this.btnDelete.Location = new Point(230, 40);
            this.btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.FlatStyle = FlatStyle.Flat;
            this.btnDelete.Click += BtnDelete_Click;

            // Refresh Button
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Size = buttonSize;
            this.btnRefresh.Location = new Point(340, 40);
            this.btnRefresh.BackColor = Color.FromArgb(108, 117, 125);
            this.btnRefresh.ForeColor = Color.White;
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.Click += BtnRefresh_Click;

            // Stock In Button
            this.btnStockIn.Text = "Stock In";
            this.btnStockIn.Size = buttonSize;
            this.btnStockIn.Location = new Point(450, 40);
            this.btnStockIn.BackColor = Color.FromArgb(23, 162, 184);
            this.btnStockIn.ForeColor = Color.White;
            this.btnStockIn.FlatStyle = FlatStyle.Flat;
            this.btnStockIn.Click += BtnStockIn_Click;

            // Stock Out Button
            this.btnStockOut.Text = "Stock Out";
            this.btnStockOut.Size = buttonSize;
            this.btnStockOut.Location = new Point(560, 40);
            this.btnStockOut.BackColor = Color.FromArgb(255, 193, 7);
            this.btnStockOut.ForeColor = Color.Black;
            this.btnStockOut.FlatStyle = FlatStyle.Flat;
            this.btnStockOut.Click += BtnStockOut_Click;
        }

        private void SetupDataGridView()
        {
            this.dgvProducts.Size = new Size(960, 480);
            this.dgvProducts.Location = new Point(10, 80);
            this.dgvProducts.AllowUserToAddRows = false;
            this.dgvProducts.AllowUserToDeleteRows = false;
            this.dgvProducts.ReadOnly = true;
            this.dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvProducts.MultiSelect = false;
            this.dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProducts.RowHeadersVisible = false;
            this.dgvProducts.BackgroundColor = Color.White;

            // Add columns
            this.dgvProducts.Columns.Add("ProductId", "ID");
            this.dgvProducts.Columns.Add("Barcode", "Barcode");
            this.dgvProducts.Columns.Add("Name", "Name");
            this.dgvProducts.Columns.Add("Price", "Price");
            this.dgvProducts.Columns.Add("Stock", "Stock");
            this.dgvProducts.Columns.Add("ReorderLevel", "Reorder Level");
            this.dgvProducts.Columns.Add("Category", "Category");
            this.dgvProducts.Columns.Add("IsActive", "Active");

            // Set column properties
            this.dgvProducts.Columns["ProductId"].Visible = false;
            this.dgvProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
        }

        private void LoadProducts()
        {
            try
            {
                dgvProducts.Rows.Clear();
                List<Product> products = inventoryService.GetAllProducts(!chkShowInactive.Checked);

                foreach (var product in products)
                {
                    if (string.IsNullOrEmpty(txtSearch.Text) || 
                        product.Name.ToLower().Contains(txtSearch.Text.ToLower()) ||
                        product.Barcode.ToLower().Contains(txtSearch.Text.ToLower()))
                    {
                        dgvProducts.Rows.Add(
                            product.ProductId,
                            product.Barcode,
                            product.Name,
                            product.Price,
                            product.Stock,
                            product.ReorderLevel,
                            product.Category,
                            product.IsActive
                        );

                        // Highlight low stock
                        if (product.IsLowStock())
                        {
                            dgvProducts.Rows[dgvProducts.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightPink;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void ChkShowInactive_CheckedChanged(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // TODO: Implement Add Product Form
            MessageBox.Show("Add Product functionality coming soon...");
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to edit.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // TODO: Implement Edit Product Form
            MessageBox.Show("Edit Product functionality coming soon...");
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to delete.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // TODO: Implement Delete Product
                MessageBox.Show("Delete Product functionality coming soon...");
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }

        private void BtnStockIn_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product for stock in.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // TODO: Implement Stock In Form
            MessageBox.Show("Stock In functionality coming soon...");
        }

        private void BtnStockOut_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product for stock out.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // TODO: Implement Stock Out Form
            MessageBox.Show("Stock Out functionality coming soon...");
        }
    }
}
