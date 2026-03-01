using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
public class Salary
{
    public int Id { get; set; }

    public int WorkerId { get; set; }
    public Worker Worker { get; set; } = null!;

    public int Month { get; set; }  
    public int Year { get; set; } 
    public decimal BaseSalary { get; set; }
    public decimal TotalAdvances { get; set; }
    public decimal NetSalary { get; set; }

    public DateTime Date { get; set; }
}
}
