namespace FarmManagement.API.DTOs
{
    public class FeedMixDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public decimal TotalWeight { get; set; }
        public decimal TotalPrice { get; set; }

        public List<FeedMixDetailDto> Items { get; set; } = new();
    }

    public class FeedMixDetailDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }
}
