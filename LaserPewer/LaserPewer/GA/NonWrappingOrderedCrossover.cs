using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            Interval interval = new Interval(random, Math.Min(parentA.Count, parentB.Count));

            buildChild(childA, parentA, parentB, interval);
            buildChild(childB, parentB, parentA, interval);
        }

        private void buildChild(List<int> childA, IReadOnlyList<int> parentA, IReadOnlyList<int> parentB, Interval interval)
        {
            setB.Clear();
            for (int i = interval.Start; i < interval.End; i++)
            {
                setB.Add(parentB[i]);
            }

            int indexA = 0;

            while (childA.Count < interval.Start && indexA < parentA.Count)
            {
                if (!setB.Contains(parentA[indexA]))
                {
                    childA.Add(parentA[indexA]);
                }

                indexA++;
            }

            for (int i = interval.Start; i < interval.End; i++)
            {
                childA.Add(parentB[i]);
            }

            while (indexA < parentA.Count)
            {
                if (!setB.Contains(parentA[indexA]))
                {
                    childA.Add(parentA[indexA]);
                }

                indexA++;
            }
        }
    }
}
