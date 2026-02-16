namespace FarmManagement.API.DTOs
{
    public class CycleDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;

        public int BarnId { get; set; }
        public string BarnName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int ChickCount { get; set; }
        public int ChickAge { get; set; }
    }
}
