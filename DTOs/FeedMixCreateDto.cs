namespace FarmManagement.API.DTOs
{
public class FeedMixCreateDto
{
    public int FeedTypeId { get; set; }  
    public int WarehouseId { get; set; }

    public List<FeedMixItemDto> Items { get; set; } = new();
}

public class FeedMixItemDto
{
    public int ItemId { get; set; }
    public decimal Quantity { get; set; }
}

}
