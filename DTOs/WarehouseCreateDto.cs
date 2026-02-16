namespace FarmManagement.API.DTOs
{
    public class WarehouseCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public int FarmId { get; set; }
    }
}
