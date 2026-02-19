namespace FarmManagement.API.Models
{
    public class Cycle
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int FarmId { get; set; }
        public Farm Farm { get; set; } = null!;

        public int BarnId { get; set; }
        public Barn Barn { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int ChickCount { get; set; }       
        public int ChickAge { get; set; }         
        public int TotalMortality { get; set; }   
        public int TotalCulled { get; set; }   

        public ICollection<DailyRecord> DailyRecords { get; set; } = new List<DailyRecord>();

        public ICollection<EggProductionRecord> EggProductionRecords { get; set; } = new List<EggProductionRecord>();

public ICollection<ChickenSale> ChickenSales { get; set; } = new List<ChickenSale>();

    }
}
