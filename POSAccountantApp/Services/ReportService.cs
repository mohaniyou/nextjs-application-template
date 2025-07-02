using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;
using POSAccountantApp.Models;
using POSAccountantApp.Database;

namespace POSAccountantApp.Services
{
    public class ReportService
    {
        public DataTable GetDailySalesReport(DateTime date)
        {
            string query = @"
                SELECT 
                    s.SaleId,
                    s.SaleDate,
                    u.FullName as Cashier,
                    s.SubTotal,
                    s.DiscountAmount,
                    s.VatAmount,
                    s.TotalAmount,
                    s.PaymentMethod,
                    COUNT(si.SaleItemId) as ItemCount
                FROM Sales s
                INNER JOIN Users u ON s.CashierId = u.UserId
                INNER JOIN SaleItems si ON s.SaleId = si.SaleId
                WHERE CAST(s.SaleDate as DATE) = @Date
                GROUP BY 
                    s.SaleId, s.SaleDate, u.FullName, s.SubTotal,
                    s.DiscountAmount, s.VatAmount, s.TotalAmount, s.PaymentMethod
                ORDER BY s.SaleDate";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Date", date.Date)
            };

            return SqlConnectionHelper.ExecuteQuery(query, parameters);
        }

        public DataTable GetSalesByDateRange(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT 
                    CAST(s.SaleDate as DATE) as Date,
                    COUNT(DISTINCT s.SaleId) as TransactionCount,
                    SUM(s.SubTotal) as GrossSales,
                    SUM(s.DiscountAmount) as TotalDiscounts,
                    SUM(s.VatAmount) as TotalVAT,
                    SUM(s.TotalAmount) as NetSales
                FROM Sales s
                WHERE s.SaleDate BETWEEN @StartDate AND @EndDate
                GROUP BY CAST(s.SaleDate as DATE)
                ORDER BY Date";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@StartDate", startDate.Date),
                new SqlParameter("@EndDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            return SqlConnectionHelper.ExecuteQuery(query, parameters);
        }

        public DataTable GetInventoryReport()
        {
            string query = @"
                SELECT 
                    p.ProductId,
                    p.Barcode,
                    p.Name,
                    p.Category,
                    p.Stock,
                    p.ReorderLevel,
                    p.Cost,
                    p.Price,
                    (p.Stock * p.Cost) as StockValue,
                    CASE WHEN p.Stock <= p.ReorderLevel THEN 'Yes' ELSE 'No' END as NeedsReorder
                FROM Products p
                WHERE p.IsActive = 1
                ORDER BY p.Category, p.Name";

            return SqlConnectionHelper.ExecuteQuery(query);
        }

        public DataTable GetTopSellingProducts(DateTime startDate, DateTime endDate, int top = 10)
        {
            string query = @"
                SELECT TOP (@Top)
                    p.Barcode,
                    p.Name,
                    p.Category,
                    SUM(si.Quantity) as TotalQuantitySold,
                    SUM(si.Total) as TotalRevenue,
                    COUNT(DISTINCT s.SaleId) as TransactionCount
                FROM Products p
                INNER JOIN SaleItems si ON p.ProductId = si.ProductId
                INNER JOIN Sales s ON si.SaleId = s.SaleId
                WHERE s.SaleDate BETWEEN @StartDate AND @EndDate
                GROUP BY p.ProductId, p.Barcode, p.Name, p.Category
                ORDER BY TotalQuantitySold DESC";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@StartDate", startDate.Date),
                new SqlParameter("@EndDate", endDate.Date.AddDays(1).AddSeconds(-1)),
                new SqlParameter("@Top", top)
            };

            return SqlConnectionHelper.ExecuteQuery(query, parameters);
        }

        public DataTable GetProfitReport(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT 
                    CAST(s.SaleDate as DATE) as Date,
                    SUM(si.Quantity * p.Cost) as TotalCost,
                    SUM(s.SubTotal) as GrossSales,
                    SUM(s.DiscountAmount) as Discounts,
                    SUM(s.TotalAmount) as NetSales,
                    SUM(s.TotalAmount - (si.Quantity * p.Cost)) as GrossProfit
                FROM Sales s
                INNER JOIN SaleItems si ON s.SaleId = si.SaleId
                INNER JOIN Products p ON si.ProductId = p.ProductId
                WHERE s.SaleDate BETWEEN @StartDate AND @EndDate
                GROUP BY CAST(s.SaleDate as DATE)
                ORDER BY Date";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@StartDate", startDate.Date),
                new SqlParameter("@EndDate", endDate.Date.AddDays(1).AddSeconds(-1))
            };

            return SqlConnectionHelper.ExecuteQuery(query, parameters);
        }

        public string GenerateDailySalesReport(DateTime date)
        {
            var report = new StringBuilder();
            var data = GetDailySalesReport(date);
            decimal totalSales = 0;
            int totalTransactions = 0;

            report.AppendLine($"Daily Sales Report for {date:MMMM dd, yyyy}");
            report.AppendLine("=====================================");
            report.AppendLine();

            foreach (DataRow row in data.Rows)
            {
                report.AppendLine($"Sale #{row["SaleId"]} - {Convert.ToDateTime(row["SaleDate"]):HH:mm:ss}");
                report.AppendLine($"Cashier: {row["Cashier"]}");
                report.AppendLine($"Items: {row["ItemCount"]}");
                report.AppendLine($"Sub Total: {Convert.ToDecimal(row["SubTotal"]):C2}");
                report.AppendLine($"Discount: {Convert.ToDecimal(row["DiscountAmount"]):C2}");
                report.AppendLine($"VAT: {Convert.ToDecimal(row["VatAmount"]):C2}");
                report.AppendLine($"Total: {Convert.ToDecimal(row["TotalAmount"]):C2}");
                report.AppendLine($"Payment: {row["PaymentMethod"]}");
                report.AppendLine("-------------------------------------");

                totalSales += Convert.ToDecimal(row["TotalAmount"]);
                totalTransactions++;
            }

            report.AppendLine();
            report.AppendLine($"Total Transactions: {totalTransactions}");
            report.AppendLine($"Total Sales: {totalSales:C2}");
            report.AppendLine($"Average Transaction Value: {(totalTransactions > 0 ? totalSales / totalTransactions : 0):C2}");

            return report.ToString();
        }

        public string GenerateInventoryReport()
        {
            var report = new StringBuilder();
            var data = GetInventoryReport();
            decimal totalStockValue = 0;
            int lowStockItems = 0;

            report.AppendLine("Inventory Status Report");
            report.AppendLine("=====================");
            report.AppendLine();

            string currentCategory = "";
            foreach (DataRow row in data.Rows)
            {
                string category = row["Category"].ToString();
                if (category != currentCategory)
                {
                    report.AppendLine();
                    report.AppendLine($"Category: {category}");
                    report.AppendLine("---------------------");
                    currentCategory = category;
                }

                report.AppendLine($"Product: {row["Name"]}");
                report.AppendLine($"Barcode: {row["Barcode"]}");
                report.AppendLine($"Stock: {row["Stock"]} units");
                report.AppendLine($"Reorder Level: {row["ReorderLevel"]}");
                report.AppendLine($"Stock Value: {Convert.ToDecimal(row["StockValue"]):C2}");
                
                if (row["NeedsReorder"].ToString() == "Yes")
                {
                    report.AppendLine("*** NEEDS REORDER ***");
                    lowStockItems++;
                }
                
                report.AppendLine("---------------------");

                totalStockValue += Convert.ToDecimal(row["StockValue"]);
            }

            report.AppendLine();
            report.AppendLine($"Total Stock Value: {totalStockValue:C2}");
            report.AppendLine($"Low Stock Items: {lowStockItems}");

            return report.ToString();
        }

        public string GenerateProfitReport(DateTime startDate, DateTime endDate)
        {
            var report = new StringBuilder();
            var data = GetProfitReport(startDate, endDate);
            decimal totalCost = 0;
            decimal totalSales = 0;
            decimal totalProfit = 0;

            report.AppendLine($"Profit Report ({startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy})");
            report.AppendLine("===========================================");
            report.AppendLine();

            foreach (DataRow row in data.Rows)
            {
                report.AppendLine($"Date: {Convert.ToDateTime(row["Date"]):MMM dd, yyyy}");
                report.AppendLine($"Cost of Goods: {Convert.ToDecimal(row["TotalCost"]):C2}");
                report.AppendLine($"Gross Sales: {Convert.ToDecimal(row["GrossSales"]):C2}");
                report.AppendLine($"Discounts: {Convert.ToDecimal(row["Discounts"]):C2}");
                report.AppendLine($"Net Sales: {Convert.ToDecimal(row["NetSales"]):C2}");
                report.AppendLine($"Gross Profit: {Convert.ToDecimal(row["GrossProfit"]):C2}");
                report.AppendLine("-------------------------------------------");

                totalCost += Convert.ToDecimal(row["TotalCost"]);
                totalSales += Convert.ToDecimal(row["NetSales"]);
                totalProfit += Convert.ToDecimal(row["GrossProfit"]);
            }

            report.AppendLine();
            report.AppendLine("Summary");
            report.AppendLine("-------");
            report.AppendLine($"Total Cost of Goods: {totalCost:C2}");
            report.AppendLine($"Total Net Sales: {totalSales:C2}");
            report.AppendLine($"Total Gross Profit: {totalProfit:C2}");
            
            if (totalSales > 0)
            {
                decimal profitMargin = (totalProfit / totalSales) * 100;
                report.AppendLine($"Profit Margin: {profitMargin:F2}%");
            }

            return report.ToString();
        }
    }
}
