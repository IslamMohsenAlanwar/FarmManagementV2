namespace FarmManagement.API.DTOs
{
    public class DailyRecordCreateDto
    {
        public int CycleId { get; set; }
        public int DeadCount { get; set; }

        public List<FeedConsumptionCreateDto> FeedConsumptions { get; set; } = new List<FeedConsumptionCreateDto>();
        public List<MedicineConsumptionCreateDto> MedicineConsumptions { get; set; } = new List<MedicineConsumptionCreateDto>();
    }

    public class FeedConsumptionCreateDto
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class MedicineConsumptionCreateDto
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
    }
}
