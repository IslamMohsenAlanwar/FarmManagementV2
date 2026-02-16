namespace FarmManagement.API.DTOs
{
    public class AssetItemDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    public class CreateAssetItemDto
    {
        public required string Name { get; set; }
    }

    public class UpdateAssetItemDto
    {
        public required string Name { get; set; }
    }
}
