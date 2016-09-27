using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.ExtendedSegments
{
    abstract class ExtendedBezierBase : ExtendedSegmentBase
    {
        public ExtendedBezierBase(PathSegment segment, Point startPoint) : base(segment, startPoint)
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
            var res = (Math.Atan(dt) * (180 / Math.PI));
            return res;
        }

        public override double GetOrientedDegreesAt(double percent)
        {
            var per = BezierNormalizedTable.First(bt => bt.Value >= percent);
            var point = BezierFormula(per.Key);
            var nextPoint = BezierFormula(per.Key + 0.0001);

            return ExtendedLineSegment.GetOrientedDegrees(point, nextPoint);

           
        }

        public sealed override Point GetPointAt(double percent)
        {
            var per = BezierNormalizedTable.First(bt => bt.Value >= percent);
            return BezierFormula(per.Key);
        }

        protected abstract Point BezierFormula(double t);
        protected abstract double BezierDerivative(double t);

        private double _length;
        protected Dictionary<double, double> BezierNormalizedTable = new Dictionary<double, double>();

        private void NormalizeBezier()
        {
            double sampleProf = 1.5;
            double step = 0.20;
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
