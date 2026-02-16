using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
    public class FeedMix
    {
        public int Id { get; set; }

        public int FeedTypeId { get; set; }
        public FeedType FeedType { get; set; } = null!;

        public decimal TotalWeight { get; set; } = 0;

        public decimal TotalPrice { get; set; } = 0;

        public decimal Quantity { get; set; } = 0;

        public decimal PricePerKg
        {
            get
            {
                return TotalWeight > 0 ? TotalPrice / TotalWeight : 0;
            }
        }

        public ICollection<FeedMixDetail> Details { get; set; } = new List<FeedMixDetail>();
    }
}
