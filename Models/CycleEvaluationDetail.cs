using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
public class CycleEvaluationDetail
{
    public int Id { get; set; }

    public int CycleEvaluationId { get; set; }
    public CycleEvaluation CycleEvaluation { get; set; } = null!;

    public int EvaluationItemId { get; set; }
    public EvaluationItem EvaluationItem { get; set; } = null!;

    public int Score { get; set; } // من 0 لـ MaxScore
}
}
