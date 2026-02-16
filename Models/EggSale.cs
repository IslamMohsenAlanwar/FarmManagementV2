using System.ComponentModel.DataAnnotations.Schema;

namespace FarmManagement.API.Models
{
    public class EggSale
    {
        public int Id { get; set; }

        public int WarehouseId { get; set; }
        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; } = null!; 

        public int TraderId { get; set; }
        [ForeignKey("TraderId")]
        public Trader Trader { get; set; } = null!;

        public DateTime Date { get; set; }
        
        public decimal Quantity { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; } 
        
        public string? Notes { get; set; }
    }
}