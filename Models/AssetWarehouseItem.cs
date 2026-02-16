namespace FarmManagement.API.Models
{
public class AssetWarehouseItem
{
    public int Id { get; set; }

    public int AssetItemId { get; set; }
    public AssetItem AssetItem { get; set; } = null!;

    public int AssetWarehouseId { get; set; } // FK 
    public AssetWarehouse AssetWarehouse { get; set; } = null!;

    public decimal Quantity { get; set; }
    public decimal InBarnsQuantity { get; set; } 
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
}
}
