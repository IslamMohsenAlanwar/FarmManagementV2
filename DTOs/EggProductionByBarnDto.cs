namespace FarmManagement.API.DTOs
{
public class EggProductionByBarnDto
{
    public string BarnName { get; set; } = string.Empty;   
    public string ItemName { get; set; } = string.Empty;   
    public int Quantity { get; set; }                      
    public string Day { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
}