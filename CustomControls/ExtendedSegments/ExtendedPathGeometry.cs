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
    /// <summary>
    /// Extended path geometry class, providing missing path geometry methods
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.DependencyObject" />
    public class ExtendedPathGeometry : DependencyObject
    {
        /// <summary>
        /// Gets the original path geometry.
        /// </summary>
        /// <value>
        /// The path geometry.
        /// </value>
        public PathGeometry PathGeometry { get; }

        /// <summary>
        /// Contains information about potential blank space on the left and top of our path
        /// </summary>
        public Point PathOffset = new Point(double.MaxValue, double.MaxValue);

        /// <summary>
        /// Contains all segments added by initial analysis
        /// </summary>
        private List<ExtendedSegmentBase> _allSegments;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedPathGeometry"/> class.
        /// </summary>
        /// <param name="data">The original data.</param>
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
            var pathLength = 0.0;

            foreach (var figure in PathGeometry.Figures)
            {
                if (figure.IsClosed)
                    figure.Segments.Add(new LineSegment() { Point = figure.StartPoint });//close path by using a line segment
                for (var i = 0; i < figure.Segments.Count; i++)
                {
                    //start point of segment is the end point of the previous one, or figure's start point if it is the first one
                    var startPoint = i == 0 ? figure.StartPoint : _allSegments[i - 1].EndPoint;

                    //add suitable extended segment based on original segment type
                    if (figure.Segments[i] is LineSegment)
                        _allSegments.Add(new ExtendedLineSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is BezierSegment)
                        _allSegments.Add(new ExtendedBezierSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is QuadraticBezierSegment)
                        _allSegments.Add(new ExtendedQuadraticBezierSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is ArcSegment)
                        _allSegments.Add(new ExtendedArcSegment(figure.Segments[i], startPoint));
                    else if (figure.Segments[i] is PolyLineSegment || figure.Segments[i] is PolyBezierSegment || figure.Segments[i] is PolyQuadraticBezierSegment)
                        _allSegments.Add(new ExtendedPolySegmentBase(figure.Segments[i], startPoint));
                }
               
                //calculate each segment's distance from start and total path length
                foreach (var segment in _allSegments)
                {
                    segment.DistanceFromStart = pathLength += segment.SegmentLength;
                }
            }

            foreach (var segment in _allSegments)
            {
                segment.EndsAtPercent = segment.DistanceFromStart / pathLength;
            }

            for (var i = 1; i < _allSegments.Count; i++)
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

            if (double.IsInfinity(PathOffset.X))
                PathOffset.X = 0;
            if (double.IsInfinity(PathOffset.Y))
                PathOffset.Y = 0;

            PathLength = pathLength;
        }

        /// <summary>
        /// Gets a point at fraction length.
        /// </summary>
        /// <param name="progress">The progress.</param>
        /// <param name="point">The point.</param>
        /// <param name="rotationTheta">The rotation theta.</param>
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
