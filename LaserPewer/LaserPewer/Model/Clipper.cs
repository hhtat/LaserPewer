using System.Collections.Generic;
using System.Windows;
using System.Linq;
using LaserPewer.Utilities;

namespace LaserPewer.Model
{
    public static class Clipper
    {
        public static List<Drawing.Path> ClipPaths(IEnumerable<Drawing.Path> paths, Rect clip)
        {
            List<Drawing.Path> clipped = new List<Drawing.Path>();

            foreach (Drawing.Path path in paths)
            {
                clipped.AddRange(ClipPath(path, clip));
            }

            return clipped;
        }

        public static List<Drawing.Path> ClipPath(Drawing.Path path, Rect clip)
        {
            PathBuilder pathBuilder = new PathBuilder();

            Point prev = Precision.Round3(path.Points[0]);
            pathBuilder.AddPoint(prev);

            for (int i = 1; i <= path.Points.Count; i++)
            {
                if (i == path.Points.Count && !path.Closed) break;
                Point point = Precision.Round3(path.Points[i % path.Points.Count]);

                Point a = prev;
                Point b = point;
                if (ClipLine(ref a, ref b, clip))
                {
                    if (a != prev) { pathBuilder.StartPath(); pathBuilder.AddPoint(a); }
                    pathBuilder.AddPoint(b);
                    if (b != point) pathBuilder.EndPath();
                }

                prev = point;
            }

            List<Drawing.Path> paths = pathBuilder.GetPaths();

            if (paths.Count >= 2)
            {
                Drawing.Path firstPath = paths.First();
                Drawing.Path lastPath = paths.Last();

                if (firstPath.Points.First() == lastPath.Points.Last())
                {
                    List<Point> points = new List<Point>();
                    for (int i = 0; i < lastPath.Points.Count; i++) points.Add(lastPath.Points[i]);
                    for (int i = 1; i < firstPath.Points.Count; i++) points.Add(firstPath.Points[i]);
                    paths[0] = new Drawing.Path(points, false);
                    paths.RemoveAt(paths.Count - 1);
                }
            }

            return paths;
        }

        public static bool ClipLine(ref Point a, ref Point b, Rect clip)
        {
            if (clip.Contains(a) && clip.Contains(b)) return true;

            double t0 = 0.0;
            double t1 = 1.0;
            Vector delta = b - a;
            double p = 0.0;
            double q = 0.0;
            double r;

            for (int edge = 0; edge < 4; edge++)
            {
                if (edge == 0) { p = -delta.X; q = -(clip.Left - a.X); }
                if (edge == 1) { p = delta.X; q = (clip.Right - a.X); }
                if (edge == 2) { p = -delta.Y; q = -(clip.Top - a.Y); }
                if (edge == 3) { p = delta.Y; q = (clip.Bottom - a.Y); }

                if (p == 0 && q < 0) return false;

                r = q / p;

                if (p < 0)
                {
                    if (r > t1) return false;
                    if (r > t0) t0 = r;
                }
                else if (p > 0)
                {
                    if (r < t0) return false;
                    if (r < t1) t1 = r;
                }
            }

            Point _a = a;

            a.X = Precision.Round3(_a.X + t0 * delta.X);
            a.Y = Precision.Round3(_a.Y + t0 * delta.Y);
            b.X = Precision.Round3(_a.X + t1 * delta.X);
            b.Y = Precision.Round3(_a.Y + t1 * delta.Y);

            return true;
        }
    }
}
