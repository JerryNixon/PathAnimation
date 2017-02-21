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
        /// <summary>
        /// Determines whether this instance is nan.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        ///   <c>true</c> if the specified point is nan; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNan(this Point point)
        {
            return double.IsNaN(point.X) || double.IsNaN(point.Y);
        }

        /// <summary>
        /// Converts a path geometry to an extended path geometry
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public static ExtendedPathGeometry ToExtendedPathGeometry(this PathGeometry geometry)
        {
            return new ExtendedPathGeometry(geometry);
        }

        /// <summary>
        /// Converts a path geometry to a layout path control
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public static Controls.LayoutPath ToLayoutPath(this PathGeometry geometry)
        {
            return new Controls.LayoutPath() { Path = geometry };
        }

        /// <summary>
        /// Converts a geometry to short path string representation
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns></returns>
        public static string ToPathGeometryString(this Geometry geometry)
        {
            if (geometry is PathGeometry)
                return new StringToPathGeometryConverter().ConvertBack((PathGeometry)geometry);
            return null;
        }
    }
}
