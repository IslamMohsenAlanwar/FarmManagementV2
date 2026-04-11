namespace FarmManagement.API.DTOs
{
    public class EggProductionDetailDto
    {
        public string EggQuality { get; set; } = string.Empty;
        public decimal CartonsCount { get; set; }
        public decimal TotalEggs { get; set; }
    }
}