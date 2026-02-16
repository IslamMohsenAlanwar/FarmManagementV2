namespace FarmManagement.API.DTOs
{
    // ================== Asset Warehouse View ==================
    public class AssetWarehouseDto
    {
        public int Id { get; set; }
        public int FarmId { get; set; }           
        public string FarmName { get; set; } = string.Empty;
         public string Name { get; set; } = "";

        public List<AssetWarehouseItemDto> Items { get; set; } = new List<AssetWarehouseItemDto>();
    }

    // ================== Create Asset Warehouse ==================
    public class CreateAssetWarehouseDto
    {
        public int FarmId { get; set; }      
        public string Name { get; set; } = string.Empty; 
    }

    // ================== Asset Warehouse Item ==================
    public class AssetWarehouseItemDto
{
    public int Id { get; set; }
    public int AssetItemId { get; set; }
    public string AssetItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal InBarnsQuantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal TotalValue => Quantity * UnitPrice;
}
}
