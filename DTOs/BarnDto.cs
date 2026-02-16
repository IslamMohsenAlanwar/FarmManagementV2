namespace FarmManagement.API.DTOs
{
    public class BarnDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;

            public int Type { get; set; }          // 1 Or 2
    public string TypeName { get; set; } = string.Empty; // بياض / تسمين
    }
}
