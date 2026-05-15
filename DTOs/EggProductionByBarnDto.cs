using System;

namespace FarmManagement.API.DTOs
{
    public class EggProductionByBarnDto
    {
        public string BarnName { get; set; } = string.Empty;
        public string CycleName { get; set; } = string.Empty;
        public int CycleId { get; set; }
        public int ChickAge { get; set; }
        public string EggQuality { get; set; } = string.Empty;
        public decimal CartonsCount { get; set; }
        public decimal TotalEggs { get; set; }
        public string Day { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class CycleEggSummaryDto
    {
        public int CycleId { get; set; }
        public string CycleName { get; set; } = string.Empty;

        public decimal TotalCartons { get; set; }
        public decimal NormalEggs { get; set; }   // سليم
        public decimal BrokenEggs { get; set; }   // كسر
        public decimal DoubleEggs { get; set; }   // دبل
        public decimal FarzaEggs { get; set; }   // فرزة
    }
}