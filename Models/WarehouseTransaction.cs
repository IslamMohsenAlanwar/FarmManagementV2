namespace FarmManagement.API.Models
{
    public class WarehouseTransaction
    {
        public int Id { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;


        public int? TraderId { get; set; }
        public Trader? Trader { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public decimal Quantity { get; set; }

        public decimal PricePerTon { get; set; } 
        public decimal TotalPrice { get; set; }

        public DateTime Date { get; set; }

        public string TransactionType { get; set; } = string.Empty;

        public int? EggProductionRecordId { get; set; }

        public int? EggSaleId { get; set; }
        public EggSale? EggSale { get; set; }
    }
}