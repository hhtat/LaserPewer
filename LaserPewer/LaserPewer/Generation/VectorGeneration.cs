using LaserPewer.Utilities;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LaserPewer.Model.Generation
{
    public class VectorGeneration
    {
        public event EventHandler GenerationCompleted;

        public IReadOnlyList<Point> VectorPath { get; private set; }

        private readonly double power;
        private readonly double speed;

        private readonly List<Drawing.Path> inputPaths;

        public VectorGeneration(double power, double speed, IReadOnlyList<Drawing.Path> paths)
        {
            this.power = power;
            this.speed = speed;

            inputPaths = new List<Drawing.Path>(paths);
        }

        public async void GenerateAsync()
        {
            StopWatch stopWatch = new StopWatch();

            List<PathInfo> paths = new List<PathInfo>();
            foreach (Drawing.Path path in inputPaths)
            {
                paths.Add(new PathInfo(path));
            }

            stopWatch.TraceLap("Calculate bounds & area");

            paths.Sort((a, b) => a.Area.CompareTo(b.Area));

            stopWatch.TraceLap("Sort by area");

            for (int i = 0; i < paths.Count; i++)
            {
                PathInfo child = paths[i];
                for (int j = i + 1; j < paths.Count; j++)
                {
                    PathInfo parent = paths[j];
                    if (parent.Bounds.Contains(child.Bounds) && parent.Bounds != child.Bounds)
                    {
                        parent.Children.Add(child);
                        child.Parents.Add(parent);
                    }
                }
            }

            stopWatch.TraceLap("Lineages calculated");

            HashSet<PathInfo> parkedPaths = new HashSet<PathInfo>(); // TODO for assertion only
            HashSet<PathInfo> readyPaths = new HashSet<PathInfo>();
            foreach (PathInfo path in paths)
            {
                if (path.Children.Count == 0)
                {
                    readyPaths.Add(path);
                }
                else
                {
                    parkedPaths.Add(path);
                }
            }

            stopWatch.TraceLap("Sets initialized");

            paths.Clear();
            Point lastEndPoint = new Point(0.0, 0.0);

            while (readyPaths.Count > 0)
            {
                PathInfo nearestPath = null;
                double nearestDistanceSquared = double.MaxValue;

                foreach (PathInfo path in readyPaths)
                {
                    double distanceSquared = path.LocateNearestStartPoint(lastEndPoint);
                    if (distanceSquared < nearestDistanceSquared)
                    {
                        nearestPath = path;
                        nearestDistanceSquared = distanceSquared;
                    }
                }

                paths.Add(nearestPath);
                readyPaths.Remove(nearestPath);
                foreach (PathInfo parent in nearestPath.Parents)
                {
                    parent.Children.Remove(nearestPath);
                    if (parent.Children.Count == 0)
                    {
                        parkedPaths.Remove(parent);
                        readyPaths.Add(parent);
                    }
                }
                nearestPath.Parents.Clear();

                lastEndPoint = nearestPath.EndPoint;
            }

            if (parkedPaths.Count > 0) throw new InvalidOperationException();

            stopWatch.TraceLap("Paths sequenced");

            List<Point> points = new List<Point>();

            foreach (PathInfo path in paths)
            {
                if (path.Path.Closed)
                {
                    for (int i = 0; i <= path.Path.Points.Count; i++)
                    {
                        points.Add(path.Path.Points[(path.StartIndex + i) % path.Path.Points.Count]);
                    }
                }
                else if (path.StartIndex == 0)
                {
                    for (int i = 0; i < path.Path.Points.Count; i++)
                    {
                        points.Add(path.Path.Points[i]);
                    }
                }
                else
                {
                    for (int i = path.Path.Points.Count - 1; i >= 0; i--)
                    {
                        points.Add(path.Path.Points[i]);
                    }
                }
            }

            VectorPath = points;

            GenerationCompleted?.Invoke(this, null);
        }

        private class PathInfo
        {
            public readonly Drawing.Path Path;

            public readonly Rect Bounds;
            public readonly double Area;

            public readonly HashSet<PathInfo> Parents;
            public readonly HashSet<PathInfo> Children;

            public int StartIndex;

            public Point EndPoint
            {
                get
                {
                    return Path.Points[Path.Closed ? StartIndex : ((StartIndex + (Path.Points.Count - 1)) % Path.Points.Count)];
                }
            }

            public PathInfo(Drawing.Path path)
            {
                Path = path;

                Bounds = path.CalculateBounds();
                Area = Bounds.Width * Bounds.Height;

                Parents = new HashSet<PathInfo>();
                Children = new HashSet<PathInfo>();

                StartIndex = 0;
            }

            public double LocateNearestStartPoint(Point point)
            {
                double nearestDistanceSquared = double.MaxValue;

                for (int i = 0; i < Path.Points.Count; i++)
                {
                    if (!Path.Closed && i == 1)
                    {
                        i = Path.Points.Count - 1;
                    }

                    double distanceSquared = (Path.Points[i] - point).LengthSquared;
                    if (distanceSquared < nearestDistanceSquared)
                    {
                        nearestDistanceSquared = distanceSquared;
                        StartIndex = i;
                    }
                }

                return nearestDistanceSquared;
            }
        }
    }
}
