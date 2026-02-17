namespace FarmManagement.API.Models
{
public class Vacation
{
    public int Id { get; set; }
    public int WorkerId { get; set; }
    public Worker? Worker { get; set; }   // relation

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Days { get; set; }        // عدد الأيام
}

}