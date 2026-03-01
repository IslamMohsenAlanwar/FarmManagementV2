using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
public class CashBoxTransaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    public string Type { get; set; } = string.Empty;// "Income" أو "Expense"
    public string Category { get; set; } = string.Empty; //  "EggSale", "ChickenSale", "Deposit", "Purchase", "Salary", "Advance", "Other"

    public decimal Amount { get; set; }
    public string? Notes { get; set; }

    // لو العملية لها طرف ثاني
    public int? TraderId { get; set; } 
    public Trader? Trader { get; set; }

    public int? WorkerId { get; set; }
    public Worker? Worker { get; set; }

    public int? WarehouseId { get; set; }
}
}
