namespace Results.Extensions;

using System.ComponentModel;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());

        if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Length != 0)
        {
            return attributes.First().Description;
        }

        return value.ToString();
    }
}
