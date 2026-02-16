using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
    public class FeedMixDetail
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        [Required]
        public decimal Quantity { get; set; }

        public decimal Price { get; set; } = 0;

        public int FeedMixId { get; set; }
        public FeedMix FeedMix { get; set; } = null!;
    }
}
