using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.ExtendedSegments
{
    /// <summary>
    /// Extended quadratic bezier segment
    /// </summary>
    /// <seealso cref="CustomControls.ExtendedSegments.ExtendedBezierBase" />
    class ExtendedQuadraticBezierSegment : ExtendedBezierBase
    {
        public ExtendedQuadraticBezierSegment(PathSegment segment, Point startPoint) : base(segment, startPoint)
        {
            if (!(segment is QuadraticBezierSegment))
                throw new ArgumentException();
        }
     
        protected override Point BezierFormula(double t)
        {
            QuadraticBezierSegment s = (QuadraticBezierSegment) Segment;
            Point r = new Point();
            r.X = (1 - t) * (1 - t) * StartPoint.X + 2 * (1 - t) * t * s.Point1.X + t * t * s.Point2.X;
            r.Y = (1 - t) * (1 - t) * StartPoint.Y + 2 * (1 - t) * t * s.Point1.Y + t * t * s.Point2.Y;
            return r;
        }

        protected override double BezierDerivative(double t)
        {
            QuadraticBezierSegment s = (QuadraticBezierSegment)Segment;
            Point r = new Point();
            Point p0 = StartPoint;
            Point p1, p2;
            p1 = s.Point1;
            p2 = s.Point2;
            r.X = 2 * (1 - t) * (p1.X - p0.X) + 2 * t * (p2.X - p1.X);
            r.Y = 2 * (1 - t) * (p1.Y - p0.Y) + 2 * t * (p2.Y - p1.Y);
            return r.Y / r.X;
        }



    }
}
