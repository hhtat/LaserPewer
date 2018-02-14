using LaserPewer.GA;
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

        private TravelCostEvaluator evaluator;
        private ISelector selector;
        private IProcreator procreator;
        private IMutator mutator;
        private GeneticOptimizer optimizer;

        private double maxFitness;
        private List<int> optimalPriorities;

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
                optimizer.Step(1, selector, procreator, mutator, evaluator, random);

                if (optimizer.CurrentPopulation.MaxFitness > maxFitness)
                {
                    maxFitness = optimizer.CurrentPopulation.MaxFitness;
                    Debug.WriteLine("FITNESS:" + maxFitness);
                    optimalPriorities.Clear();
                    optimalPriorities.AddRange(optimizer.CurrentPopulation.ReadOnlyIndividuals[0].Chromosome);
                    evolved = true;
                }
            }

            return evolved;
        }

        public MachinePath Generate(double power, double speed)
        {
            IReadOnlyList<PathNode> schedule = precedenceTree.PriorityOrderTraversal(optimalPriorities);
            return toMachinePath(schedule, power, speed);
        }

        private void ensureInitialized()
        {
            if (precedenceTree == null)
            {
                precedenceTree = new PathTree(paths);

                evaluator = new TravelCostEvaluator(precedenceTree);
                selector = new RouletteWheelSelector();
                procreator = new CloneProcreator();
                mutator = new SwapMutator(1);

                Population genesis = new Population();

                Individual individual0 = genesis.Append();
                IReadOnlyList<PathNode> nodes = precedenceTree.NearestNeighborTraversal();
                for (int i = 0; i < nodes.Count; i++) nodes[i].Reset(i);
                List<int> chromosome0 = individual0.GetChromosome();
                chromosome0.AddRange(precedenceTree.Nodes.Select(node => node.Priority));
                evaluator.Baseline(chromosome0);

                for (int i = 1; i < 100; i++)
                {
                    genesis.Append().GetChromosome().AddRange(Enumerable.Range(0, precedenceTree.Nodes.Count));
                }

                genesis.Freeze(evaluator);

                optimizer = new GeneticOptimizer(genesis);
                maxFitness = double.MinValue;
                optimalPriorities = new List<int>();
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
            private readonly List<PathNode> _nodes;
            public IReadOnlyList<PathNode> Nodes { get { return _nodes; } }

            public PathTree(IReadOnlyList<Path> paths)
            {
                _nodes = paths.Select(path => new PathNode(path)).ToList();
                _nodes.Sort((a, b) => a.Path.BoundsArea.CompareTo(b.Path.BoundsArea));

                for (int i = 0; i < _nodes.Count; i++)
                {
                    PathNode nodeA = _nodes[i];
                    for (int j = i + 1; j < _nodes.Count; j++)
                    {
                        PathNode nodeB = _nodes[j];
                        if (nodeB.Path.Bounds.Contains(nodeA.Path.Bounds) && nodeB.Path.Bounds != nodeA.Path.Bounds)
                        {
                            nodeB.Children.Add(nodeA);
                            nodeA.Parents.Add(nodeB);
                        }
                    }
                }
            }

            public IReadOnlyList<PathNode> PriorityOrderTraversal(IReadOnlyList<int> priorities)
            {
                for (int i = 0; i < _nodes.Count; i++)
                {
                    _nodes[i].Reset(priorities[i]);
                }

                PriorityQueue<PathNode> queue = new PriorityQueue<PathNode>((a, b) => a.Priority.CompareTo(b.Priority));
                foreach (PathNode node in _nodes.Where(node => node.PendingChildren.Count == 0))
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

            public IReadOnlyList<PathNode> NearestNeighborTraversal()
            {
                for (int i = 0; i < _nodes.Count; i++)
                {
                    _nodes[i].Reset(0);
                }

                HashSet<PathNode> readySet = new HashSet<PathNode>(_nodes.Where(node => node.PendingChildren.Count == 0));

                List<PathNode> traversal = new List<PathNode>();
                Point lastEndPoint = new Point(0.0, 0.0);

                while (readySet.Count > 0)
                {
                    PathNode node = findNearest(lastEndPoint, readySet);
                    readySet.Remove(node);

                    traversal.Add(node);
                    foreach (PathNode parent in node.Parents)
                    {
                        parent.PendingChildren.Remove(node);
                        if (parent.PendingChildren.Count == 0) readySet.Add(parent);
                    }

                    lastEndPoint = node.Path.Points[node.EndIndex];
                }

                return traversal;
            }

            private static PathNode findNearest(Point point, IEnumerable<PathNode> nodes)
            {
                PathNode nearest = null;
                double minDistanceSquared = double.MaxValue;

                foreach (PathNode node in nodes)
                {
                    double distanceSquared = node.LocateNearestStartPoint(point);
                    if (distanceSquared < minDistanceSquared)
                    {
                        nearest = node;
                        minDistanceSquared = distanceSquared;
                    }
                }

                return nearest;
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

        private class TravelCostEvaluator : IEvaluator
        {
            private readonly PathTree tree;
            private double baseline;

            public TravelCostEvaluator(PathTree tree)
            {
                this.tree = tree;
            }

            public void Baseline(IReadOnlyList<int> priorities)
            {
                baseline = calculateTravelCost(tree.PriorityOrderTraversal(priorities));
            }

            public double Evaluate(IReadOnlyIndividual individual)
            {
                return Math.Max(0.0, baseline - calculateTravelCost(tree.PriorityOrderTraversal(individual.Chromosome)));
            }
        }

        private class RandomWriteMutator : IMutator
        {
            public List<int> Mutate(List<int> chromosome, Random random)
            {
                int mutations = 1 + random.Next(10);
                for (int j = 0; j < mutations; j++)
                {
                    chromosome[random.Next(chromosome.Count)] = random.Next(chromosome.Count);
                }
                return chromosome;
            }
        }
    }
}
