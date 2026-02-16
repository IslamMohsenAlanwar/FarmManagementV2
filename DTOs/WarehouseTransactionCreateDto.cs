namespace FarmManagement.API.DTOs
{
    public class WarehouseTransactionCreateDto
    {
        public int WarehouseId { get; set; }
        public int TraderId { get; set; }
        public DateTime? Date { get; set; }
        public List<WarehouseTransactionItemDto> Items { get; set; } = new List<WarehouseTransactionItemDto>();
    }
}
