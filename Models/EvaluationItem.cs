using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
public class EvaluationItem
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int MaxScore { get; set; } = 10;
}
}
