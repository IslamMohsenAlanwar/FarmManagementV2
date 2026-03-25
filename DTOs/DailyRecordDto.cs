namespace FarmManagement.API.DTOs
{
    public class DailyRecordDto
    {
        public int Id { get; set; }
        public int CycleId { get; set; }
        public DateTime Date { get; set; }
        public int DayNumber { get; set; }
            public string DayName { get; set; } = "";  

        public int ChickAge { get; set; }
        public int DeadCount { get; set; }
        public int DeadCumulative { get; set; }
        public int RemainingChicks { get; set; }
        public decimal MortalityRate { get; set; }
        public decimal TargetMortalityRate { get; set; }
        
        public decimal FeedPerBirdGram { get; set; }
        public decimal CumulativeFeedPerBirdKg { get; set; }
        public decimal FeedPerHouseTon { get; set; }
        public decimal CumulativeFeedHouseTon { get; set; }
        public decimal FeedTargetPerBirdGram { get; set; }
        public decimal FeedAchievementPercent { get; set; }
        public string FeedStatus { get; set; } = string.Empty;

        public List<FeedConsumptionDto> FeedConsumptions { get; set; } = new List<FeedConsumptionDto>();
        public List<MedicineConsumptionDto> MedicineConsumptions { get; set; } = new List<MedicineConsumptionDto>();
    }

    public class FeedConsumptionDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
    }

    public class MedicineConsumptionDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
    }

    
}
