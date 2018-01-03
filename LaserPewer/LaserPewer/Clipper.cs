using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace LaserPewer
{
    public class Clipper
    {
        public Rect Clip { get; set; }

        public List<Drawing.Path> ClipPath(Drawing.Path path)
        {
            PathBuilder pathBuilder = new PathBuilder();

            Point prev = Optimizer.Round3(path.Points[0]);
            pathBuilder.AddPoint(prev);

            for (int i = 1; i <= path.Points.Count; i++)
            {
                int j = i % path.Points.Count;
                if (j == 0 && !path.Closed) break;
                Point point = Optimizer.Round3(path.Points[j]);

                Point a = prev;
                Point b = point;
                if (ClipLine(ref a, ref b))
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
                    lastPath.Points.RemoveAt(lastPath.Points.Count - 1);
                    lastPath.Points.AddRange(firstPath.Points);
                    paths.RemoveAt(paths.Count - 1);
                    paths[0] = lastPath;
                }
            }

            return paths;
        }

        public bool ClipLine(ref Point a, ref Point b)
        {
            if (Clip.Contains(a) && Clip.Contains(b)) return true;

            double t0 = 0.0;
            double t1 = 1.0;
            Vector delta = b - a;
            double p = 0.0;
            double q = 0.0;
            double r;

            for (int edge = 0; edge < 4; edge++)
            {
                if (edge == 0) { p = -delta.X; q = -(Clip.Left - a.X); }
                if (edge == 1) { p = delta.X; q = (Clip.Right - a.X); }
                if (edge == 2) { p = -delta.Y; q = -(Clip.Top - a.Y); }
                if (edge == 3) { p = delta.Y; q = (Clip.Bottom - a.Y); }

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

            a.X = Optimizer.Round3(_a.X + t0 * delta.X);
            a.Y = Optimizer.Round3(_a.Y + t0 * delta.Y);
            b.X = Optimizer.Round3(_a.X + t1 * delta.X);
            b.Y = Optimizer.Round3(_a.Y + t1 * delta.Y);

            return true;
        }
    }
}
