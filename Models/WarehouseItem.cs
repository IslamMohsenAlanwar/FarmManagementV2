using System.ComponentModel.DataAnnotations.Schema;

namespace FarmManagement.API.Models
{
    public class WarehouseItem
    {
        public int Id { get; set; }

        public int WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public decimal Quantity { get; set; } = 0;

        public decimal PricePerUnit { get; set; } = 0;

        public decimal Withdrawn { get; set; } = 0;

        public string Type { get; set; } = string.Empty;

        [NotMapped]
        public int FarmId => Warehouse.FarmId;
    }
}
