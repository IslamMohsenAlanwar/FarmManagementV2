using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
    public class EggProductionSetting
{
    public int Id { get; set; }

    public int BreedId { get; set; }

    public int WeekStart { get; set; }
    public int WeekEnd { get; set; }

    // متوسط عدد البيض المستهدف للطائر في اليوم
    public decimal TargetProductionPercent { get; set; }
    public decimal TargetPerBird { get; set; }

}
}
