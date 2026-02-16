namespace FarmManagement.API.Models
{
    public class Warehouse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int FarmId { get; set; }
        public Farm Farm { get; set; } = null!;

        public ICollection<WarehouseItem> WarehouseItems { get; set; } = new List<WarehouseItem>();

        public ICollection<WarehouseTransaction> Transactions { get; set; } = new List<WarehouseTransaction>();
    }
}
