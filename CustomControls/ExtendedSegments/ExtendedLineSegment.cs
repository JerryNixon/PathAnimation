using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.ExtendedSegments
{
    /// <summary>
    /// Extended line segment
    /// </summary>
    /// <seealso cref="CustomControls.ExtendedSegments.ExtendedSegmentBase" />
    class ExtendedLineSegment : ExtendedSegmentBase
    {
        public ExtendedLineSegment(PathSegment segment, Point startPoint) : base(segment, startPoint)
        {
            if (!(segment is LineSegment))
                throw new ArgumentException("LineSegment segment expected on ExtendedLineSegment constructor");
        }

        protected override Point GetEndPoint()
        {
            return ((LineSegment)Segment).Point;
        }

        protected override double GetSegmentLength()
        {
            return GetDistanceBetweenPoints(StartPoint, EndPoint);
        }

        public override double GetDegreesAt(double percent)
        {
            if (EndPoint.X.Equals(StartPoint.X))
            {
                return EndPoint.Y >= StartPoint.Y ? 90 : -90;
            }

            var dt = (EndPoint.Y - StartPoint.Y) / (EndPoint.X - StartPoint.X);
            return (Math.Atan(dt) * (180 / Math.PI));
        }

        public override double GetOrientedDegreesAt(double percent)
        {
            return GetOrientedDegrees(StartPoint, EndPoint);
        }

        public override Point GetPointAt(double percent)
        {
            return new Point
            {
                X = StartPoint.X + percent * (EndPoint.X - StartPoint.X),
                Y = StartPoint.Y + percent * (EndPoint.Y - StartPoint.Y)
            };
        }

        /// <summary>
        /// Gets oriented degrees by using the vector that is described by provided 2 points
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <returns></returns>
        public static double GetOrientedDegrees(Point start, Point end)
        {
            end.X = end.X - start.X;
            start.X = start.X - start.X;

            end.Y = end.Y - start.Y;
            start.Y = start.Y - start.Y;

            double dt;

            if (end.Y != 0)
            {
                dt = end.Y / end.X;
            }
            else
            {
                if (end.X > 0)
                    return 0;
                return 180;
            }

            var res = (Math.Atan(dt) * (180 / Math.PI));
            if (end.X >= 0)
            {
                return res;
            }
            else
            {
                return res + 180;
            }
        }
    }
}
