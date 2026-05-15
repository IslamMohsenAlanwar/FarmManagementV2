namespace FarmManagement.API.DTOs
{
    public class DailySummaryDto
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = "";
        public string CycleName { get; set; } = "";
        public int ChickAge { get; set; }

        // إنتاج البيض
        public decimal EggsGood { get; set; }
        public decimal EggsBroken { get; set; }
        public decimal EggsDouble { get; set; }
        public decimal EggsFarza { get; set; }
        public decimal EggsTotal { get; set; }

        // النافق والمستهلك
        public int DeadCount { get; set; }
        public decimal  FeedConsumed { get; set; }

        // مبيعات البيض
        public decimal EggsSold { get; set; }
    }
}