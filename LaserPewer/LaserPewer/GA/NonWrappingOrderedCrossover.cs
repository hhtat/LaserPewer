using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class NonWrappingOrderedCrossover : ICrossover
    {
        private readonly HashSet<int> setB;

        public NonWrappingOrderedCrossover()
        {
            setB = new HashSet<int>();
        }

        public void Crossover(List<int> child, IReadOnlyList<int> parentA, IReadOnlyList<int> parentB, Random random)
        {
            Interval interval = new Interval(random, Math.Min(parentA.Count, parentB.Count));

            setB.Clear();
            for (int i = interval.Start; i < interval.End; i++)
            {
                setB.Add(parentB[i]);
            }

            int indexA = 0;

            while (child.Count < interval.Start && indexA < parentA.Count)
            {
                if (!setB.Contains(parentA[indexA]))
                {
                    child.Add(parentA[indexA]);
                }

                indexA++;
            }

            for (int i = interval.Start; i < interval.End; i++)
            {
                child.Add(parentB[i]);
            }

            while (indexA < parentA.Count)
            {
                if (!setB.Contains(parentA[indexA]))
                {
                    child.Add(parentA[indexA]);
                }

                indexA++;
            }
        }
    }
}
