namespace FarmManagement.API.DTOs
{
    public class DailyRecordUpdateDto
    {
        public int DeadCount { get; set; }
        public decimal FeedConsumed { get; set; }
        public decimal MedicineConsumed { get; set; }
    }
}
