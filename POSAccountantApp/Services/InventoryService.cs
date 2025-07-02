using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using POSAccountantApp.Models;
using POSAccountantApp.Database;

namespace POSAccountantApp.Services
{
    public class InventoryService
    {
        public void InitializeProductTable()
        {
            string query = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
                BEGIN
                    CREATE TABLE Products (
                        ProductId INT PRIMARY KEY IDENTITY(1,1),
                        Barcode NVARCHAR(50) UNIQUE NOT NULL,
                        Name NVARCHAR(100) NOT NULL,
                        Description NVARCHAR(500),
                        Price DECIMAL(18,2) NOT NULL,
                        Cost DECIMAL(18,2) NOT NULL,
                        Stock INT NOT NULL,
                        ReorderLevel INT NOT NULL,
                        Category NVARCHAR(50),
                        Unit NVARCHAR(20),
                        IsActive BIT NOT NULL,
                        CreatedDate DATETIME NOT NULL,
                        LastModifiedDate DATETIME
                    );

                    CREATE TABLE InventoryLogs (
                        LogId INT PRIMARY KEY IDENTITY(1,1),
                        ProductId INT FOREIGN KEY REFERENCES Products(ProductId),
                        ChangeType NVARCHAR(20) NOT NULL,
                        Quantity INT NOT NULL,
                        OldStock INT NOT NULL,
                        NewStock INT NOT NULL,
                        Remarks NVARCHAR(500),
                        LogDate DATETIME NOT NULL,
                        UserId INT FOREIGN KEY REFERENCES Users(UserId)
                    );
                END";

            SqlConnectionHelper.ExecuteNonQuery(query);
        }

        public void AddProduct(Product product)
        {
            product.ValidateProduct();

            string query = @"
                INSERT INTO Products (
                    Barcode, Name, Description, Price, Cost, Stock, 
                    ReorderLevel, Category, Unit, IsActive, CreatedDate
                )
                VALUES (
                    @Barcode, @Name, @Description, @Price, @Cost, @Stock,
                    @ReorderLevel, @Category, @Unit, @IsActive, @CreatedDate
                )";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Barcode", product.Barcode),
                new SqlParameter("@Name", product.Name),
                new SqlParameter("@Description", (object)product.Description ?? DBNull.Value),
                new SqlParameter("@Price", product.Price),
                new SqlParameter("@Cost", product.Cost),
                new SqlParameter("@Stock", product.Stock),
                new SqlParameter("@ReorderLevel", product.ReorderLevel),
                new SqlParameter("@Category", (object)product.Category ?? DBNull.Value),
                new SqlParameter("@Unit", (object)product.Unit ?? DBNull.Value),
                new SqlParameter("@IsActive", product.IsActive),
                new SqlParameter("@CreatedDate", product.CreatedDate)
            };

            try
            {
                SqlConnectionHelper.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add product: {ex.Message}", ex);
            }
        }

        public void UpdateProduct(Product product)
        {
            product.ValidateProduct();

            string query = @"
                UPDATE Products 
                SET Name = @Name,
                    Description = @Description,
                    Price = @Price,
                    Cost = @Cost,
                    ReorderLevel = @ReorderLevel,
                    Category = @Category,
                    Unit = @Unit,
                    IsActive = @IsActive,
                    LastModifiedDate = @LastModifiedDate
                WHERE ProductId = @ProductId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ProductId", product.ProductId),
                new SqlParameter("@Name", product.Name),
                new SqlParameter("@Description", (object)product.Description ?? DBNull.Value),
                new SqlParameter("@Price", product.Price),
                new SqlParameter("@Cost", product.Cost),
                new SqlParameter("@ReorderLevel", product.ReorderLevel),
                new SqlParameter("@Category", (object)product.Category ?? DBNull.Value),
                new SqlParameter("@Unit", (object)product.Unit ?? DBNull.Value),
                new SqlParameter("@IsActive", product.IsActive),
                new SqlParameter("@LastModifiedDate", DateTime.Now)
            };

            try
            {
                SqlConnectionHelper.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update product: {ex.Message}", ex);
            }
        }

        public void UpdateStock(int productId, int quantity, bool isAddition, string remarks, int userId)
        {
            using (var connection = SqlConnectionHelper.GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Get current stock
                        string getStockQuery = "SELECT Stock FROM Products WHERE ProductId = @ProductId";
                        var getStockParam = new SqlParameter("@ProductId", productId);
                        
                        int currentStock;
                        using (var command = new SqlCommand(getStockQuery, connection, transaction))
                        {
                            command.Parameters.Add(getStockParam);
                            currentStock = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Calculate new stock
                        int newStock = isAddition ? currentStock + quantity : currentStock - quantity;
                        
                        if (newStock < 0)
                            throw new Exception("Insufficient stock.");

                        // Update stock
                        string updateQuery = "UPDATE Products SET Stock = @NewStock WHERE ProductId = @ProductId";
                        using (var command = new SqlCommand(updateQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@NewStock", newStock);
                            command.Parameters.AddWithValue("@ProductId", productId);
                            command.ExecuteNonQuery();
                        }

                        // Log the change
                        string logQuery = @"
                            INSERT INTO InventoryLogs (
                                ProductId, ChangeType, Quantity, OldStock, NewStock, 
                                Remarks, LogDate, UserId
                            )
                            VALUES (
                                @ProductId, @ChangeType, @Quantity, @OldStock, @NewStock,
                                @Remarks, @LogDate, @UserId
                            )";

                        using (var command = new SqlCommand(logQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@ProductId", productId);
                            command.Parameters.AddWithValue("@ChangeType", isAddition ? "Stock In" : "Stock Out");
                            command.Parameters.AddWithValue("@Quantity", quantity);
                            command.Parameters.AddWithValue("@OldStock", currentStock);
                            command.Parameters.AddWithValue("@NewStock", newStock);
                            command.Parameters.AddWithValue("@Remarks", remarks);
                            command.Parameters.AddWithValue("@LogDate", DateTime.Now);
                            command.Parameters.AddWithValue("@UserId", userId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Failed to update stock: {ex.Message}", ex);
                    }
                }
            }
        }

        public Product GetProductByBarcode(string barcode)
        {
            string query = "SELECT * FROM Products WHERE Barcode = @Barcode";
            var parameters = new SqlParameter[] { new SqlParameter("@Barcode", barcode) };

            try
            {
                DataTable result = SqlConnectionHelper.ExecuteQuery(query, parameters);
                if (result.Rows.Count == 0)
                    return null;

                return MapDataRowToProduct(result.Rows[0]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get product: {ex.Message}", ex);
            }
        }

        public List<Product> GetAllProducts(bool activeOnly = true)
        {
            string query = "SELECT * FROM Products";
            if (activeOnly)
                query += " WHERE IsActive = 1";
            query += " ORDER BY Name";

            try
            {
                DataTable result = SqlConnectionHelper.ExecuteQuery(query);
                var products = new List<Product>();

                foreach (DataRow row in result.Rows)
                {
                    products.Add(MapDataRowToProduct(row));
                }

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get products: {ex.Message}", ex);
            }
        }

        public List<Product> GetLowStockProducts()
        {
            string query = "SELECT * FROM Products WHERE Stock <= ReorderLevel AND IsActive = 1";

            try
            {
                DataTable result = SqlConnectionHelper.ExecuteQuery(query);
                var products = new List<Product>();

                foreach (DataRow row in result.Rows)
                {
                    products.Add(MapDataRowToProduct(row));
                }

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get low stock products: {ex.Message}", ex);
            }
        }

        private Product MapDataRowToProduct(DataRow row)
        {
            return new Product
            {
                ProductId = Convert.ToInt32(row["ProductId"]),
                Barcode = row["Barcode"].ToString(),
                Name = row["Name"].ToString(),
                Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString(),
                Price = Convert.ToDecimal(row["Price"]),
                Cost = Convert.ToDecimal(row["Cost"]),
                Stock = Convert.ToInt32(row["Stock"]),
                ReorderLevel = Convert.ToInt32(row["ReorderLevel"]),
                Category = row["Category"] == DBNull.Value ? null : row["Category"].ToString(),
                Unit = row["Unit"] == DBNull.Value ? null : row["Unit"].ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                LastModifiedDate = row["LastModifiedDate"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["LastModifiedDate"])
            };
        }
    }
}
