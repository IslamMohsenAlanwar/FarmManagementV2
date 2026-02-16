namespace FarmManagement.API.Models
{
    public class DailyRecord
    {
        public int Id { get; set; }

        public int CycleId { get; set; }
        public Cycle Cycle { get; set; } = null!;

        public DateTime Date { get; set; }
        public int DayNumber { get; set; }
        public int ChickAge { get; set; }

        public int DeadCount { get; set; }
        public int DeadCumulative { get; set; }
        public int RemainingChicks { get; set; }

        public ICollection<DailyFeedConsumption> FeedConsumptions { get; set; } = new List<DailyFeedConsumption>();
        public ICollection<DailyMedicineConsumption> MedicineConsumptions { get; set; } = new List<DailyMedicineConsumption>();
    }

    public class DailyFeedConsumption
    {
        public int Id { get; set; }

        public int DailyRecordId { get; set; }
        public DailyRecord DailyRecord { get; set; } = null!;

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
    }

    public class DailyMedicineConsumption
    {
        public int Id { get; set; }

        public int DailyRecordId { get; set; }
        public DailyRecord DailyRecord { get; set; } = null!;

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
    }
}
