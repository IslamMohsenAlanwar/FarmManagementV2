using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
public class CycleEvaluation
{
    public int Id { get; set; }

    public int CycleId { get; set; }
    public Cycle Cycle { get; set; } = null!;

    public DateTime Date { get; set; } = DateTime.Now;

    public ICollection<CycleEvaluationDetail> Details { get; set; } = new List<CycleEvaluationDetail>();
}
}
