namespace FarmManagement.API.DTOs
{
public class PayTraderDto
{
    public int TraderId { get; set; }
    public decimal Amount { get; set; }  // المبلغ اللي هيتدفع دلوقتي
    public DateTime? Date { get; set; }  // اختياري، افتراضي اليوم
    public string Notes { get; set; } = string.Empty;   // ملاحظات إضافية
    public int? WarehouseId { get; set; } // لو مرتبط بخزنة معينة
}

}
