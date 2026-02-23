namespace FarmManagement.API.DTOs
{
public class CycleCreateDto
{
    public string Name { get; set; } = string.Empty;

    public int FarmId { get; set; }
    public int BarnId { get; set; }

    public int? BarnManagerId { get; set; }
    public int? BarnWorkerId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int ChickCount { get; set; }
    public int ChickAge { get; set; }
}
}
