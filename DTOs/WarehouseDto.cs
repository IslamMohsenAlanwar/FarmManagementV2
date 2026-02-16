namespace FarmManagement.API.DTOs
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;
    }

}
