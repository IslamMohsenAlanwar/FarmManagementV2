namespace FarmManagement.API.DTOs
{
    public class WarehouseTransactionUpdateDto
    {
        public DateTime Date { get; set; }
        public List<WarehouseTransactionItemDto> Items { get; set; } = new List<WarehouseTransactionItemDto>();
    }
}
