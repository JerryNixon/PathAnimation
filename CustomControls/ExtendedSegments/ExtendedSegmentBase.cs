using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.ExtendedSegments
{
    abstract class ExtendedSegmentBase
    {
        #region private members
        private Point _endPoint = new Point(double.NaN, double.NaN);
        private double _segmentLength = double.NaN;
        #endregion

        #region abstract members
        public abstract Point GetPointAt(double percent);
        protected abstract Point GetEndPoint();
        protected abstract double GetSegmentLength();
        public abstract double GetDegreesAt(double percent);
        public abstract double GetOrientedDegreesAt(double percent);
        #endregion

        protected ExtendedSegmentBase(PathSegment segment, Point startPoint)
        {
            Segment = segment;
            StartPoint = startPoint;
        }

        public PathSegment Segment { get; protected set; }

        public Point StartPoint { get; private set; }

        public Point EndPoint
        {
            get
            {
                if (!double.IsNaN(_endPoint.X) && !double.IsNaN(_endPoint.Y))
                    return _endPoint;
                return _endPoint = GetEndPoint();
            }
        }

        public double DistanceFromStart { get; set; }
        public double StartsAtPercent { get; set; }
        public double EndsAtPercent { get; set; }

        public double SegmentLength
        {
            get
            {
                if (!double.IsNaN(_segmentLength))
                    return _segmentLength;
                return _segmentLength = GetSegmentLength();
            }
        }

        protected double GetDistanceBetweenPoints(Point pointA, Point pointB)
        {
            double a = pointA.X - pointB.X;
            double b = pointA.Y - pointB.Y;
            double distance = Math.Sqrt(a * a + b * b);
            return distance;
        }
    }
}
