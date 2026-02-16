using FarmManagement.API.Models;
namespace FarmManagement.API.DTOs
{
public class TraderCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public TraderType Type { get; set; } 
    public decimal Balance { get; set; } = 0;
}

}