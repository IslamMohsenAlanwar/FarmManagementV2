namespace FarmManagement.API.DTOs
{
    public class AddAssetToWarehouseDto
    {
        public int AssetWarehouseId { get; set; }
        public int AssetItemId { get; set; }      
        public decimal Quantity { get; set; }     
        public decimal UnitPrice { get; set; }   
    }
}
