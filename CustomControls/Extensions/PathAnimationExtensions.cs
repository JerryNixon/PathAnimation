using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using CustomControls.Converters;
using CustomControls.ExtendedSegments;

namespace CustomControls.Extensions
{
    public static class PathAnimationExtensions
    {
        public static bool IsNan(this Point point)
        {
            return double.IsNaN(point.X) || double.IsNaN(point.Y);
        }

        public static ExtendedPathGeometry ToExtendedPathGeometry(this PathGeometry geometry)
        {
            return new ExtendedPathGeometry(geometry);
        }

        public static Controls.LayoutPath ToLayoutPath(this PathGeometry geometry)
        {
            return new Controls.LayoutPath() { Path = geometry };
        }

        public static string ToPathGeometryString(this Geometry geometry)
        {
            if (geometry is PathGeometry)
                return new StringToPathGeometryConverter().ConvertBack((PathGeometry)geometry);
            return null;
        }
    }
}
