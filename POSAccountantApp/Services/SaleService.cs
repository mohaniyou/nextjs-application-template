using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using POSAccountantApp.Models;
using POSAccountantApp.Database;

namespace POSAccountantApp.Services
{
    public class SaleService
    {
        private readonly InventoryService inventoryService;

        public SaleService()
        {
            inventoryService = new InventoryService();
        }

        public void InitializeSalesTables()
        {
            string query = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sales')
                BEGIN
                    CREATE TABLE Sales (
                        SaleId INT PRIMARY KEY IDENTITY(1,1),
                        SaleDate DATETIME NOT NULL,
                        CashierId INT FOREIGN KEY REFERENCES Users(UserId),
                        SubTotal DECIMAL(18,2) NOT NULL,
                        DiscountPercentage DECIMAL(5,2) NOT NULL,
                        DiscountAmount DECIMAL(18,2) NOT NULL,
                        VatPercentage DECIMAL(5,2) NOT NULL,
                        VatAmount DECIMAL(18,2) NOT NULL,
                        TotalAmount DECIMAL(18,2) NOT NULL,
                        AmountPaid DECIMAL(18,2) NOT NULL,
                        Change DECIMAL(18,2) NOT NULL,
                        PaymentMethod NVARCHAR(50) NOT NULL,
                        ReferenceNumber NVARCHAR(50),
                        Notes NVARCHAR(500),
                        CreatedDate DATETIME NOT NULL
                    );

                    CREATE TABLE SaleItems (
                        SaleItemId INT PRIMARY KEY IDENTITY(1,1),
                        SaleId INT FOREIGN KEY REFERENCES Sales(SaleId),
                        ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
                        UnitPrice DECIMAL(18,2) NOT NULL,
                        Quantity INT NOT NULL,
                        Discount DECIMAL(18,2) NOT NULL,
                        Total DECIMAL(18,2) NOT NULL
                    );
                END";

            SqlConnectionHelper.ExecuteNonQuery(query);
        }

        public int ProcessSale(Sale sale)
        {
            sale.ValidateSale();

            using (var connection = SqlConnectionHelper.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Insert sale
                    string saleQuery = @"
                        INSERT INTO Sales (
                            SaleDate, CashierId, SubTotal, DiscountPercentage,
                            DiscountAmount, VatPercentage, VatAmount, TotalAmount,
                            AmountPaid, Change, PaymentMethod, ReferenceNumber,
                            Notes, CreatedDate
                        )
                        VALUES (
                            @SaleDate, @CashierId, @SubTotal, @DiscountPercentage,
                            @DiscountAmount, @VatPercentage, @VatAmount, @TotalAmount,
                            @AmountPaid, @Change, @PaymentMethod, @ReferenceNumber,
                            @Notes, @CreatedDate
                        );
                        SELECT SCOPE_IDENTITY();";

                    int saleId;
                    using (var command = new SqlCommand(saleQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@SaleDate", sale.SaleDate);
                        command.Parameters.AddWithValue("@CashierId", sale.CashierId);
                        command.Parameters.AddWithValue("@SubTotal", sale.SubTotal);
                        command.Parameters.AddWithValue("@DiscountPercentage", sale.DiscountPercentage);
                        command.Parameters.AddWithValue("@DiscountAmount", sale.DiscountAmount);
                        command.Parameters.AddWithValue("@VatPercentage", sale.VatPercentage);
                        command.Parameters.AddWithValue("@VatAmount", sale.VatAmount);
                        command.Parameters.AddWithValue("@TotalAmount", sale.TotalAmount);
                        command.Parameters.AddWithValue("@AmountPaid", sale.AmountPaid);
                        command.Parameters.AddWithValue("@Change", sale.Change);
                        command.Parameters.AddWithValue("@PaymentMethod", sale.PaymentMethod);
                        command.Parameters.AddWithValue("@ReferenceNumber", 
                            (object)sale.ReferenceNumber ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Notes", 
                            (object)sale.Notes ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedDate", sale.CreatedDate);

                        saleId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // Insert sale items
                    foreach (var item in sale.Items)
                    {
                        string itemQuery = @"
                            INSERT INTO SaleItems (
                                SaleId, ProductId, UnitPrice, Quantity,
                                Discount, Total
                            )
                            VALUES (
                                @SaleId, @ProductId, @UnitPrice, @Quantity,
                                @Discount, @Total
                            )";

                        using (var command = new SqlCommand(itemQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@SaleId", saleId);
                            command.Parameters.AddWithValue("@ProductId", item.ProductId);
                            command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                            command.Parameters.AddWithValue("@Quantity", item.Quantity);
                            command.Parameters.AddWithValue("@Discount", item.Discount);
                            command.Parameters.AddWithValue("@Total", item.Total);
                            command.ExecuteNonQuery();
                        }

                        // Update inventory
                        inventoryService.UpdateStock(
                            item.ProductId,
                            item.Quantity,
                            false, // isAddition = false (reducing stock)
                            $"Sale #{saleId}",
                            sale.CashierId
                        );
                    }

                    transaction.Commit();
                    return saleId;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"Failed to process sale: {ex.Message}", ex);
                }
            }
        }

        public Sale GetSaleById(int saleId)
        {
            string saleQuery = @"
                SELECT s.*, u.FullName as CashierName
                FROM Sales s
                INNER JOIN Users u ON s.CashierId = u.UserId
                WHERE s.SaleId = @SaleId";

            string itemsQuery = @"
                SELECT si.*, p.Name as ProductName, p.Barcode as ProductBarcode
                FROM SaleItems si
                INNER JOIN Products p ON si.ProductId = p.ProductId
                WHERE si.SaleId = @SaleId";

            try
            {
                var saleParams = new SqlParameter[] { new SqlParameter("@SaleId", saleId) };
                DataTable saleResult = SqlConnectionHelper.ExecuteQuery(saleQuery, saleParams);

                if (saleResult.Rows.Count == 0)
                    return null;

                var sale = MapDataRowToSale(saleResult.Rows[0]);

                DataTable itemsResult = SqlConnectionHelper.ExecuteQuery(itemsQuery, saleParams);
                foreach (DataRow row in itemsResult.Rows)
                {
                    sale.Items.Add(MapDataRowToSaleItem(row));
                }

                return sale;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get sale: {ex.Message}", ex);
            }
        }

        public List<Sale> GetSalesByDateRange(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT s.*, u.FullName as CashierName
                FROM Sales s
                INNER JOIN Users u ON s.CashierId = u.UserId
                WHERE s.SaleDate BETWEEN @StartDate AND @EndDate
                ORDER BY s.SaleDate DESC";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            try
            {
                DataTable result = SqlConnectionHelper.ExecuteQuery(query, parameters);
                var sales = new List<Sale>();

                foreach (DataRow row in result.Rows)
                {
                    sales.Add(MapDataRowToSale(row));
                }

                return sales;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get sales: {ex.Message}", ex);
            }
        }

        private Sale MapDataRowToSale(DataRow row)
        {
            return new Sale
            {
                SaleId = Convert.ToInt32(row["SaleId"]),
                SaleDate = Convert.ToDateTime(row["SaleDate"]),
                CashierId = Convert.ToInt32(row["CashierId"]),
                CashierName = row["CashierName"].ToString(),
                SubTotal = Convert.ToDecimal(row["SubTotal"]),
                DiscountPercentage = Convert.ToDecimal(row["DiscountPercentage"]),
                DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                VatPercentage = Convert.ToDecimal(row["VatPercentage"]),
                VatAmount = Convert.ToDecimal(row["VatAmount"]),
                TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                AmountPaid = Convert.ToDecimal(row["AmountPaid"]),
                Change = Convert.ToDecimal(row["Change"]),
                PaymentMethod = row["PaymentMethod"].ToString(),
                ReferenceNumber = row["ReferenceNumber"] == DBNull.Value ? null : row["ReferenceNumber"].ToString(),
                Notes = row["Notes"] == DBNull.Value ? null : row["Notes"].ToString(),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"])
            };
        }

        private SaleItem MapDataRowToSaleItem(DataRow row)
        {
            return new SaleItem
            {
                SaleItemId = Convert.ToInt32(row["SaleItemId"]),
                SaleId = Convert.ToInt32(row["SaleId"]),
                ProductId = Convert.ToInt32(row["ProductId"]),
                ProductName = row["ProductName"].ToString(),
                ProductBarcode = row["ProductBarcode"].ToString(),
                UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                Quantity = Convert.ToInt32(row["Quantity"]),
                Discount = Convert.ToDecimal(row["Discount"]),
                Total = Convert.ToDecimal(row["Total"])
            };
        }
    }
}
