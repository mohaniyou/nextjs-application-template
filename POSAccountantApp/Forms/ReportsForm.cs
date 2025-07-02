using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using POSAccountantApp.Models;
using POSAccountantApp.Services;
using POSAccountantApp.Utilities;

namespace POSAccountantApp
{
    public partial class ReportsForm : Form
    {
        private readonly ReportService reportService;
        private readonly User currentUser;

        private TabControl tabReports;
        private TabPage tabSales;
        private TabPage tabInventory;
        private TabPage tabProfit;

        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private Button btnGenerateReport;
        private Button btnPrint;
        private DataGridView dgvReport;
        private ComboBox cboReportType;

        public ReportsForm(User user)
        {
            currentUser = user;
            reportService = new ReportService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.tabReports = new TabControl();
            this.tabSales = new TabPage();
            this.tabInventory = new TabPage();
            this.tabProfit = new TabPage();
            this.dtpStartDate = new DateTimePicker();
            this.dtpEndDate = new DateTimePicker();
            this.btnGenerateReport = new Button();
            this.btnPrint = new Button();
            this.dgvReport = new DataGridView();
            this.cboReportType = new ComboBox();

            // Form settings
            this.Text = "Reports";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Tab Control
            this.tabReports.Dock = DockStyle.Fill;
            this.tabReports.Controls.Add(tabSales);
            this.tabReports.Controls.Add(tabInventory);
            this.tabReports.Controls.Add(tabProfit);

            // Sales Tab
            this.tabSales.Text = "Sales Reports";
            SetupSalesTab();

            // Inventory Tab
            this.tabInventory.Text = "Inventory Reports";
            SetupInventoryTab();

            // Profit Tab
            this.tabProfit.Text = "Profit Reports";
            SetupProfitTab();

            // Add controls
            this.Controls.Add(this.tabReports);
        }

        private void SetupSalesTab()
        {
            // Report Type ComboBox
            this.cboReportType = new ComboBox();
            this.cboReportType.Items.AddRange(new string[] {
                "Daily Sales",
                "Date Range Sales",
                "Top Selling Products"
            });
            this.cboReportType.Location = new Point(10, 10);
            this.cboReportType.Size = new Size(200, 25);
            this.cboReportType.SelectedIndex = 0;
            this.cboReportType.SelectedIndexChanged += CboReportType_SelectedIndexChanged;

            // Date Pickers
            this.dtpStartDate = new DateTimePicker();
            this.dtpStartDate.Location = new Point(220, 10);
            this.dtpStartDate.Size = new Size(200, 25);
            this.dtpStartDate.Format = DateTimePickerFormat.Short;

            this.dtpEndDate = new DateTimePicker();
            this.dtpEndDate.Location = new Point(430, 10);
            this.dtpEndDate.Size = new Size(200, 25);
            this.dtpEndDate.Format = DateTimePickerFormat.Short;
            this.dtpEndDate.Visible = false;

            // Generate Button
            this.btnGenerateReport = new Button();
            this.btnGenerateReport.Text = "Generate Report";
            this.btnGenerateReport.Location = new Point(640, 10);
            this.btnGenerateReport.Size = new Size(120, 25);
            this.btnGenerateReport.Click += BtnGenerateReport_Click;

            // Print Button
            this.btnPrint = new Button();
            this.btnPrint.Text = "Print";
            this.btnPrint.Location = new Point(770, 10);
            this.btnPrint.Size = new Size(100, 25);
            this.btnPrint.Click += BtnPrint_Click;

            // DataGridView
            this.dgvReport = new DataGridView();
            this.dgvReport.Location = new Point(10, 45);
            this.dgvReport.Size = new Size(1160, 680);
            this.dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvReport.AllowUserToAddRows = false;
            this.dgvReport.ReadOnly = true;
            this.dgvReport.BackgroundColor = Color.White;

            // Add controls to Sales Tab
            this.tabSales.Controls.Add(this.cboReportType);
            this.tabSales.Controls.Add(this.dtpStartDate);
            this.tabSales.Controls.Add(this.dtpEndDate);
            this.tabSales.Controls.Add(this.btnGenerateReport);
            this.tabSales.Controls.Add(this.btnPrint);
            this.tabSales.Controls.Add(this.dgvReport);
        }

        private void SetupInventoryTab()
        {
            // Create new instances for Inventory tab
            var dgvInventory = new DataGridView();
            var btnGenerateInventory = new Button();
            var btnPrintInventory = new Button();

            // Generate Button
            btnGenerateInventory.Text = "Generate Inventory Report";
            btnGenerateInventory.Location = new Point(10, 10);
            btnGenerateInventory.Size = new Size(150, 25);
            btnGenerateInventory.Click += (s, e) => {
                try
                {
                    dgvInventory.DataSource = reportService.GetInventoryReport();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating inventory report: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Print Button
            btnPrintInventory.Text = "Print";
            btnPrintInventory.Location = new Point(170, 10);
            btnPrintInventory.Size = new Size(100, 25);
            btnPrintInventory.Click += (s, e) => {
                try
                {
                    string report = reportService.GenerateInventoryReport();
                    PrintHelper.PrintReport("Inventory Report", report);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error printing inventory report: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // DataGridView
            dgvInventory.Location = new Point(10, 45);
            dgvInventory.Size = new Size(1160, 680);
            dgvInventory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInventory.AllowUserToAddRows = false;
            dgvInventory.ReadOnly = true;
            dgvInventory.BackgroundColor = Color.White;

            // Add controls to Inventory Tab
            this.tabInventory.Controls.Add(btnGenerateInventory);
            this.tabInventory.Controls.Add(btnPrintInventory);
            this.tabInventory.Controls.Add(dgvInventory);
        }

        private void SetupProfitTab()
        {
            // Create new instances for Profit tab
            var dtpProfitStart = new DateTimePicker();
            var dtpProfitEnd = new DateTimePicker();
            var btnGenerateProfit = new Button();
            var btnPrintProfit = new Button();
            var dgvProfit = new DataGridView();

            // Date Pickers
            dtpProfitStart.Location = new Point(10, 10);
            dtpProfitStart.Size = new Size(200, 25);
            dtpProfitStart.Format = DateTimePickerFormat.Short;

            dtpProfitEnd.Location = new Point(220, 10);
            dtpProfitEnd.Size = new Size(200, 25);
            dtpProfitEnd.Format = DateTimePickerFormat.Short;

            // Generate Button
            btnGenerateProfit.Text = "Generate Profit Report";
            btnGenerateProfit.Location = new Point(430, 10);
            btnGenerateProfit.Size = new Size(150, 25);
            btnGenerateProfit.Click += (s, e) => {
                try
                {
                    dgvProfit.DataSource = reportService.GetProfitReport(
                        dtpProfitStart.Value, dtpProfitEnd.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating profit report: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Print Button
            btnPrintProfit.Text = "Print";
            btnPrintProfit.Location = new Point(590, 10);
            btnPrintProfit.Size = new Size(100, 25);
            btnPrintProfit.Click += (s, e) => {
                try
                {
                    string report = reportService.GenerateProfitReport(
                        dtpProfitStart.Value, dtpProfitEnd.Value);
                    PrintHelper.PrintReport("Profit Report", report);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error printing profit report: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // DataGridView
            dgvProfit.Location = new Point(10, 45);
            dgvProfit.Size = new Size(1160, 680);
            dgvProfit.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProfit.AllowUserToAddRows = false;
            dgvProfit.ReadOnly = true;
            dgvProfit.BackgroundColor = Color.White;

            // Add controls to Profit Tab
            this.tabProfit.Controls.Add(dtpProfitStart);
            this.tabProfit.Controls.Add(dtpProfitEnd);
            this.tabProfit.Controls.Add(btnGenerateProfit);
            this.tabProfit.Controls.Add(btnPrintProfit);
            this.tabProfit.Controls.Add(dgvProfit);
        }

        private void CboReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboReportType.SelectedIndex)
            {
                case 0: // Daily Sales
                    dtpEndDate.Visible = false;
                    break;
                case 1: // Date Range Sales
                case 2: // Top Selling Products
                    dtpEndDate.Visible = true;
                    break;
            }
        }

        private void BtnGenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                switch (cboReportType.SelectedIndex)
                {
                    case 0: // Daily Sales
                        dgvReport.DataSource = reportService.GetDailySalesReport(dtpStartDate.Value);
                        break;
                    case 1: // Date Range Sales
                        dgvReport.DataSource = reportService.GetSalesByDateRange(
                            dtpStartDate.Value, dtpEndDate.Value);
                        break;
                    case 2: // Top Selling Products
                        dgvReport.DataSource = reportService.GetTopSellingProducts(
                            dtpStartDate.Value, dtpEndDate.Value);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                string report = "";
                string title = "";

                switch (cboReportType.SelectedIndex)
                {
                    case 0: // Daily Sales
                        report = reportService.GenerateDailySalesReport(dtpStartDate.Value);
                        title = $"Daily Sales Report - {dtpStartDate.Value:MMM dd, yyyy}";
                        break;
                    case 1: // Date Range Sales
                        // TODO: Implement date range sales report formatting
                        MessageBox.Show("Date range report printing coming soon...");
                        return;
                    case 2: // Top Selling Products
                        // TODO: Implement top selling products report formatting
                        MessageBox.Show("Top selling products report printing coming soon...");
                        return;
                }

                PrintHelper.PrintReport(title, report);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
