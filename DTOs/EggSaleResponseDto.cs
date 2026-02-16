namespace FarmManagement.API.DTOs
{
public class EggSaleResponseDto
{
    public int Id { get; set; }
    public string TraderName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal CumulativeBalance { get; set; }
}
}