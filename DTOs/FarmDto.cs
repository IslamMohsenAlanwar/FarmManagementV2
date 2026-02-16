namespace FarmManagement.API.DTOs
{
    public class FarmDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
        
        public List<string> BarnNames { get; set; } = new();
    }
}
