using FarmManagement.API.Models;

namespace FarmManagement.API.DTOs
{
    public class EggReportDto
    {
        public int ChickAge { get; set; }

        // ===== عدد الأطباق الفعلية =====
        public int BrokenCartons { get; set; }
        public int DoubleCartons { get; set; }
        public int NormalCartons { get; set; }
        public int TotalActualCartons { get; set; }

        // ===== أطباق البيض المستهدفة =====
        public decimal TargetCartons { get; set; }

        // ===== النسب % =====
        public decimal ActualPercent { get; set; }       // الإنتاج الفعلي %
        public decimal TargetPercent { get; set; }       // الإنتاج المستهدف %
        public decimal AchievementPercent { get; set; }  // نسبة التحقيق %

        // ===== متوسط انتاج البيض لكل طائر =====
        public decimal TargetPerBird { get; set; }
        public decimal ActualPerBird { get; set; }
        public decimal AchievementPerBird { get; set; }

        // ===== التراكمية =====
        public decimal CumulativeActual { get; set; }
        public decimal CumulativeTarget { get; set; }
        public decimal CumulativeAchievement { get; set; }
    }
}