using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using POSAccountantApp.Models;
using POSAccountantApp.Services;

namespace POSAccountantApp
{
    public partial class POSForm : Form
    {
        private readonly User currentUser;
        private readonly SaleService saleService;
        private readonly InventoryService inventoryService;
        private Sale currentSale;

        private TextBox txtBarcode;
        private DataGridView dgvCart;
        private Label lblSubTotal;
        private Label lblDiscount;
        private Label lblVat;
        private Label lblTotal;
        private Label lblAmountPaid;
        private Label lblChange;
        private TextBox txtDiscount;
        private TextBox txtAmountPaid;
        private ComboBox cboPaymentMethod;
        private Button btnAdd;
        private Button btnRemove;
        private Button btnClear;
        private Button btnCheckout;
        private Button btnCancel;

        public POSForm(User user)
        {
            currentUser = user;
            saleService = new SaleService();
            inventoryService = new InventoryService();
            InitializeComponent();
            InitializeNewSale();
        }

        private void InitializeComponent()
        {
            this.txtBarcode = new TextBox();
            this.dgvCart = new DataGridView();
            this.lblSubTotal = new Label();
            this.lblDiscount = new Label();
            this.lblVat = new Label();
            this.lblTotal = new Label();
            this.lblAmountPaid = new Label();
            this.lblChange = new Label();
            this.txtDiscount = new TextBox();
            this.txtAmountPaid = new TextBox();
            this.cboPaymentMethod = new ComboBox();
            this.btnAdd = new Button();
            this.btnRemove = new Button();
            this.btnClear = new Button();
            this.btnCheckout = new Button();
            this.btnCancel = new Button();

            // Form settings
            this.Text = "Point of Sale";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Barcode TextBox
            this.txtBarcode.Size = new Size(200, 30);
            this.txtBarcode.Location = new Point(10, 10);
            this.txtBarcode.Font = new Font("Segoe UI", 12F);
            this.txtBarcode.PlaceholderText = "Scan barcode...";
            this.txtBarcode.KeyPress += TxtBarcode_KeyPress;

            // Cart DataGridView
            SetupDataGridView();

            // Labels and TextBoxes for totals
            SetupTotalsPanel();

            // Payment Method ComboBox
            this.cboPaymentMethod.Size = new Size(150, 30);
            this.cboPaymentMethod.Location = new Point(820, 520);
            this.cboPaymentMethod.Font = new Font("Segoe UI", 12F);
            this.cboPaymentMethod.Items.AddRange(new object[] { "Cash", "Card" });
            this.cboPaymentMethod.SelectedIndex = 0;

            // Buttons
            SetupButtons();

            // Add controls
            this.Controls.Add(this.txtBarcode);
            this.Controls.Add(this.dgvCart);
            this.Controls.Add(this.lblSubTotal);
            this.Controls.Add(this.lblDiscount);
            this.Controls.Add(this.lblVat);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.lblAmountPaid);
            this.Controls.Add(this.lblChange);
            this.Controls.Add(this.txtDiscount);
            this.Controls.Add(this.txtAmountPaid);
            this.Controls.Add(this.cboPaymentMethod);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnCheckout);
            this.Controls.Add(this.btnCancel);
        }

        private void SetupDataGridView()
        {
            this.dgvCart.Size = new Size(960, 400);
            this.dgvCart.Location = new Point(10, 50);
            this.dgvCart.AllowUserToAddRows = false;
            this.dgvCart.AllowUserToDeleteRows = false;
            this.dgvCart.ReadOnly = true;
            this.dgvCart.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvCart.MultiSelect = false;
            this.dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCart.RowHeadersVisible = false;
            this.dgvCart.BackgroundColor = Color.White;

            // Add columns
            this.dgvCart.Columns.Add("ProductId", "ID");
            this.dgvCart.Columns.Add("Barcode", "Barcode");
            this.dgvCart.Columns.Add("Name", "Product Name");
            this.dgvCart.Columns.Add("Price", "Unit Price");
            this.dgvCart.Columns.Add("Quantity", "Quantity");
            this.dgvCart.Columns.Add("Discount", "Discount");
            this.dgvCart.Columns.Add("Total", "Total");

            // Set column properties
            this.dgvCart.Columns["ProductId"].Visible = false;
            this.dgvCart.Columns["Price"].DefaultCellStyle.Format = "C2";
            this.dgvCart.Columns["Discount"].DefaultCellStyle.Format = "C2";
            this.dgvCart.Columns["Total"].DefaultCellStyle.Format = "C2";
        }

        private void SetupTotalsPanel()
        {
            Font labelFont = new Font("Segoe UI", 12F, FontStyle.Bold);
            int startY = 470;
            int labelX = 820;
            int valueX = 980;

            // SubTotal
            this.lblSubTotal = new Label();
            this.lblSubTotal.Text = "Sub Total: $0.00";
            this.lblSubTotal.Font = labelFont;
            this.lblSubTotal.AutoSize = true;
            this.lblSubTotal.Location = new Point(labelX, startY);

            // Discount
            this.lblDiscount = new Label();
            this.lblDiscount.Text = "Discount:";
            this.lblDiscount.Font = labelFont;
            this.lblDiscount.AutoSize = true;
            this.lblDiscount.Location = new Point(labelX, startY + 40);

            this.txtDiscount = new TextBox();
            this.txtDiscount.Size = new Size(80, 30);
            this.txtDiscount.Location = new Point(valueX, startY + 40);
            this.txtDiscount.Text = "0";
            this.txtDiscount.TextAlign = HorizontalAlignment.Right;
            this.txtDiscount.TextChanged += TxtDiscount_TextChanged;

            // VAT
            this.lblVat = new Label();
            this.lblVat.Text = "VAT (12%): $0.00";
            this.lblVat.Font = labelFont;
            this.lblVat.AutoSize = true;
            this.lblVat.Location = new Point(labelX, startY + 80);

            // Total
            this.lblTotal = new Label();
            this.lblTotal.Text = "Total: $0.00";
            this.lblTotal.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new Point(labelX, startY + 120);

            // Amount Paid
            this.lblAmountPaid = new Label();
            this.lblAmountPaid.Text = "Amount Paid:";
            this.lblAmountPaid.Font = labelFont;
            this.lblAmountPaid.AutoSize = true;
            this.lblAmountPaid.Location = new Point(labelX, startY + 160);

            this.txtAmountPaid = new TextBox();
            this.txtAmountPaid.Size = new Size(150, 30);
            this.txtAmountPaid.Location = new Point(valueX, startY + 160);
            this.txtAmountPaid.TextAlign = HorizontalAlignment.Right;
            this.txtAmountPaid.TextChanged += TxtAmountPaid_TextChanged;

            // Change
            this.lblChange = new Label();
            this.lblChange.Text = "Change: $0.00";
            this.lblChange.Font = labelFont;
            this.lblChange.AutoSize = true;
            this.lblChange.Location = new Point(labelX, startY + 200);
        }

        private void SetupButtons()
        {
            // Common button settings
            Size buttonSize = new Size(120, 40);
            Font buttonFont = new Font("Segoe UI", 10F);

            // Add Button
            this.btnAdd = new Button();
            this.btnAdd.Text = "Add Item";
            this.btnAdd.Size = buttonSize;
            this.btnAdd.Location = new Point(220, 10);
            this.btnAdd.BackColor = Color.FromArgb(40, 167, 69);
            this.btnAdd.ForeColor = Color.White;
            this.btnAdd.FlatStyle = FlatStyle.Flat;
            this.btnAdd.Click += BtnAdd_Click;

            // Remove Button
            this.btnRemove = new Button();
            this.btnRemove.Text = "Remove Item";
            this.btnRemove.Size = buttonSize;
            this.btnRemove.Location = new Point(10, 460);
            this.btnRemove.BackColor = Color.FromArgb(220, 53, 69);
            this.btnRemove.ForeColor = Color.White;
            this.btnRemove.FlatStyle = FlatStyle.Flat;
            this.btnRemove.Click += BtnRemove_Click;

            // Clear Button
            this.btnClear = new Button();
            this.btnClear.Text = "Clear Cart";
            this.btnClear.Size = buttonSize;
            this.btnClear.Location = new Point(140, 460);
            this.btnClear.BackColor = Color.FromArgb(255, 193, 7);
            this.btnClear.ForeColor = Color.Black;
            this.btnClear.FlatStyle = FlatStyle.Flat;
            this.btnClear.Click += BtnClear_Click;

            // Checkout Button
            this.btnCheckout = new Button();
            this.btnCheckout.Text = "Checkout";
            this.btnCheckout.Size = new Size(200, 50);
            this.btnCheckout.Location = new Point(820, 600);
            this.btnCheckout.BackColor = Color.FromArgb(0, 123, 255);
            this.btnCheckout.ForeColor = Color.White;
            this.btnCheckout.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnCheckout.FlatStyle = FlatStyle.Flat;
            this.btnCheckout.Click += BtnCheckout_Click;

            // Cancel Button
            this.btnCancel = new Button();
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Size = buttonSize;
            this.btnCancel.Location = new Point(1030, 600);
            this.btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Click += BtnCancel_Click;
        }

        private void InitializeNewSale()
        {
            currentSale = new Sale
            {
                CashierId = currentUser.UserId,
                CashierName = currentUser.FullName
            };
            dgvCart.Rows.Clear();
            UpdateTotals();
            txtBarcode.Focus();
        }

        private void TxtBarcode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                AddProductByBarcode(txtBarcode.Text.Trim());
                txtBarcode.Clear();
            }
        }

        private void AddProductByBarcode(string barcode)
        {
            try
            {
                var product = inventoryService.GetProductByBarcode(barcode);
                if (product == null)
                {
                    MessageBox.Show("Product not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (product.Stock <= 0)
                {
                    MessageBox.Show("Product is out of stock.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if product already in cart
                foreach (DataGridViewRow row in dgvCart.Rows)
                {
                    if (row.Cells["ProductId"].Value.ToString() == product.ProductId.ToString())
                    {
                        int currentQty = Convert.ToInt32(row.Cells["Quantity"].Value);
                        if (currentQty >= product.Stock)
                        {
                            MessageBox.Show("Not enough stock available.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        row.Cells["Quantity"].Value = currentQty + 1;
                        row.Cells["Total"].Value = (currentQty + 1) * Convert.ToDecimal(row.Cells["Price"].Value);
                        UpdateTotals();
                        return;
                    }
                }

                // Add new product to cart
                dgvCart.Rows.Add(
                    product.ProductId,
                    product.Barcode,
                    product.Name,
                    product.Price,
                    1, // Quantity
                    0, // Discount
                    product.Price // Total
                );

                UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // TODO: Implement manual product search and add
            MessageBox.Show("Manual product search coming soon...");
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to remove.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvCart.Rows.RemoveAt(dgvCart.SelectedRows[0].Index);
            UpdateTotals();
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to clear the cart?", "Confirm Clear",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dgvCart.Rows.Clear();
                UpdateTotals();
            }
        }

        private void TxtDiscount_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtDiscount.Text, out decimal discount))
            {
                currentSale.DiscountPercentage = discount;
                UpdateTotals();
            }
        }

        private void TxtAmountPaid_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtAmountPaid.Text, out decimal amountPaid))
            {
                currentSale.AmountPaid = amountPaid;
                currentSale.Change = amountPaid - currentSale.TotalAmount;
                lblChange.Text = $"Change: {currentSale.Change:C2}";
            }
        }

        private void UpdateTotals()
        {
            currentSale.Items.Clear();
            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                var item = new SaleItem
                {
                    ProductId = Convert.ToInt32(row.Cells["ProductId"].Value),
                    ProductName = row.Cells["Name"].Value.ToString(),
                    ProductBarcode = row.Cells["Barcode"].Value.ToString(),
                    UnitPrice = Convert.ToDecimal(row.Cells["Price"].Value),
                    Quantity = Convert.ToInt32(row.Cells["Quantity"].Value),
                    Discount = Convert.ToDecimal(row.Cells["Discount"].Value)
                };
                item.CalculateTotal();
                currentSale.Items.Add(item);
            }

            currentSale.CalculateTotals();

            lblSubTotal.Text = $"Sub Total: {currentSale.SubTotal:C2}";
            lblDiscount.Text = $"Discount ({currentSale.DiscountPercentage}%): {currentSale.DiscountAmount:C2}";
            lblVat.Text = $"VAT (12%): {currentSale.VatAmount:C2}";
            lblTotal.Text = $"Total: {currentSale.TotalAmount:C2}";

            if (decimal.TryParse(txtAmountPaid.Text, out decimal amountPaid))
            {
                currentSale.Change = amountPaid - currentSale.TotalAmount;
                lblChange.Text = $"Change: {currentSale.Change:C2}";
            }
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentSale.Items.Count == 0)
                {
                    MessageBox.Show("Please add items to cart before checkout.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(txtAmountPaid.Text) || currentSale.AmountPaid < currentSale.TotalAmount)
                {
                    MessageBox.Show("Please enter valid amount paid.", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                currentSale.PaymentMethod = cboPaymentMethod.SelectedItem.ToString();

                int saleId = saleService.ProcessSale(currentSale);
                MessageBox.Show($"Sale #{saleId} completed successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // TODO: Print receipt
                // PrintHelper.PrintReceipt(currentSale);

                InitializeNewSale();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing sale: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (dgvCart.Rows.Count > 0)
            {
                if (MessageBox.Show("Are you sure you want to cancel this sale?", "Confirm Cancel",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }

            this.Close();
        }
    }
}
