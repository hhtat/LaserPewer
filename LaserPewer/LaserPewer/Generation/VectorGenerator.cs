using LaserPewer.Geometry;
using LaserPewer.Shared;
using LaserPewer.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace LaserPewer.Generation
{
    public class VectorGenerator
    {
        private readonly List<Path> paths;
        private readonly Random random;

        private PathTree precedenceTree;
        private int[] bestPriorities;
        private IReadOnlyList<PathNode> bestSchedule;
        private double lowestCost;

        public VectorGenerator(IReadOnlyList<Path> paths)
        {
            this.paths = paths.ToList();
            random = new Random();
        }

        public bool Step(TimeSpan timeout)
        {
            bool evolved = false;

            StopWatch stopWatch = new StopWatch();

            ensureInitialized();

            while (!stopWatch.Expired(timeout))
            {
                int[] priorities = new int[precedenceTree.Size];
                Array.Copy(bestPriorities, priorities, priorities.Length);
                int mutations = 1 + random.Next(10);
                for (int j = 0; j < mutations; j++)
                {
                    priorities[random.Next(priorities.Length)] = random.Next();
                }

                IReadOnlyList<PathNode> schedule = precedenceTree.Traverse(priorities);
                double cost = calculateTravelCost(schedule);

                if (cost < lowestCost)
                {
                    bestPriorities = priorities;
                    bestSchedule = schedule;
                    lowestCost = cost;
                    Debug.WriteLine("COST:" + lowestCost);
                    evolved = true;
                }
            }

            return evolved;
        }

        public MachinePath Generate(double power, double speed)
        {
            return toMachinePath(bestSchedule, power, speed);
        }

        private void ensureInitialized()
        {
            if (precedenceTree == null)
            {
                precedenceTree = new PathTree(paths);

                bestPriorities = new int[precedenceTree.Size];
                for (int i = 0; i < bestPriorities.Length; i++) bestPriorities[i] = random.Next();
                bestSchedule = null;
                lowestCost = double.MaxValue;
            }
        }

        private static double calculateTravelCost(IReadOnlyList<PathNode> schedule)
        {
            double travelCost = 0.0;
            Point lastEndPoint = new Point(0.0, 0.0);

            foreach (PathNode node in schedule)
            {
                travelCost += node.LocateNearestStartPoint(lastEndPoint);
                lastEndPoint = node.Path.Points[node.EndIndex];
            }

            return travelCost;
        }

        private static MachinePath toMachinePath(IReadOnlyList<PathNode> schedule, double power, double speed)
        {
            MachinePath machinePath = new MachinePath();

            machinePath.SetPowerAndSpeed(power, speed);
            Point lastEndPoint = new Point(0.0, 0.0);

            foreach (PathNode node in schedule)
            {
                node.LocateNearestStartPoint(lastEndPoint);

                if (node.Path.Closed)
                {
                    for (int i = 0; i <= node.Path.Points.Count; i++)
                    {
                        machinePath.TravelTo(node.Path.Points[(node.StartIndex + i) % node.Path.Points.Count]);
                    }
                }
                else if (node.StartIndex == 0)
                {
                    for (int i = 0; i < node.Path.Points.Count; i++)
                    {
                        machinePath.TravelTo(node.Path.Points[i]);
                    }
                }
                else
                {
                    for (int i = node.Path.Points.Count - 1; i >= 0; i--)
                    {
                        machinePath.TravelTo(node.Path.Points[i]);
                    }
                }

                machinePath.EndCut();
                lastEndPoint = node.Path.Points[node.EndIndex];
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
                        parent.PendingChildren.Remove(node);
                        if (parent.PendingChildren.Count == 0) queue.Enqueue(parent);
                    }
                }

                return traversal;
            }
        }

        private class PathNode
        {
            public readonly Path Path;

            public readonly ISet<PathNode> Parents;
            public readonly ISet<PathNode> Children;

            public ISet<PathNode> PendingChildren { get; private set; }

            public int Priority { get; private set; }
            public int StartIndex { get; private set; }
            public int EndIndex { get { return Path.Closed ? StartIndex : (StartIndex == 0 ? Path.Points.Count - 1 : 0); } }

            public PathNode(Path path)
            {
                Path = path;

                Parents = new HashSet<PathNode>();
                Children = new HashSet<PathNode>();
            }

            public void Reset(int priority)
            {
                PendingChildren = new HashSet<PathNode>(Children);

                Priority = priority;
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
