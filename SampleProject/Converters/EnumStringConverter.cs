using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Data;

namespace SampleProject.Converters
{
    internal sealed class EnumStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;
         
            
            if (value is Enum)
            {
                var name = Enum.GetName(value.GetType(), value);
                return name + "";
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            string language)
        {
            if (value == null)
                return null;

            if (targetType == null || !targetType.GetTypeInfo().IsEnum)
                return null;

            var values = Enum.GetValues(targetType).Cast<Enum>().ToList();
            foreach (var v in values)
            {
                if (v.ToString() == value.ToString())
                    return v;
            }

            if (value is int)
            {
                return values[(int) value];
            }

            return values.FirstOrDefault();
        }
    }
}
