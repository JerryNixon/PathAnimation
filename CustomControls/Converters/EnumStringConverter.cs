using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Data;

namespace CustomControls.Converters
{
    public sealed class EnumStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            if (value is Enum)
            {
                var name = Enum.GetName(value.GetType(), value);
                return name;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            string language)
        {
            throw new NotImplementedException();
        }
    }
}