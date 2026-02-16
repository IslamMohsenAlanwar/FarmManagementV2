namespace FarmManagement.API.Models
{
    public class FeedType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<FeedMix> FeedMixes { get; set; } = new List<FeedMix>();
    }
}
