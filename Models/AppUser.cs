
namespace FarmManagement.API.Models
{
public class AppUser
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserType UserType { get; set; }
}
}
