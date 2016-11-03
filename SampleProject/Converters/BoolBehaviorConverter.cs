using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using CustomControls.Enums;

namespace SampleProject.Converters
{
    class BoolBehaviorConverter : IValueConverter
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
                return ((bool)o) ? Behaviors.Stack : Behaviors.Default;
            if (o is Behaviors)
            {
                return ((Behaviors)o) == Behaviors.Stack;
            }
            return o;
        }
    }
}
