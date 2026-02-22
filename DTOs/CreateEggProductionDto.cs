using System;
using System.Collections.Generic;

namespace FarmManagement.API.DTOs
{
    public class CreateEggProductionDto
    {
        public int FarmId { get; set; }
        public int BarnId { get; set; }
        public int CycleId { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }

        public List<CreateEggProductionDetailDto> Details { get; set; } = new();
    }
}