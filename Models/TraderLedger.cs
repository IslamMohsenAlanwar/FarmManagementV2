using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
public class TraderLedger
{
    public int Id { get; set; }
    public int TraderId { get; set; }
    public DateTime Date { get; set; }

    public decimal Debit { get; set; }   // الفاتورة المستحقة
    public decimal Credit { get; set; }  // المدفوع للمورد
    public decimal Balance { get; set; } // رصيد المورد بعد الحركة

    public string Notes { get; set; } = string.Empty;
}
}
