using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SampleProject.Converters
{
    class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Convert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Convert(value);
        }

        private object Convert(object o)
        {
            if (o is bool)
                return ((bool)o) ? Visibility.Visible : Visibility.Collapsed;
            if (o is Visibility)
            {
                return ((Visibility)o) == Visibility.Visible;
            }
            return o;
        }
    }
}
