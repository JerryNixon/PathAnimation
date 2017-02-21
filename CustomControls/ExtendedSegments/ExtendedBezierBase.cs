using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.ExtendedSegments
{
    /// <summary>
    /// Extended bezier base class, providing common functionality across different bezier segment types.
    /// </summary>
    /// <seealso cref="CustomControls.ExtendedSegments.ExtendedSegmentBase" />
    abstract class ExtendedBezierBase : ExtendedSegmentBase
    {
        protected ExtendedBezierBase(PathSegment segment, Point startPoint) : base(segment, startPoint)
        {
            NormalizeBezier();
        }

        protected sealed override Point GetEndPoint()
        {
            return GetPointAt(1);
        }

        protected sealed override double GetSegmentLength()
        {
            return _length;
        }

        public sealed override double GetDegreesAt(double percent)
        {
            var per = BezierNormalizedTable.First(bt => bt.Value >= percent);
            var dt = BezierDerivative(per.Key);
            var res = Math.Atan(dt) * (180 / Math.PI);
            return res;
        }

        /// <summary>
        /// Gets the oriented degrees, meaning that the rotation will follow the direction of the path.
        /// For doing that, we take a second point really close to the current one, and we calculate the
        /// vector between those 2 points.
        /// </summary>
        /// <param name="percent">The percent.</param>
        /// <returns></returns>
        public override double GetOrientedDegreesAt(double percent)
        {
            var per = BezierNormalizedTable.First(bt => bt.Value >= percent);
            var point = BezierFormula(per.Key);
            var nextPoint = BezierFormula(per.Key + 0.0001);
            return ExtendedLineSegment.GetOrientedDegrees(point, nextPoint);
        }

        /// <summary>
        /// Gets point at current percent
        /// </summary>
        /// <param name="percent">The percent.</param>
        /// <returns></returns>
        public sealed override Point GetPointAt(double percent)
        {
            //Get the point based on distance (taken from normalized table)
            var per = BezierNormalizedTable.First(bt => bt.Value >= percent);
            return BezierFormula(per.Key);
        }

        protected abstract Point BezierFormula(double t);
        protected abstract double BezierDerivative(double t);

        private double _length;
        /// <summary>
        /// The bezier normalized table.
        /// Keys contain the x value that we call BezierFormul, values contain the actual point distance from start.
        /// </summary>
        protected Dictionary<double, double> BezierNormalizedTable = new Dictionary<double, double>();

        /// <summary>
        /// Bezier curves formula, does not provide a constant speed accross the curve.
        /// For this reason, we have to construct a normalized table, for having a quick reference to the percent
        /// that we have to provide in order to get a constant speed.
        /// </summary>
        private void NormalizeBezier()
        {
            var sampleProf = 1.5;
            var step = 0.20;
            for (double i = 0; i < 100; i = i + step)
            {
                _length += (GetDistanceBetweenPoints(BezierFormula(i / 100), BezierFormula((i + step) / 100)));
            }

            var noOfSamples = (int)_length * sampleProf;

            BezierNormalizedTable.Add(0, 0);
            _length = 0;
            for (double i = 0; i < noOfSamples - 1; i++)
            {
                _length += (GetDistanceBetweenPoints(BezierFormula(i / noOfSamples), BezierFormula((i + 1) / noOfSamples)));
                var cur = (i + 1) / noOfSamples;
                var act = (_length / noOfSamples) * sampleProf;
                BezierNormalizedTable.Add(cur, act);
            }
            BezierNormalizedTable.Add(1, 1);
        }

    }
}
