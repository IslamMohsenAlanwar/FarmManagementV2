namespace FarmManagement.API.DTOs
{
public class PayTraderDto
{
    public int TraderId { get; set; }
    public decimal Amount { get; set; }  
    public DateTime? Date { get; set; }  
    public string Notes { get; set; } = "دفع لحساب مورد";
    public int? WarehouseId { get; set; }
}

}
