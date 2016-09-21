using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.ExtendedSegments
{
    class ExtendedPolySegmentBase : ExtendedSegmentBase
    {
        private List<ExtendedSegmentBase> _segments = new List<ExtendedSegmentBase>();

        public ExtendedPolySegmentBase(PathSegment segment, Point startPoint) : base(segment, startPoint)
        {
            List<PathSegment> segments = new List<PathSegment>();
            if (segment is PolyLineSegment)
            {
                var tmp = (PolyLineSegment)segment;
                FillSegments(tmp.Points.Select(x => (PathSegment)new LineSegment() { Point = x }).ToList(), startPoint);
            }
            else if (segment is PolyBezierSegment)
            {
                var tmp = (PolyBezierSegment)segment;
                for (int i = 0; i < tmp.Points.Count; i = i + 3)
                    segments.Add(new BezierSegment() { Point1 = tmp.Points[i], Point2 = tmp.Points[i + 1], Point3 = tmp.Points[i + 2] });
                FillSegments(segments, startPoint);
            }
            else if (segment is PolyQuadraticBezierSegment)
            {
                var tmp = (PolyQuadraticBezierSegment)segment;
                for (int i = 0; i < tmp.Points.Count ; i = i + 2)
                    segments.Add(new QuadraticBezierSegment() { Point1 = tmp.Points[i], Point2 = tmp.Points[i + 1] });
                FillSegments(segments, startPoint);
            }
            if (!_segments.Any())
                throw new ArgumentNullException();
        }

        private void FillSegments(List<PathSegment> segments, Point FirstPoint)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                Point startPoint;
                startPoint = (i == 0 ? FirstPoint : _segments[i - 1].EndPoint);
                if (segments[i] is LineSegment)
                    _segments.Add(new ExtendedLineSegment(segments[i], startPoint));
                else if (segments[i] is BezierSegment)
                    _segments.Add(new ExtendedBezierSegment(segments[i], startPoint));
                else if (segments[i] is QuadraticBezierSegment)
                    _segments.Add(new ExtendedQuadraticBezierSegment(segments[i], startPoint));
            }

            //calculate total length
            double TotalLength = 0;
            for (int i = 0; i < _segments.Count; i++)
            {
                var d = _segments[i].SegmentLength;
                TotalLength += d;
                _segments[i].DistanceFromStart = TotalLength;
            }

            for (int i = 0; i < _segments.Count; i++)
            {
                _segments[i].EndsAtPercent = _segments[i].DistanceFromStart / TotalLength;
            }
            for (int i = 1; i < _segments.Count; i++)
            {
                _segments[i].StartsAtPercent = _segments[i - 1].EndsAtPercent;
            }
        }

        public override Point GetPointAt(double percent)
        {
            var s = _segments.First(c => c.EndsAtPercent >= percent);
            double range = s.EndsAtPercent - s.StartsAtPercent;
            percent = percent - s.StartsAtPercent; //tranfer to 0
            percent = percent / range;//convert to local percent
            return s.GetPointAt(percent);
        }

        protected override Point GetEndPoint()
        {
            return _segments.Last().EndPoint;
        }

        protected override double GetSegmentLength()
        {
            return _segments.Sum(x => x.SegmentLength);
        }

        public override double GetDegreesAt(double percent)
        {
            var s = _segments.First(c => c.EndsAtPercent >= percent);
            double range = s.EndsAtPercent - s.StartsAtPercent;
            percent = percent - s.StartsAtPercent; //tranfer to 0
            percent = percent / range;//convert to local percent
            return s.GetDegreesAt(percent);
        }
    }
}
