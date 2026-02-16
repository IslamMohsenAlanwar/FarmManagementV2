using FarmManagement.API.Models;

namespace FarmManagement.API.DTOs
{
    public class ItemUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal PricePerTon { get; set; }
        public ItemType ItemType { get; set; }
    }
}
