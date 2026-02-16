namespace FarmManagement.API.Models
{
    public class Asset
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;
    }
}
