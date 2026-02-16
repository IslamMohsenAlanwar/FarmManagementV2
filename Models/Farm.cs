namespace FarmManagement.API.Models
{
    public class Farm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<Barn> Barns { get; set; } = new List<Barn>();

        public Warehouse? Warehouse { get; set; }

        public AssetWarehouse AssetWarehouse { get; set; } = null!;

        public ICollection<Cycle> Cycles { get; set; } = new List<Cycle>();
    }
}
