using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
public class FeedMixConsumption
{
    public int Id { get; set; }

    public int FeedMixId { get; set; }
    public FeedMix? FeedMix { get; set; }

    public int WarehouseId { get; set; }

    public int ItemId { get; set; }

    public decimal Quantity { get; set; }
}
}
