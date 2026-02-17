namespace FarmManagement.API.Models
{
public class Advance
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public Worker? Worker { get; set; }

    public decimal Amount { get; set; }      // قيمة السلفة
    public DateTime Date { get; set; }       // تاريخ السلفة
}
}
