using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
    public class Breed
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
