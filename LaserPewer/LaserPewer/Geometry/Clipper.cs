using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LaserPewer.Geometry
{
    public static class Clipper
    {
        public static List<Path> ClipPaths(IEnumerable<Path> paths, Rect clip)
        {
            List<Path> clipped = new List<Path>();

            foreach (Path path in paths)
            {
                clipped.AddRange(ClipPath(path, clip));
            }

            return clipped;
        }

        public static List<Path> ClipPath(Path path, Rect clip)
        {
            Path.Builder pathBuilder = new Path.Builder();

            Point prev = path.Points[0];
            pathBuilder.AddPoint(prev);

            for (int i = 1; i <= path.Points.Count; i++)
            {
                if (i == path.Points.Count && !path.Closed) break;
                Point point = path.Points[i % path.Points.Count];

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

            List<Path> paths = pathBuilder.GetPaths();

            if (paths.Count >= 2)
            {
                Path firstPath = paths.First();
                Path lastPath = paths.Last();

                if (firstPath.Points.First() == lastPath.Points.Last())
                {
                    Path.Builder pathBuilder2 = new Path.Builder();
                    foreach (Point point in lastPath.Points) pathBuilder2.AddPoint(point);
                    foreach (Point point in firstPath.Points) pathBuilder2.AddPoint(point);
                    paths[0] = pathBuilder2.GetPaths().First();
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

                if (p == 0 && q < 0)
                {
                    return false;
                }

                r = q / p;

                if (p < 0)
                {
                    if (r > t1)
                    {
                        return false;
                    }
                    if (r > t0) t0 = r;
                }
                else if (p > 0)
                {
                    if (r < t0)
                    {
                        return false;
                    }
                    if (r < t1) t1 = r;
                }
            }

            Point _a = a;

            if (t0 != 0.0)
            {
                a.X = _a.X + t0 * delta.X;
                a.Y = _a.Y + t0 * delta.Y;
            }

            if (t1 != 1.0)
            {
                b.X = _a.X + t1 * delta.X;
                b.Y = _a.Y + t1 * delta.Y;
            }

            return true;
        }
    }
}
