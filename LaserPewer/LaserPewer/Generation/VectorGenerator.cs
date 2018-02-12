using LaserPewer.Geometry;
using LaserPewer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LaserPewer.Generation
{
    public static class VectorGenerator
    {
        public static MachinePath Generate(IReadOnlyList<Path> paths, double power, double speed)
        {
            PathTree precedenceTree = new PathTree(paths);
            int[] priorities = new int[precedenceTree.Size];
            IReadOnlyList<PathNode> schedule = precedenceTree.Traverse(priorities);
            return toMachinePath(schedule, power, speed);
        }

        private static MachinePath toMachinePath(IReadOnlyList<PathNode> schedule, double power, double speed)
        {
            MachinePath machinePath = new MachinePath();

            machinePath.SetPowerAndSpeed(power, speed);
            Point lastEndPoint = new Point(0.0, 0.0);

            foreach (PathNode path in schedule)
            {
                path.LocateNearestStartPoint(lastEndPoint);
                if (path.Path.Closed)
                {
                    for (int i = 0; i <= path.Path.Points.Count; i++)
                    {
                        machinePath.TravelTo(path.Path.Points[(path.StartIndex + i) % path.Path.Points.Count]);
                    }
                }
                else if (path.StartIndex == 0)
                {
                    for (int i = 0; i < path.Path.Points.Count; i++)
                    {
                        machinePath.TravelTo(path.Path.Points[i]);
                    }
                }
                else
                {
                    for (int i = path.Path.Points.Count - 1; i >= 0; i--)
                    {
                        machinePath.TravelTo(path.Path.Points[i]);
                    }
                }

                machinePath.EndCut();
            }

            return machinePath;
        }

        private class PathTree
        {
            public int Size { get { return nodes.Count; } }

            private readonly List<PathNode> nodes;

            public PathTree(IReadOnlyList<Path> paths)
            {
                nodes = paths.Select(path => new PathNode(path)).ToList();
                nodes.Sort((a, b) => a.Path.BoundsArea.CompareTo(b.Path.BoundsArea));

                for (int i = 0; i < nodes.Count; i++)
                {
                    PathNode nodeA = nodes[i];
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        PathNode nodeB = nodes[j];
                        if (nodeB.Path.Bounds.Contains(nodeA.Path.Bounds) && nodeB.Path.Bounds != nodeA.Path.Bounds)
                        {
                            nodeB.Children.Add(nodeA);
                            nodeA.Parents.Add(nodeB);
                        }
                    }
                }
            }

            public IReadOnlyList<PathNode> Traverse(int[] priorities)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Reset(priorities[i]);
                }

                PriorityQueue<PathNode> queue = new PriorityQueue<PathNode>((a, b) => a.Priority.CompareTo(b.Priority));
                foreach (PathNode node in nodes.Where(node => node.PendingChildren.Count == 0))
                {
                    queue.Enqueue(node);
                }

                List<PathNode> traversal = new List<PathNode>();

                while (queue.Count > 0)
                {
                    PathNode node = queue.Dequeue();
                    traversal.Add(node);
                    foreach (PathNode parent in node.Parents)
                    {
                        parent.Children.Remove(node);
                        if (parent.Children.Count == 0) queue.Enqueue(parent);
                    }
                }

                return traversal;
            }
        }

        private class PathNode
        {
            public readonly Path Path;

            public readonly HashSet<PathNode> Parents;
            public readonly HashSet<PathNode> Children;

            public HashSet<PathNode> PendingChildren { get; private set; }

            public int Priority { get; private set; }
            public int StartIndex { get; private set; }
            public int EndIndex { get { return Path.Closed ? StartIndex : ((StartIndex + Path.Points.Count - 1) % Path.Points.Count); } }

            public PathNode(Path path)
            {
                Path = path;

                Parents = new HashSet<PathNode>();
                Children = new HashSet<PathNode>();
            }

            public void Reset(int priority)
            {
                PendingChildren = new HashSet<PathNode>(Children);

                Priority = 0;
                StartIndex = 0;
            }

            public double LocateNearestStartPoint(Point point)
            {
                double nearestDistanceSquared = double.MaxValue;

                for (int i = 0; i < Path.Points.Count; i++)
                {
                    if (!Path.Closed && i == 1) i = Path.Points.Count - 1;

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
