using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SampleProject.Converters
{
    public class HeightToPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Point res = new Point(0, 0);
            if (parameter is int)
                res.X = (int)parameter;
            if (value is FrameworkElement)
            {
                var val = (FrameworkElement)value;
                    res.Y = -(double)val.ActualHeight;
            }
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
