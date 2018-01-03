﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LaserPewer
{
    public class PathBuilder
    {
        private readonly List<Drawing.Path> paths;
        private List<Point> points;

        public PathBuilder()
        {
            paths = new List<Drawing.Path>();
            points = new List<Point>();
        }

        public void StartPath()
        {
            if (points.Count >= 2)
            {
                if (points.First() != points.Last())
                {
                    paths.Add(new Drawing.Path(points, false));
                }
                else if (points.Count > 2)
                {
                    points.RemoveAt(points.Count - 1);
                    paths.Add(new Drawing.Path(points, true));
                }
            }
            points = new List<Point>();
        }

        public void AddPoint(double x, double y)
        {
            AddPoint(new Point(x, y));
        }

        public void AddPoint(Point point)
        {
            points.Add(point);
        }

        public void ClosePath()
        {
            if (points.Count >= 2) paths.Add(new Drawing.Path(points, true));
            points = new List<Point>();
        }

        public void EndPath()
        {
            StartPath();
        }

        public List<Drawing.Path> GetPaths()
        {
            EndPath();
            return paths;
        }
    }
}
