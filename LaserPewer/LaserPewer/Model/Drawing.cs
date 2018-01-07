using System;
using System.Collections.Generic;
using System.Windows;

namespace LaserPewer.Model
{
    public class Drawing
    {
        public IReadOnlyList<Path> Paths { get; private set; }

        public Drawing(List<Path> paths)
        {
            Paths = paths;
        }

        public void Clip(Rect clip)
        {
            List<Path> paths = new List<Path>();

            Clipper clipper = new Clipper() { Clip = clip };

            foreach (Path path in Paths)
            {
                paths.AddRange(clipper.ClipPath(path));
            }

            Paths = paths;
        }

        public Drawing Clone()
        {
            return new Drawing(new List<Path>(Paths));
        }

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
        }
    }
}
