using System;

namespace FarmManagement.API.DTOs
{
    public class EggProductionByBarnDto
    {
        public string BarnName { get; set; } = string.Empty;
        public string EggQuality { get; set; } = string.Empty;
        public decimal CartonsCount { get; set; }
        public decimal TotalEggs { get; set; }
        public string Day { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}