using System.ComponentModel.DataAnnotations;
using FarmManagement.API.Enums;

namespace FarmManagement.API.Models
{
public class CashBoxTransaction
{
    public int Id { get; set; }
    public DateTime Date { get; set; }

    public CashBoxType Type { get; set; } 
    public CashBoxCategory Category { get; set; } 

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
