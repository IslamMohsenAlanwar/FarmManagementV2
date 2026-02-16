namespace FarmManagement.API.Models
{
public class AssetTransaction
{
    public int Id { get; set; }

    public int AssetWarehouseItemId { get; set; }
    public AssetWarehouseItem AssetWarehouseItem { get; set; } = null!;

    public int TargetBarnId { get; set; }
    public Barn? TargetBarn { get; set; }

    public decimal Quantity { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Withdraw / Deposit
    public DateTime Date { get; set; } = DateTime.Now;
}


}
