namespace FarmManagement.API.Models
{
    public enum ItemType
    {
        RawMaterial = 1,   // أصناف خام
        Medicine = 2,      // أدوية
        FeedMix = 3,       // خلطة علف (يستخدمها النظام)
        Egg = 4            // كرتونة بيض
    }

    public class Item
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal PricePerTon { get; set; } = 0;

        public ItemType ItemType { get; set; } = ItemType.RawMaterial;

        public ICollection<WarehouseItem> WarehouseItems { get; set; } = new List<WarehouseItem>();
    }
}
