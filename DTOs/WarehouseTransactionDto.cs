namespace FarmManagement.API.DTOs
{
    public class WarehouseTransactionDto
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;

        public int TraderId { get; set; }
        public string TraderName { get; set; } = string.Empty;

        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal PricePerTon { get; set; }
        public decimal TotalPrice { get; set; }

        public DateTime Date { get; set; }
    }
}
