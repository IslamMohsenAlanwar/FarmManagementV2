using FarmManagement.API.Models;

namespace FarmManagement.API.DTOs
{
public class ActiveCycleDto
{
    public int Id { get; set; }         
    public string CycleName { get; set; } = string.Empty;  
    public string BarnName { get; set; } = string.Empty;   
}

}
