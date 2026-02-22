using System;
using System.Collections.Generic;

namespace FarmManagement.API.Models
{
    public class EggProductionRecord
    {
        public int Id { get; set; }

        public int FarmId { get; set; }
        public Farm Farm { get; set; } = null!;

        public int BarnId { get; set; }
        public Barn Barn { get; set; } = null!;

        public int CycleId { get; set; }
        public Cycle Cycle { get; set; } = null!;

        public DateTime Date { get; set; }

        public int LiveBirdsCount { get; set; }

        public int TotalEggs { get; set; }

        public decimal ProductionRate { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<EggProductionDetail> Details { get; set; } = new List<EggProductionDetail>();
    }
}