using System.Collections.Generic;

namespace FarmManagement.API.Models
{
  
    public enum BarnType
    {
        Layers = 1,   // بياض
        Broilers = 2  // تسمين
    }

    public class Barn
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // 1 = بياض ، 2 = تسمين
        public BarnType Type { get; set; }

        public int FarmId { get; set; }
        public Farm Farm { get; set; } = null!;

        public ICollection<Cycle> Cycles { get; set; } = new List<Cycle>();

        public ICollection<EggProductionRecord> EggProductionRecords { get; set; } = new List<EggProductionRecord>();
    }
}
