using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.ExtendedSegments
{
    class ExtendedLineSegment: ExtendedSegmentBase
    {
        public ExtendedLineSegment(PathSegment segment,Point startPoint) : base(segment, startPoint)
        {
            if (!(segment is LineSegment))
                throw new ArgumentException();
        }

        protected override Point GetEndPoint()
        {
            var s = (LineSegment)Segment;
            return s.Point;
        }

        protected override double GetSegmentLength()
        {
            return GetDistanceBetweenPoints(StartPoint, EndPoint);
        }

        public override double GetDegreesAt(double percent)
        {
            if (EndPoint.X.Equals(StartPoint.X))
                if (EndPoint.Y >= StartPoint.Y)
                    return 90;
                else
                    return -90;

            var dt = (EndPoint.Y - StartPoint.Y) / (EndPoint.X - StartPoint.X);
            return  (Math.Atan(dt) * (180 / Math.PI));
           
        }

        public override Point GetPointAt(double percent)
        {
            var indP = new Point();
            indP.X = StartPoint.X + percent * (EndPoint.X - StartPoint.X);
            indP.Y = StartPoint.Y + percent * (EndPoint.Y - StartPoint.Y);
            return indP;
        }
    }
}
