using System;
using System.Collections.Generic;
using System.Windows;

namespace LaserPewer.Geometry
{
    public class Path
    {
        public readonly IReadOnlyList<Point> Points;
        public readonly bool Closed;

        public Path(List<Point> points, bool closed)
        {
            if (points.Count < 2) throw new ArgumentException();

            Points = points;
            Closed = closed;
        }

        public Rect CalculateBounds()
        {
            double xMin = double.MaxValue;
            double xMax = double.MinValue;
            double yMin = double.MaxValue;
            double yMax = double.MinValue;

            foreach (Point point in Points)
            {
                if (point.X < xMin) xMin = point.X;
                if (point.X > xMax) xMax = point.X;
                if (point.Y < yMin) yMin = point.Y;
                if (point.Y > yMax) yMax = point.Y;
            }

            return new Rect(new Point(xMin, yMin), new Point(xMax, yMax));
        }

        public static Path Offset(Path path, Vector offset)
        {
            List<Point> points = new List<Point>();

            foreach (Point point in path.Points)
            {
                points.Add(point + offset);
            }

            return new Path(points, path.Closed);
        }
    }
}
