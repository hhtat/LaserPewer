using System.Collections.Generic;
using System.Windows;

namespace LaserPewer
{
    public class Drawing
    {
        public readonly Path[] Paths;

        public Drawing(List<Path> paths)
        {
            Paths = paths.ToArray();
        }

        public class Path
        {
            public readonly Point[] Points;

            public Path(List<Point> points)
            {
                Points = points.ToArray();
            }
        }
    }
}
