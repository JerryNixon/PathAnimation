using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace CustomControls.Internal
{
    internal class SegmentsHelper
    {
        /// <summary>
        /// Converts an arc to cubic Bezier segments and returns a PolyBezierSegment.
        /// </summary>
        /// <param name="x0">The x coordinate of the  ellipse's start point</param>
        /// <param name="y0">The y coordinate of the  ellipse's start point</param>
        /// <param name="x1">The x coordinate of the  ellipse's end point</param>
        /// <param name="y1">The y coordinate of the  ellipse's end point</param>
        /// <param name="a">The x-radius, width</param>
        /// <param name="b">The y-radius, height</param>
        /// <param name="theta">Rotation angle in degrees</param>
        /// <param name="isLarge">Uses the large arc</param>
        /// <param name="isPositiveArc">Set if clockwise direction</param>
        /// <returns></returns>
        public static PolyBezierSegment ArcToBezier(double x0, double y0, double x1, double y1, double a, double b, double theta, bool isLarge, bool isPositiveArc)
        {
            // Convert rotation angle from degrees to radians 
            double thetaD = (Math.PI / 180) * theta;
            // Pre-compute rotation matrix entries
            double cosTheta = Math.Cos(thetaD);
            double sinTheta = Math.Sin(thetaD);
            // Transform (x0, y0) and (x1, y1) into unit space
            // using (inverse) rotation, followed by (inverse) scale
            double x0p = (x0 * cosTheta + y0 * sinTheta) / a;
            double y0p = (-x0 * sinTheta + y0 * cosTheta) / b;
            double x1p = (x1 * cosTheta + y1 * sinTheta) / a;
            double y1p = (-x1 * sinTheta + y1 * cosTheta) / b;
            // Compute differences and averages
            double dx = x0p - x1p;
            double dy = y0p - y1p;
            double xm = (x0p + x1p) / 2;
            double ym = (y0p + y1p) / 2;
            // Solve for intersecting unit circles 
            double dsq = dx * dx + dy * dy;
            if (dsq == 0.0)
            {
                return null; // Points are coincident
            }
            double disc = 1.0 / dsq - 1.0 / 4.0;
            if (disc < 0.0)
            {
                float adjust = (float)(Math.Sqrt(dsq) / 1.99999);
                return ArcToBezier(x0, y0, x1, y1, a * adjust, b * adjust, theta, isLarge, isPositiveArc);
            }
            double s = Math.Sqrt(disc);
            double sdx = s * dx;
            double sdy = s * dy;
            double cx;
            double cy;
            if (isLarge == isPositiveArc)
            {
                cx = xm - sdy;
                cy = ym + sdx;
            }
            else
            {
                cx = xm + sdy;
                cy = ym - sdx;
            }
            double eta0 = Math.Atan2((y0p - cy), (x0p - cx));
            double eta1 = Math.Atan2((y1p - cy), (x1p - cx));
            double sweep = (eta1 - eta0);
            if (isPositiveArc != (sweep >= 0))
            {
                if (sweep > 0)
                {
                    sweep -= 2 * Math.PI;
                }
                else
                {
                    sweep += 2 * Math.PI;
                }
            }
            cx *= a;
            cy *= b;
            double tcx = cx;
            cx = cx * cosTheta - cy * sinTheta;
            cy = tcx * sinTheta + cy * cosTheta;
            return ArcToBezier(cx, cy, a, b, x0, y0, thetaD, eta0, sweep);
        }
        
        /// <summary>
        /// Converts an arc to cubic Bezier segments and returns a PolyBezierSegment.
        /// </summary>
        /// <param name="cx">The x coordinate center of the ellipse</param>
        /// <param name="cy">The y coordinate center of the ellipse</param>
        /// <param name="a">The radius of the ellipse in the horizontal direction</param>
        /// <param name="b">The radius of the ellipse in the vertical direction</param>
        /// <param name="e1x">E(eta1) x coordinate of the starting point of the arc</param>
        /// <param name="e1y">E(eta2) y coordinate of the starting point of the arc</param>
        /// <param name="theta">The angle that the ellipse bounding rectangle makes with horizontal plane</param>
        /// <param name="start">The start angle of the arc on the ellipse</param>
        /// <param name="sweep">The angle (positive or negative) of the sweep of the arc on the ellipse</param>
        /// <returns></returns>
        private static PolyBezierSegment ArcToBezier(double cx, double cy, double a, double b, double e1x, double e1y, double theta, double start, double sweep)
        {
            PolyBezierSegment res = new PolyBezierSegment();
            // Taken from equations at: http://spaceroots.org/documents/ellipse/node8.html
            // and http://www.spaceroots.org/documents/ellipse/node22.html
            // Maximum of 45 degrees per cubic Bezier segment
            int numSegments = (int)Math.Ceiling(Math.Abs(sweep * 4 / Math.PI));
            double eta1 = start;
            double cosTheta = Math.Cos(theta);
            double sinTheta = Math.Sin(theta);
            double cosEta1 = Math.Cos(eta1);
            double sinEta1 = Math.Sin(eta1);
            double ep1x = (-a * cosTheta * sinEta1) - (b * sinTheta * cosEta1);
            double ep1y = (-a * sinTheta * sinEta1) + (b * cosTheta * cosEta1);
            double anglePerSegment = sweep / numSegments;
            for (int i = 0; i < numSegments; i++)
            {
                double eta2 = eta1 + anglePerSegment;
                double sinEta2 = Math.Sin(eta2);
                double cosEta2 = Math.Cos(eta2);
                double e2x = cx + (a * cosTheta * cosEta2) - (b * sinTheta * sinEta2);
                double e2y = cy + (a * sinTheta * cosEta2) + (b * cosTheta * sinEta2);
                double ep2x = -a * cosTheta * sinEta2 - b * sinTheta * cosEta2;
                double ep2y = -a * sinTheta * sinEta2 + b * cosTheta * cosEta2;
                double tanDiff2 = Math.Tan((eta2 - eta1) / 2);
                double alpha = Math.Sin(eta2 - eta1) * (Math.Sqrt(4 + (3 * tanDiff2 * tanDiff2)) - 1) / 3;
                double q1x = e1x + alpha * ep1x;
                double q1y = e1y + alpha * ep1y;
                double q2x = e2x - alpha * ep2x;
                double q2y = e2y - alpha * ep2y;
                Debug.WriteLine("{0:F2},{1:F2} {2:F2},{3:F2} {4:F2},{5:F2}", q1x, q1y, q2x, q2y, e2x, e2y);
                res.Points.Add(new Point(q1x, q1y));
                res.Points.Add(new Point(q2x, q2y));
                res.Points.Add(new Point(e2x, e2y));
                eta1 = eta2;
                e1x = e2x;
                e1y = e2y;
                ep1x = ep2x;
                ep1y = ep2y;
            }
            return res;
        }
    }
}