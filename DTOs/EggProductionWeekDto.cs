namespace FarmManagement.API.DTOs
{
    public class EggProductionWeekDto
    {
        public int WeekStart { get; set; }
        public int WeekEnd { get; set; }
        public decimal TargetProductionPercent { get; set; } // نسبة الإنتاج لكل أسبوع
    }

    public class EggProductionBatchDto
    {
        public int BreedId { get; set; }
        public List<EggProductionWeekDto> Weeks { get; set; } = new List<EggProductionWeekDto>();
    }
}