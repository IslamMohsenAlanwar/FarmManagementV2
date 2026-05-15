namespace FarmManagement.API.DTOs
{
    public class CashBoxTransactionDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        public EnumDto Type { get; set; }
        public EnumDto Category { get; set; }

        public decimal Amount { get; set; }
        public string? Notes { get; set; }

        public int? TraderId { get; set; }
        public int? WorkerId { get; set; }
        public int? WarehouseId { get; set; }
    }

    public class EnumDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? DisplayName { get; set; }
    }

}
