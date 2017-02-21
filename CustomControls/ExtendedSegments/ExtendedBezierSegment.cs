using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.ExtendedSegments
{
    /// <summary>
    /// Extended bezier segment
    /// </summary>
    /// <seealso cref="CustomControls.ExtendedSegments.ExtendedBezierBase" />
    class ExtendedBezierSegment : ExtendedBezierBase
    {
        public ExtendedBezierSegment(PathSegment segment, Point startPoint) : base(segment, startPoint)
        {
            if (!(segment is BezierSegment))
                throw new ArgumentException("BezierSegment segment expected on ExtendedBezierSegment constructor");
        }

        protected override Point BezierFormula(double t)
        {
            BezierSegment s = (BezierSegment)Segment;
            Point r = new Point();
            Point p0 = StartPoint;
            r.X = Math.Pow(1 - t, 3) * p0.X + 3 * Math.Pow(1 - t, 2) * t * s.Point1.X + 3 * (1 - t) * t * t * s.Point2.X + t * t * t * s.Point3.X;
            r.Y = Math.Pow(1 - t, 3) * p0.Y + 3 * Math.Pow(1 - t, 2) * t * s.Point1.Y + 3 * (1 - t) * t * t * s.Point2.Y + t * t * t * s.Point3.Y;
            return r;
        }

        protected override double BezierDerivative(double t)
        {
            BezierSegment s = (BezierSegment)Segment;
            Point r = new Point();
            Point p0 = StartPoint;
            Point p1, p2, p3;
            p1 = s.Point1;
            p2 = s.Point2;
            p3 = s.Point3;
            r.X = 3 * (p1.X - p0.X) * Math.Pow(1 - t, 2) + 3 * (p2.X - p1.X) * 2 * t * (1 - t) + 3 * (p3.X - p2.X) * t * t;
            r.Y = 3 * (p1.Y - p0.Y) * Math.Pow(1 - t, 2) + 3 * (p2.Y - p1.Y) * 2 * t * (1 - t) + 3 * (p3.Y - p2.Y) * t * t;
            return r.Y / r.X;
        }
    }
}
