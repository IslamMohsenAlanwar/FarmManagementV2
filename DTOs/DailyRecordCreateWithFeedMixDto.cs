namespace FarmManagement.API.DTOs
{
    public class DailyRecordCreateWithMixDto
    {
        public int CycleId { get; set; }
        public DateTime Date { get; set; }

        public int DeadCount { get; set; }

        public decimal? FeedConsumed { get; set; }

        public decimal? MedicineConsumed { get; set; }

        public int? FeedMixId { get; set; }
        public decimal? FeedMixQuantity { get; set; }
    }
}
