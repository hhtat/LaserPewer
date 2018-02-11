using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LaserPewer.Geometry
{
    public class Path
    {
        public readonly IReadOnlyList<Point> Points;
        public readonly bool Closed;

        private Path(List<Point> points, bool closed)
        {
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

        public class Builder
        {
            private List<Point> points;
            private readonly List<Path> paths;

            public Builder()
            {
                paths = new List<Path>();
                points = new List<Point>();
            }

            public void StartPath()
            {
                EndPath();
            }

            public void AddPoint(double x, double y)
            {
                AddPoint(new Point(x, y));
            }

            public void AddPoint(Point point)
            {
                if (points.Count > 0 && point == points.Last()) return;

                points.Add(point);
            }

            public void EndPath()
            {
                if (points.Count == 0) return;

                bool closed = false;
                if (points.Last() == points.First())
                {
                    points.RemoveAt(points.Count - 1);
                    if (points.Count >= 3) closed = true;
                }

                if (points.Count >= 2) paths.Add(new Path(points, closed));
                points = new List<Point>();
            }

            public void ClosePath()
            {
                if (points.Count == 0) return;

                Point first = points.First();
                if (points.Last() != first) points.Add(first);
                EndPath();
            }

            public List<Path> GetPaths()
            {
                EndPath();
                return paths;
            }
        }
    }
}
