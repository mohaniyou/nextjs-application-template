using System;
using System.Collections.Generic;

namespace POSAccountantApp.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; }
        public int CashierId { get; set; }
        public string CashierName { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal VatPercentage { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Change { get; set; }
        public string PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; }
        public List<SaleItem> Items { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }

        public Sale()
        {
            SaleDate = DateTime.Now;
            CreatedDate = DateTime.Now;
            Items = new List<SaleItem>();
            VatPercentage = 0.12M; // 12% VAT by default
        }

        public void CalculateTotals()
        {
            SubTotal = 0;
            foreach (var item in Items)
            {
                item.CalculateTotal();
                SubTotal += item.Total;
            }

            DiscountAmount = SubTotal * (DiscountPercentage / 100);
            decimal afterDiscount = SubTotal - DiscountAmount;
            VatAmount = afterDiscount * VatPercentage;
            TotalAmount = afterDiscount + VatAmount;
            Change = AmountPaid - TotalAmount;
        }

        public void ValidateSale()
        {
            if (Items.Count == 0)
                throw new Exception("Sale must have at least one item.");

            if (CashierId <= 0)
                throw new Exception("Invalid cashier ID.");

            if (TotalAmount <= 0)
                throw new Exception("Total amount must be greater than zero.");

            if (AmountPaid < TotalAmount)
                throw new Exception("Amount paid must be greater than or equal to total amount.");

            if (string.IsNullOrWhiteSpace(PaymentMethod))
                throw new Exception("Payment method is required.");

            foreach (var item in Items)
            {
                item.ValidateSaleItem();
            }
        }

        public string GetReceiptNumber()
        {
            return $"SALE-{SaleId:D6}";
        }
    }

    public class SaleItem
    {
        public int SaleItemId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductBarcode { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public void CalculateTotal()
        {
            Total = (UnitPrice * Quantity) - Discount;
        }

        public void ValidateSaleItem()
        {
            if (ProductId <= 0)
                throw new Exception("Invalid product ID.");

            if (string.IsNullOrWhiteSpace(ProductName))
                throw new Exception("Product name is required.");

            if (UnitPrice <= 0)
                throw new Exception("Unit price must be greater than zero.");

            if (Quantity <= 0)
                throw new Exception("Quantity must be greater than zero.");

            if (Discount < 0)
                throw new Exception("Discount cannot be negative.");

            if (Total <= 0)
                throw new Exception("Total must be greater than zero.");
        }
    }
}
