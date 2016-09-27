using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace CustomControls.ExtendedSegments
{
    public class ExtendedPathGeometry : DependencyObject
    {
        public PathGeometry PathGeometry { get; }

        /// <summary>
        /// Contains information about potential blank space on the left and top of our path
        /// </summary>
        public Point PathOffset = new Point(double.MaxValue, double.MaxValue);

        /// <summary>
        /// contains all segments added by initial analysis
        /// </summary>
        private List<ExtendedSegmentBase> _allSegments;

        public ExtendedPathGeometry(PathGeometry data)
        {
            PathGeometry = data;
            AnalyzeSegments();
        }

        /// <summary>
        /// Total circumferential length of <see cref="ExtendedPathGeometry"/>
        /// </summary>
        public double PathLength { get { return (double)GetValue(PathLengthProperty); } private set { SetValue(PathLengthProperty, value); } }
        public static readonly DependencyProperty PathLengthProperty =
            DependencyProperty.Register(nameof(PathLength), typeof(double), typeof(ExtendedPathGeometry), new PropertyMetadata(default(double)));

        private void AnalyzeSegments()
        {
            _allSegments = new List<ExtendedSegmentBase>();
            var pg = PathGeometry;
            double pathLength = 0;

            foreach (var figure in pg.Figures)
            {
                var figureSegments = new List<ExtendedSegmentBase>();
                if (figure.IsClosed)
                    figure.Segments.Add(new LineSegment() { Point = figure.StartPoint });//close path by adding again startPoint
                for (int i = 0; i < figure.Segments.Count; i++)
                {
                    Point startPoint;
                    //determine start point of segment
                    startPoint = (i == 0 ? figure.StartPoint : figureSegments[i - 1].EndPoint);

                    if (figure.Segments[i] is LineSegment)
                        figureSegments.Add(new ExtendedLineSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is BezierSegment)
                        figureSegments.Add(new ExtendedBezierSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is QuadraticBezierSegment)
                        figureSegments.Add(new ExtendedQuadraticBezierSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is ArcSegment)
                        figureSegments.Add(new ExtendedArcSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is PolyLineSegment || figure.Segments[i] is PolyBezierSegment || figure.Segments[i] is PolyQuadraticBezierSegment)
                        figureSegments.Add(new ExtendedPolySegmentBase(figure.Segments[i], startPoint));
                }

                foreach (ExtendedSegmentBase t in figureSegments)
                {
                    t.DistanceFromStart = pathLength += t.SegmentLength;
                }

                _allSegments.AddRange(figureSegments);
            }

            for (int i = 0; i < _allSegments.Count; i++)
            {
                _allSegments[i].EndsAtPercent = _allSegments[i].DistanceFromStart / pathLength;
            }
            for (int i = 1; i < _allSegments.Count; i++)
            {
                _allSegments[i].StartsAtPercent = _allSegments[i - 1].EndsAtPercent;
            }

            foreach (var segment in _allSegments)
            {
                for (double i = 0; i < 1; i = i + 0.01)
                {
                    var point = segment.GetPointAt(i);
                    if (point.X < PathOffset.X)
                        PathOffset.X = point.X;
                    if (point.Y < PathOffset.Y)
                        PathOffset.Y = point.Y;
                }
            }

            PathLength = pathLength;

        }

        public void GetPointAtFractionLength(double progress, out Point point, out double rotationTheta)
        {
            if (progress < 0)
                progress = 0;
            progress = (progress % 100) / 100.0;//make sure that 0 <= percent <= 1
                                                //get segment that falls on this percent
            var segment = _allSegments.First(c => c.EndsAtPercent >= progress);

            //find range of segment
            double range = segment.EndsAtPercent - segment.StartsAtPercent;
            progress = progress - segment.StartsAtPercent; //tranfer to 0
            progress = progress / range;//convert to local percent

            point = segment.GetPointAt(progress);//get point at percent for segment
            rotationTheta = segment.GetOrientedDegreesAt(progress);
        }

        /// <summary>
        /// Returns current point and rotation theta
        /// </summary>
        /// <param name="progress"></param>
        /// <returns></returns>
        public Tuple<Point, double> GetPointAtFractionLength(double progress)
        {
            Point p;
            double rotation;
            GetPointAtFractionLength(progress, out p, out rotation);
            return new Tuple<Point, double>(p, rotation);
        }
    }
}
