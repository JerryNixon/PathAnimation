using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using CustomControls.Internal;

namespace CustomControls.ExtendedSegments
{
    class ExtendedArcSegment : ExtendedSegmentBase
    {
        private readonly ExtendedPolySegmentBase _approximateBezier;

        public ExtendedArcSegment(PathSegment segment, Point startPoint) : base(segment, startPoint)
        {
            if (!(segment is ArcSegment))
                throw new ArgumentException();
            var s = (ArcSegment)segment;

            var tmp = SegmentsHelper.ArcToBezier(StartPoint.X, StartPoint.Y, s.Point.X, s.Point.Y, s.Size.Width, s.Size.Height, s.RotationAngle, s.IsLargeArc, s.SweepDirection == SweepDirection.Clockwise);
            _approximateBezier = new ExtendedPolySegmentBase(tmp, startPoint);
        }

        public override Point GetPointAt(double percent)
        {
            return _approximateBezier.GetPointAt(percent);
        }

        protected override Point GetEndPoint()
        {
            return _approximateBezier.EndPoint;
        }

        protected override double GetSegmentLength()
        {
            return _approximateBezier.SegmentLength;
        }

        public override double GetDegreesAt(double percent)
        {
            return _approximateBezier.GetDegreesAt(percent);
        }

        public override double GetOrientedDegreesAt(double percent)
        {
            return _approximateBezier.GetOrientedDegreesAt(percent);
        }
    }
}
