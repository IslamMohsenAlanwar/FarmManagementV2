namespace FarmManagement.API.DTOs
{
    public class TargetMortalityWeekInputDto
    {
        public int WeekStart { get; set; }
        public int WeekEnd { get; set; }
        public decimal ExpectedMortalityRate { get; set; }
    }

    public class TargetMortalityInputDto
    {
        public int BreedId { get; set; }
        public List<TargetMortalityWeekInputDto> Weeks { get; set; } = new List<TargetMortalityWeekInputDto>();
    }


    public class TargetMortalityWeekDto
    {
        public int WeekStart { get; set; }
        public int WeekEnd { get; set; }
        public decimal ExpectedMortalityRate { get; set; }
    }
}