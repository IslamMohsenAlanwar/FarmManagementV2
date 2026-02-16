namespace FarmManagement.API.DTOs
{
    public class BarnUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public int FarmId { get; set; }
        public int Type { get; set; } // 1 Or 2
    }
}
