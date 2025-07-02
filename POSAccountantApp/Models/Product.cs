using System;

namespace POSAccountantApp.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int Stock { get; set; }
        public int ReorderLevel { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public Product()
        {
            CreatedDate = DateTime.Now;
            IsActive = true;
        }

        public decimal GetProfit()
        {
            return Price - Cost;
        }

        public decimal GetProfitMargin()
        {
            if (Cost == 0) return 0;
            return (GetProfit() / Cost) * 100;
        }

        public bool IsLowStock()
        {
            return Stock <= ReorderLevel;
        }

        public override string ToString()
        {
            return $"{Name} (Stock: {Stock})";
        }

        public void ValidateProduct()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Product name is required.");

            if (string.IsNullOrWhiteSpace(Barcode))
                throw new ArgumentException("Barcode is required.");

            if (Price < 0)
                throw new ArgumentException("Price cannot be negative.");

            if (Cost < 0)
                throw new ArgumentException("Cost cannot be negative.");

            if (Stock < 0)
                throw new ArgumentException("Stock cannot be negative.");

            if (ReorderLevel < 0)
                throw new ArgumentException("Reorder level cannot be negative.");
        }

        public void UpdateStock(int quantity, bool isAddition)
        {
            if (isAddition)
            {
                if (quantity < 0)
                    throw new ArgumentException("Quantity to add must be positive.");
                Stock += quantity;
            }
            else
            {
                if (quantity > Stock)
                    throw new ArgumentException("Insufficient stock.");
                Stock -= quantity;
            }

            LastModifiedDate = DateTime.Now;
        }
    }
}
