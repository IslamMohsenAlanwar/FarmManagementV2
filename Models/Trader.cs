namespace FarmManagement.API.Models
{
    public enum TraderType 
    { 
        مورد=1, // مورد (علف، أدوية، خامات)
        مشتري=2     // مشتري (تاجر بيض)
    }

    public class Trader
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;


        public decimal Balance { get; set; } = 0;

        public TraderType Type { get; set; } = TraderType.مورد;

        
        public ICollection<WarehouseTransaction> Transactions { get; set; } = new List<WarehouseTransaction>();

        public ICollection<EggSale> EggSales { get; set; } = new List<EggSale>();
    }
}