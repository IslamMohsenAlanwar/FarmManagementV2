using FarmManagement.API.Models;

namespace FarmManagement.API.DTOs
{
    public class ItemCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal PricePerTon { get; set; }
        public ItemType ItemType { get; set; }   // 1 = أصناف خام ، 2 = أدوية
    }
}
