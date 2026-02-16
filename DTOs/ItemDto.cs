using FarmManagement.API.Models;

namespace FarmManagement.API.DTOs
{
    public class ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PricePerTon { get; set; }
        public ItemType ItemType { get; set; }
    }
}
