using FarmManagement.API.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Linq;
public static class EnumHelper
{
    public static EnumDto ToEnumResponse<T>(T enumValue) where T : Enum
    {
        return new EnumDto
        {
            Id = Convert.ToInt32(enumValue),
            Name = enumValue.ToString(),
            DisplayName = GetDisplayName(enumValue)
        };
    }

    private static string GetDisplayName(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        var attribute = field?
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        return attribute?.Name ?? value.ToString();
    }
}