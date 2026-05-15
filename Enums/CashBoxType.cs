using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Enums
{
    public enum CashBoxCategory
    {
        [Display(Name = "بيع بيض")]
        EggSale,

        [Display(Name = "بيع فراخ")]
        ChickenSale,

        [Display(Name = "رواتب")]
        Salary,

        [Display(Name = "سلف")]
        Advance,

        [Display(Name = "شراء خامات/أدوية")]
        Purchase, // (خامات + أدوية + أعلاف)

        [Display(Name = "أخرى")]
        Other
    }

    public enum CashBoxType
    {
        [Display(Name = "إيراد")]
        Income,

        [Display(Name = "منصرف")]
        Expense
    }
}