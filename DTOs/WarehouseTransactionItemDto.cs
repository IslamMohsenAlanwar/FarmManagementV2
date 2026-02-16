namespace FarmManagement.API.DTOs
{
    public class WarehouseTransactionItemDto
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PricePerTon { get; set; }
    }
}
