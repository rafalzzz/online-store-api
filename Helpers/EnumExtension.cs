using System.ComponentModel;
using System.Reflection;

namespace OnlineStoreAPI.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            string description = null;

            if (e is Enum)
            {
                FieldInfo fieldInfo = e.GetType().GetField(e.ToString());

                if (fieldInfo != null)
                {
                    var attributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                    if (attributes.Any())
                    {
                        description = (attributes.First() as DescriptionAttribute)?.Description;
                    }
                }
            }

            return description ?? e.ToString();
        }
    }
}