namespace FarmManagement.API.DTOs
{
    public class DailyRecordUpdateDto
    {
        public int CycleId { get; set; }   // عشان نجيب آخر سجل

        public int DeadCount { get; set; }

        public int WarehouseId { get; set; }

        public List<FeedConsumptionInputDto> FeedConsumptions { get; set; } = new();

        public List<MedicineConsumptionInputDto> MedicineConsumptions { get; set; } = new();
    }

    public class FeedConsumptionInputDto
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class MedicineConsumptionInputDto
    {
        public int ItemId { get; set; }
        public decimal Quantity { get; set; }
    }
}