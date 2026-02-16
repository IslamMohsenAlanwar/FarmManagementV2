namespace FarmManagement.API.DTOs
{
    public class WarehouseItemDto
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;

        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;

        public decimal Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal Withdrawn { get; set; }

    public decimal TotalValue => Quantity * PricePerUnit;

    }
}
