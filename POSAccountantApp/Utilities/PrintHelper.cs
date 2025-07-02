using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using POSAccountantApp.Models;

namespace POSAccountantApp.Utilities
{
    public static class PrintHelper
    {
        private static Font titleFont = new Font("Arial", 14, FontStyle.Bold);
        private static Font headerFont = new Font("Arial", 10, FontStyle.Bold);
        private static Font bodyFont = new Font("Arial", 10);
        private static int lineHeight = 20;
        private static int margin = 20;
        private static int currentY = margin;
        private static Sale currentSale;

        public static void PrintReceipt(Sale sale)
        {
            try
            {
                currentSale = sale;
                var document = new PrintDocument();
                document.PrintPage += Document_PrintPage;

                var dialog = new PrintPreviewDialog();
                dialog.Document = document;
                dialog.WindowState = FormWindowState.Maximized;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing receipt: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void Document_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            int startX = margin;
            currentY = margin;
            int width = e.PageBounds.Width - (margin * 2);

            // Store Name/Header
            string storeName = "POS + Accounting System";
            SizeF storeNameSize = g.MeasureString(storeName, titleFont);
            g.DrawString(storeName, titleFont, Brushes.Black, 
                (e.PageBounds.Width - storeNameSize.Width) / 2, currentY);
            currentY += (int)storeNameSize.Height + 10;

            // Receipt info
            string receiptInfo = $"Receipt #{currentSale.GetReceiptNumber()}";
            g.DrawString(receiptInfo, headerFont, Brushes.Black, startX, currentY);
            currentY += lineHeight;

            string dateInfo = $"Date: {currentSale.SaleDate:yyyy-MM-dd HH:mm:ss}";
            g.DrawString(dateInfo, headerFont, Brushes.Black, startX, currentY);
            currentY += lineHeight;

            string cashierInfo = $"Cashier: {currentSale.CashierName}";
            g.DrawString(cashierInfo, headerFont, Brushes.Black, startX, currentY);
            currentY += lineHeight * 2;

            // Column headers
            DrawLine(g, startX, currentY - 5, width);
            g.DrawString("Item", headerFont, Brushes.Black, startX, currentY);
            g.DrawString("Qty", headerFont, Brushes.Black, startX + 300, currentY);
            g.DrawString("Price", headerFont, Brushes.Black, startX + 400, currentY);
            g.DrawString("Total", headerFont, Brushes.Black, startX + 500, currentY);
            currentY += lineHeight;
            DrawLine(g, startX, currentY, width);
            currentY += 5;

            // Items
            foreach (var item in currentSale.Items)
            {
                g.DrawString(item.ProductName, bodyFont, Brushes.Black, startX, currentY);
                g.DrawString(item.Quantity.ToString(), bodyFont, Brushes.Black, startX + 300, currentY);
                g.DrawString(item.UnitPrice.ToString("C2"), bodyFont, Brushes.Black, startX + 400, currentY);
                g.DrawString(item.Total.ToString("C2"), bodyFont, Brushes.Black, startX + 500, currentY);
                currentY += lineHeight;
            }

            currentY += 5;
            DrawLine(g, startX, currentY, width);
            currentY += 10;

            // Totals
            int totalsX = startX + 350;
            g.DrawString("Sub Total:", headerFont, Brushes.Black, totalsX, currentY);
            g.DrawString(currentSale.SubTotal.ToString("C2"), bodyFont, Brushes.Black, totalsX + 150, currentY);
            currentY += lineHeight;

            if (currentSale.DiscountAmount > 0)
            {
                g.DrawString($"Discount ({currentSale.DiscountPercentage}%):", headerFont, Brushes.Black, totalsX, currentY);
                g.DrawString(currentSale.DiscountAmount.ToString("C2"), bodyFont, Brushes.Black, totalsX + 150, currentY);
                currentY += lineHeight;
            }

            g.DrawString("VAT (12%):", headerFont, Brushes.Black, totalsX, currentY);
            g.DrawString(currentSale.VatAmount.ToString("C2"), bodyFont, Brushes.Black, totalsX + 150, currentY);
            currentY += lineHeight;

            DrawLine(g, totalsX, currentY, 250);
            currentY += 5;

            g.DrawString("Total:", headerFont, Brushes.Black, totalsX, currentY);
            g.DrawString(currentSale.TotalAmount.ToString("C2"), headerFont, Brushes.Black, totalsX + 150, currentY);
            currentY += lineHeight;

            g.DrawString("Amount Paid:", headerFont, Brushes.Black, totalsX, currentY);
            g.DrawString(currentSale.AmountPaid.ToString("C2"), bodyFont, Brushes.Black, totalsX + 150, currentY);
            currentY += lineHeight;

            g.DrawString("Change:", headerFont, Brushes.Black, totalsX, currentY);
            g.DrawString(currentSale.Change.ToString("C2"), bodyFont, Brushes.Black, totalsX + 150, currentY);
            currentY += lineHeight * 2;

            // Payment info
            string paymentInfo = $"Payment Method: {currentSale.PaymentMethod}";
            if (!string.IsNullOrEmpty(currentSale.ReferenceNumber))
            {
                paymentInfo += $" (Ref: {currentSale.ReferenceNumber})";
            }
            g.DrawString(paymentInfo, bodyFont, Brushes.Black, startX, currentY);
            currentY += lineHeight * 2;

            // Footer
            string footer = "Thank you for your purchase!";
            SizeF footerSize = g.MeasureString(footer, bodyFont);
            g.DrawString(footer, bodyFont, Brushes.Black,
                (e.PageBounds.Width - footerSize.Width) / 2, currentY);
        }

        private static void DrawLine(Graphics g, int x, int y, int width)
        {
            g.DrawLine(Pens.Black, x, y, x + width, y);
        }

        public static void PrintReport(string title, string content)
        {
            try
            {
                var document = new PrintDocument();
                document.PrintPage += (sender, e) =>
                {
                    Graphics g = e.Graphics;
                    int startX = margin;
                    int startY = margin;
                    int width = e.PageBounds.Width - (margin * 2);

                    // Title
                    SizeF titleSize = g.MeasureString(title, titleFont);
                    g.DrawString(title, titleFont, Brushes.Black,
                        (e.PageBounds.Width - titleSize.Width) / 2, startY);
                    startY += (int)titleSize.Height + 20;

                    // Content
                    g.DrawString(content, bodyFont, Brushes.Black,
                        new RectangleF(startX, startY, width, e.PageBounds.Height - margin));
                };

                var dialog = new PrintPreviewDialog();
                dialog.Document = document;
                dialog.WindowState = FormWindowState.Maximized;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
