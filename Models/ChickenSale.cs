using System.ComponentModel.DataAnnotations.Schema;

namespace FarmManagement.API.Models
{
    public class ChickenSale
    {
        public int Id { get; set; }

        public int CycleId { get; set; }
        public Cycle Cycle { get; set; } = null!;

        public int TraderId { get; set; }
        public Trader Trader { get; set; } = null!;

        public DateTime Date { get; set; }

        public int Quantity { get; set; } 

        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }

        public string? Notes { get; set; }
    }
}
