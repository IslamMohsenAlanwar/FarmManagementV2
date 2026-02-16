namespace FarmManagement.API.DTOs
{

public class CreateEggProductionDto
{
    public int FarmId { get; set; }
    public int BarnId { get; set; }       
    public int CycleId { get; set; }
    public DateTime Date { get; set; }
    public int CartonsCount { get; set; }
    public string? Notes { get; set; }
}

    public class EggProductionRecordDto
    {
        public int Id { get; set; }
        public int FarmId { get; set; }
        public int BarnId { get; set; }
        public int CycleId { get; set; }
        public DateTime Date { get; set; }
        public int CartonsCount { get; set; }
        public int TotalEggs { get; set; }
        public int LiveBirdsCount { get; set; }
        public decimal ProductionRate { get; set; }
        public string? Notes { get; set; }
    }
}