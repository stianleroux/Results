namespace Results.Extensions;

using System.ComponentModel;

public static class EnumExtensions
{
    extension(Enum value)
    {
        public string GetDescription()
        {
            var field = value.GetType().GetField(value.ToString());
            if (field is null)
            {
                return value.ToString();
            }

            var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
            return attributes is not null && attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }
    }
}