using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using CustomControls.Internal;

namespace CustomControls.ExtendedSegments
{
    /// <summary>
    /// Extended arc segment class. 
    /// </summary>
    /// <seealso cref="CustomControls.ExtendedSegments.ExtendedSegmentBase" />
    class ExtendedArcSegment : ExtendedSegmentBase
    {
        private readonly ExtendedPolySegmentBase _approximateBezier;

        public ExtendedArcSegment(PathSegment segment, Point startPoint) : base(segment, startPoint)
        {
            //Making sure segment is ArcSegment
            if (!(segment is ArcSegment))
                throw new ArgumentException("ArcSegment expected on ExtendedArcSegment constructor");
            var s = (ArcSegment)segment;

            //We approximate ArcSegment to a PolyBezierSegment
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
