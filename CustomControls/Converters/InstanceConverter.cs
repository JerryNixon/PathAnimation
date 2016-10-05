using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace CustomControls.Converters
{
    internal class InstanceConverter : IValueConverter
    {
        private Func<object, object> _convert;

        public InstanceConverter(Func<object, object> converter)
        {
            _convert = converter;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return _convert.Invoke(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
