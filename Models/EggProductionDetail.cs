namespace FarmManagement.API.Models
{
    public class EggProductionDetail
    {
        public int Id { get; set; }

        public int EggProductionRecordId { get; set; }
        public EggProductionRecord EggProductionRecord { get; set; } = null!;

        public EggQualityType EggQuality { get; set; }

        public decimal CartonsCount { get; set; }

        public decimal TotalEggs { get; set; }
    }
}