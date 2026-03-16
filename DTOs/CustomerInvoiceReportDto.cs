namespace FarmManagement.API.DTOs
{
    public class CustomerInvoiceReportDto
    {
        public string TraderName { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public List<CustomerInvoiceDto> Invoices { get; set; } = new List<CustomerInvoiceDto>();
    }

    public class CustomerInvoiceDto
    {
        public int SaleId { get; set; }
        public DateTime Date { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public List<CustomerInvoiceItemDto> Items { get; set; } = new List<CustomerInvoiceItemDto>();
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }

    public class CustomerInvoiceItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string EggQuality { get; set; } = "غير معروف";
    }
}