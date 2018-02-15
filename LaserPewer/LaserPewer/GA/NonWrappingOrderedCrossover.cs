using System;
using System.Collections.Generic;
using System.Linq;

namespace LaserPewer.GA
{
    public class NonWrappingOrderedCrossover : ICrossover
    {
        private readonly HashSet<int> setB;

        public NonWrappingOrderedCrossover()
        {
            setB = new HashSet<int>();
        }

        public void Crossover(List<int> childA, List<int> childB, IReadOnlyList<int> parentA, IReadOnlyList<int> parentB, Random random)
        {
            int minLength = Math.Min(parentA.Count, parentB.Count);
            int startPoint = random.Next(minLength);
            int endPoint = random.Next(minLength);
            if (startPoint > endPoint)
            {
                int temp = startPoint;
                startPoint = endPoint;
                endPoint = temp;
            }

            buildChild(childA, parentA, parentB, startPoint, endPoint);
            buildChild(childB, parentB, parentA, startPoint, endPoint);
        }

        public void Crossover(List<int> childA, List<int> childB, IReadOnlyList<int> parentA, IReadOnlyList<int> parentB, int startPoint, int endPoint)
        {
            buildChild(childA, parentA, parentB, startPoint, endPoint);
            buildChild(childB, parentB, parentA, startPoint, endPoint);
        }

        private void buildChild(List<int> childA, IReadOnlyList<int> parentA, IReadOnlyList<int> parentB, int startPoint, int endPoint)
        {
            setB.Clear();
            for (int i = startPoint; i < endPoint; i++)
            {
                setB.Add(parentB[i]);
            }

            foreach (int gene in parentA)
            {
                if (childA.Count == startPoint)
                {
                    while (childA.Count < endPoint)
                    {
                        childA.Add(parentB[childA.Count]);
                    }
                }

                if (!setB.Contains(gene))
                {
                    childA.Add(gene);
                }
            }
        }
    }
}
