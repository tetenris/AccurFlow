using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace AccuFlow.Entities.Enums.Extensions
{
    public static class EnumExtention
    {
        public static string GetDescription<T>(this T enumValue) where T : Enum
        {
            if (!typeof(T).IsEnum)
                return null;

            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }

        public static List<SelectListItem> ToSelectList<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Select(e => new SelectListItem
                {
                    Value = e.GetHashCode().ToString(),
                    Text = e.GetDescription()
                })
                .ToList();
        }
    }
}
