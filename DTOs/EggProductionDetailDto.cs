namespace FarmManagement.API.DTOs
{
    public class EggProductionDetailDto
    {
        public string EggQuality { get; set; } = string.Empty;
        public int CartonsCount { get; set; }
        public int TotalEggs { get; set; }
    }
}