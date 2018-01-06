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
        }
    }
}
