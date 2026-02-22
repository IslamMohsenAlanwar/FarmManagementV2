using System;
using System.Collections.Generic;

namespace FarmManagement.API.DTOs
{
    public class EggProductionRecordDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalEggs { get; set; }
        public int LiveBirdsCount { get; set; }
        public decimal ProductionRate { get; set; }
        public string? Notes { get; set; }

        public List<EggProductionDetailDto> Details { get; set; } = new();
    }
}