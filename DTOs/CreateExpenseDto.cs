namespace FarmManagement.API.DTOs
{
    public class CreateExpenseDto
    {
        public decimal Amount { get; set; }      // قيمة المصروف
        public string? Notes { get; set; }  // سبب المصروف
        public DateTime? Date { get; set; }      // اختياري، لو مش موجود = DateTime.Now
    }

    public class CreateIncomeDto
    {
        public decimal Amount { get; set; }      // قيمة الإيراد
        public string? Notes { get; set; }  // سبب الإيراد
        public DateTime? Date { get; set; }      // اختياري، لو مش موجود = DateTime.Now
    }

}