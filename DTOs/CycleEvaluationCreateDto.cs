namespace FarmManagement.API.DTOs
{
public class CycleEvaluationCreateDto
{
    public int CycleId { get; set; }
    public List<EvaluationScoreDto> Scores { get; set; } = new();
}

public class EvaluationScoreDto
{
    public int EvaluationItemId { get; set; }
    public int Score { get; set; }
}

}
