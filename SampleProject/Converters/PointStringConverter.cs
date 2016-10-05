using System;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace SampleProject.Converters
{
    internal class PointStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var v = value.ToString().Split(',');
                if (v.Length == 2)
                    return new Point(double.Parse(v[0]), double.Parse(v[1]));
                return new Point(double.NaN, double.NaN);
            }
            catch (Exception)
            {
                return new Point(double.NaN, double.NaN);
            }
        }
    }
}
