using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Models
{
    public class TargetMortalitySetting
    {
        public int Id { get; set; }

  
        public int BreedId { get; set; }
        public Breed Breed { get; set; } = null!;

        public int WeekStart { get; set; }
        public int WeekEnd { get; set; }

       
        public decimal ExpectedMortalityRate { get; set; }
    }
}
