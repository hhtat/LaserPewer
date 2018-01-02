using System.Collections.Generic;
using System.Windows;

namespace LaserPewer
{
    public class DrawingBuilder
    {
        private List<Drawing.Path> paths;
        private List<Point> points;

        public DrawingBuilder()
        {
            paths = new List<Drawing.Path>();
            points = new List<Point>();
        }

        public void StartPath()
        {
            if (points.Count >= 2) paths.Add(new Drawing.Path(points, false));
            points.Clear();
        }

        public void AddPoint(double x, double y)
        {
            points.Add(new Point(x, y));
        }

        public void ClosePath()
        {
            if (points.Count >= 2) paths.Add(new Drawing.Path(points, true));
            points.Clear();
        }

        public Drawing ToDrawing()
        {
            StartPath();
            return new Drawing(paths);
        }
    }
}
