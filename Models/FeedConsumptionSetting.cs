namespace FarmManagement.API.Models
{
    public class FeedConsumptionSetting
{
    public int Id { get; set; }

    public int BreedId { get; set; }
    public Breed? Breed { get; set; }

    public int WeekStart { get; set; }
    public int WeekEnd { get; set; }

    public decimal TargetPerBirdGram { get; set; }
}
}
