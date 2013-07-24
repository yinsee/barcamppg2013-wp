using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BarCamp
{
    public struct BarcodePosition
    {
        public Point BottomLeft { get; set; }
        public Point TopLeft { get; set; }
        public Point TopRight { get; set; }
        public Point BottomRight { get; set; }

        public BarcodePosition(Point p1, Point p2, Point p3, int dimension, double imageWidth, double imageHeight)
            : this()
        {
            var x = (p1.X + p3.X) - p2.X;
            var y = (p1.Y + p3.Y) - p2.Y;
            var p4 = new Point(x, y);

            double distance = GetExtendDistance(p1, p2, dimension);
            p3 = GetExtendedPoint(p1, p3, distance, imageWidth, imageHeight);
            p1 = GetExtendedPoint(p3, p1, distance, imageWidth, imageHeight);
            p4 = GetExtendedPoint(p2, p4, distance, imageWidth, imageHeight);
            p2 = GetExtendedPoint(p4, p2, distance, imageWidth, imageHeight);

            this.BottomLeft = p1;
            this.TopLeft = p2;
            this.TopRight = p3;
            this.BottomRight = p4;
        }

        private static double GetExtendDistance(Point p1, Point p2, int dimension)
        {
            double p1p2 = PointDistance(p1, p2);
            int squareCount = dimension - 6;
            double unit = p1p2 / squareCount;
            double side = 4 * unit;
            return Math.Sqrt(2) * side;
        }

        private static double PointDistance(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        private static Point GetExtendedPoint(Point p1, Point p2, double distance, double width, double height)
        {
            double x1 = p1.X;
            double x2 = p2.X;
            double y1 = height - p1.Y;
            double y2 = height - p2.Y;
            double s = (x1 - x2) / (y1 - y2);
            double t = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            double common = (distance + t) * Math.Sqrt(1 / (1 + s * s));

            double y = common + y1;
            double x = s * common + x1;
            double calcDistance = PointDistance(new Point(x, height - y), p2);
            if (calcDistance > 2 * distance)
            {
                y = -common + y1;
                x = -s * common + x1;
            }
            var p3 = new Point(x, height - y);
            return p3;
        }

        private static bool CloseTo(double x, double y)
        {
            return Math.Abs(x - y) < 0.000001;
        }
    }
}
