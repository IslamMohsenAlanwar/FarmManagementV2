namespace FarmManagement.API.Models
{
public class AssetWarehouse
{
    public int Id { get; set; }

    public int FarmId { get; set; }
    public Farm Farm { get; set; } = null!;
    public string Name { get; set; } = string.Empty; 


    public ICollection<AssetWarehouseItem> Items { get; set; } = new List<AssetWarehouseItem>();
}

}
