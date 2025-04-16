using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GIS
{
    public static class LayerSimplifier
    {
        public static (float zoomLimit, float angle)[] zoomAnglePairs = new (float zoomLimit, float angle)[]
        {
            //(0.045f, 0.5f),
            //(0.1f, 0.49f),
            //(0.4f, 0.47f),
            (0.8f, 0f)
        };


        public static float ZoomToAngle(float zoom)
        {
            foreach (var (zoomLimit, angle) in zoomAnglePairs)
            {
                if (zoom <= zoomLimit)
                    return angle;
            }

            return 0;
        }

        public static List<Point> SimplifyPolygon(List<Point> points, double epsilon)
        {
            if (points.Count < 3) return points;

            int firstIndex = 0;
            int lastIndex = points.Count - 1;
            List<int> pointIndicesToKeep = new List<int> { firstIndex, lastIndex };

            SimplifySection(points, firstIndex, lastIndex, epsilon, pointIndicesToKeep);

            pointIndicesToKeep.Sort();
            return pointIndicesToKeep.Select(i => points[i]).ToList();
        }

        private static void SimplifySection(List<Point> points, int first, int last, double epsilon, List<int> pointIndicesToKeep)
        {
            double maxDist = 0;
            int index = first;

            for (int i = first + 1; i < last; i++)
            {
                double dist = PerpendicularDistance(points[i], points[first], points[last]);
                if (dist > maxDist)
                {
                    index = i;
                    maxDist = dist;
                }
            }

            if (maxDist > epsilon)
            {
                pointIndicesToKeep.Add(index);
                SimplifySection(points, first, index, epsilon, pointIndicesToKeep);
                SimplifySection(points, index, last, epsilon, pointIndicesToKeep);
            }
        }

        private static double PerpendicularDistance(Point pt, Point lineStart, Point lineEnd)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;
            double mag = Math.Sqrt(dx * dx + dy * dy);
            if (mag == 0) return 0;

            double u = ((pt.X - lineStart.X) * dx + (pt.Y - lineStart.Y) * dy) / (mag * mag);
            PointF intersection = new PointF(
                (float)(lineStart.X + u * dx),
                (float)(lineStart.Y + u * dy)
            );

            return Math.Sqrt(Math.Pow(pt.X - intersection.X, 2) + Math.Pow(pt.Y - intersection.Y, 2));
        }

        public static Point[] SimplifyPoly(Point[] points, float prop)
        {
            List<Point> p = new List<Point>(points);
            int i = 0;
            while (i + 2 < p.Count)
            {
                float a = GetProportion(p[i].X, p[i].Y, p[i + 1].X, p[i + 1].Y, p[i + 2].X, p[i + 2].Y);
                if (a < prop)
                {
                    p.RemoveAt(i + 1);
                    continue;
                }
                i++;
            }

            float GetProportion(float x0, float y0, float x1, float y1, float x2, float y2)
            {
                double dx = x2 - x0;
                double dy = y2 - y0;

                double midX = x0 + dx / 2;
                double midY = y0 + dy / 2;

                double numerator = Math.Pow(x1 - midX, 2) + Math.Pow(y1 - midY, 2);
                double denominator = Math.Pow(dx, 2) + Math.Pow(dy, 2);

                return (float)Math.Sqrt(numerator / denominator);
            }

            return p.Count >= 3 ? p.ToArray() : points;
        }
    }
}

