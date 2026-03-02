namespace FarmManagement.API.DTOs
{
    public class DailySummaryDto
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = "";
        public string CycleName { get; set; } = "";
        public int ChickAge { get; set; }

        // إنتاج البيض
        public int EggsGood { get; set; }
        public int EggsBroken { get; set; }
        public int EggsDouble { get; set; }
        public int EggsTotal { get; set; }

        // النافق والمستهلك
        public int DeadCount { get; set; }
        public int FeedConsumed { get; set; }

        // مبيعات البيض
        public decimal EggsSold { get; set; }
    }
}