namespace FarmManagement.API.DTOs
{
    public class ChickenSaleResponseDto
    {
        public int Id { get; set; }
        public string TraderName { get; set; } = string.Empty;
        public string CycleName { get; set; } = string.Empty;

        public DateTime Date { get; set; }
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public decimal TraderBalance { get; set; }
    }
}
