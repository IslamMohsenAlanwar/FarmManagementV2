using FarmManagement.API.Models;

namespace FarmManagement.API.DTOs
{
    public class TraderDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;

        public TraderType Type { get; set; }      //  (1 Or 2)
        public string TypeName { get; set; } = ""; // name (Supplier / Buyer)

        public decimal Balance { get; set; }
    }
}
