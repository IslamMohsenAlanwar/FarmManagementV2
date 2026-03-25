namespace FarmManagement.API.DTOs
{
    public class FeedConsumptionBatchDto
    {
        public int BreedId { get; set; }
        public List<WeekDto> Weeks { get; set; } = new List<WeekDto>();
    }

    public class WeekDto
    {
        public int WeekStart { get; set; }
        public int WeekEnd { get; set; }
        public decimal TargetPerBirdGram { get; set; }
    }
}