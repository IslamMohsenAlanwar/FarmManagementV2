using FarmManagement.API.Models;

namespace FarmManagement.API.DTOs
{
    public class CreateEggProductionDetailDto
    {
        public EggQualityType EggQuality { get; set; }
        public decimal CartonsCount { get; set; }
    }
}