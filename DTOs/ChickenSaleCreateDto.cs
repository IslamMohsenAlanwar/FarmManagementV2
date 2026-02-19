namespace FarmManagement.API.DTOs
{
    public class ChickenSaleCreateDto
    {
        public int CycleId { get; set; }
        public int TraderId { get; set; }
        public DateTime Date { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PaidAmount { get; set; }

        public string? Notes { get; set; }
    }
}
