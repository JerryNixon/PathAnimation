using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.ExtendedSegments
{
    /// <summary>
    /// Extended segment base, providing common functionality for all extended segment types.
    /// </summary>
    abstract class ExtendedSegmentBase
    {
        #region private members
        private Point _endPoint = new Point(double.NaN, double.NaN);
        private double _segmentLength = double.NaN;
        #endregion

        #region abstract members        
        /// <summary>
        /// Gets a point at percent
        /// </summary>
        /// <param name="percent">The percent.</param>
        /// <returns></returns>
        public abstract Point GetPointAt(double percent);
        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <returns></returns>
        protected abstract Point GetEndPoint();
        /// <summary>
        /// Gets the length of the segment.
        /// </summary>
        /// <returns></returns>
        protected abstract double GetSegmentLength();
        /// <summary>
        /// Gets the degrees at percent.
        /// </summary>
        /// <param name="percent">The percent.</param>
        /// <returns></returns>
        public abstract double GetDegreesAt(double percent);
        /// <summary>
        /// Gets the oriented degrees at percent.
        /// </summary>
        /// <param name="percent">The percent.</param>
        /// <returns></returns>
        public abstract double GetOrientedDegreesAt(double percent);
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedSegmentBase"/> class.
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="startPoint">The start point.</param>
        protected ExtendedSegmentBase(PathSegment segment, Point startPoint)
        {
            Segment = segment;
            StartPoint = startPoint;
        }

        /// <summary>
        /// Gets or sets the segment.
        /// </summary>
        /// <value>
        /// The original segment.
        /// </value>
        public PathSegment Segment { get; protected set; }

        /// <summary>
        /// Gets the start point.
        /// </summary>
        /// <value>
        /// The start point.
        /// </value>
        public Point StartPoint { get; private set; }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        public Point EndPoint
        {
            get
            {
                if (!double.IsNaN(_endPoint.X) && !double.IsNaN(_endPoint.Y))
                    return _endPoint;
                return _endPoint = GetEndPoint();
            }
        }

        /// <summary>
        /// Gets or sets the distance from start.
        /// </summary>
        /// <value>
        /// The distance from start.
        /// </value>
        public double DistanceFromStart { get; internal set; }
        
        /// <summary>
        /// Gets or sets a value indicating the percent from start
        /// </summary>
        /// <value>
        /// The starts at percent.
        /// </value>
        public double StartsAtPercent { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating the percent that the segment ends
        /// </summary>
        /// <value>
        /// The ends at percent.
        /// </value>
        public double EndsAtPercent { get; internal set; }

        /// <summary>
        /// Gets the length of the segment.
        /// </summary>
        /// <value>
        /// The length of the segment.
        /// </value>
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
