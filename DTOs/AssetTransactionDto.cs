namespace FarmManagement.API.DTOs
{
    public class AssetTransactionDto
    {
        public int AssetWarehouseItemId { get; set; } 
        public int BarnId { get; set; }               
        public decimal Quantity { get; set; }    
        public DateTime Date { get; set; } = DateTime.Now;
    }
public class AssetTransactionResponseDto
{
    public int Id { get; set; }
    public int AssetWarehouseItemId { get; set; }
    public int AssetItemId { get; set; }
    public string AssetItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal InBarnsQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public int BarnId { get; set; }
    public string BarnName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

}
