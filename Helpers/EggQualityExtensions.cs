using FarmManagement.API.Models;

namespace FarmManagement.API.Helpers
{
    public static class EggQualityExtensions
    {
        public static string ToArabic(this EggQualityType type)
        {
            return type switch
            {
                EggQualityType.Normal => "سليم",
                EggQualityType.Broken => "كسر",
                EggQualityType.Double => "دبل",
                _ => type.ToString()
            };
        }

        public static string? ToArabic(this EggQualityType? type)
        {
            if (type == null)
                return null;

            return type.Value.ToArabic();
        }
    }
}