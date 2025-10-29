using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MetaTune.Helpers
{
    public class EnumDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            var enumType = value.GetType();
            var enumName = Enum.GetName(enumType, value);

            if (enumName != null)
            {
                var field = enumType.GetField(enumName);
                if (field != null)
                {
                    if (field.GetCustomAttributes(typeof(DisplayAttribute), false)
                                           .FirstOrDefault() is DisplayAttribute displayAttr && displayAttr.Name != null)
                        return displayAttr.Name;
                }
            }

            // fallback if no attribute
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing; // not needed for ComboBox selection
        }
    }
}
