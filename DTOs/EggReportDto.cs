using FarmManagement.API.Models;

namespace FarmManagement.API.DTOs
{
    public class EggReportDto
    {
        public int ChickAge { get; set; }

        // ===== عدد الأطباق الفعلية =====
        public decimal BrokenCartons { get; set; }
        public decimal DoubleCartons { get; set; }
        public decimal NormalCartons { get; set; }
        public decimal FarzaCartons { get; set; }
        public decimal TotalActualCartons { get; set; }

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