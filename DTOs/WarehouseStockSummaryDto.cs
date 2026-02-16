namespace FarmManagement.API.DTOs
{
    public class WarehouseStockSummaryDto
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;

        public decimal TotalQuantity { get; set; }
        public decimal TotalStockValue { get; set; }

        public decimal RemainingQuantity { get; set; }
        public decimal RemainingStockValue { get; set; }
    }
}
