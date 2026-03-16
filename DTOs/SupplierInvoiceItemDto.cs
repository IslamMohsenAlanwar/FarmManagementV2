namespace FarmManagement.API.DTOs
{
    public class SupplierInvoiceItemDto
    {
        public string ItemName { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalPrice => Quantity * PricePerUnit;
    }

    public class SupplierInvoiceDto
    {
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string WarehouseName { get; set; } = "";
        public List<SupplierInvoiceItemDto> Items { get; set; } = new List<SupplierInvoiceItemDto>();
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => TotalAmount - PaidAmount;
    }

    public class SupplierInvoiceReportDto
    {
        public string TraderName { get; set; } = "";
        public decimal CurrentBalance { get; set; }
        public List<SupplierInvoiceDto> Invoices { get; set; } = new List<SupplierInvoiceDto>();
    }
}