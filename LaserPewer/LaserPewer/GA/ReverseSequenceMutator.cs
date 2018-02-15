using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class ReverseSequenceMutator : IMutator
    {
        public void Mutate(List<int> chromosome, Random random)
        {
            int pointA = random.Next(chromosome.Count);
            int pointB = random.Next(chromosome.Count);
            chromosome.Reverse(Math.Min(pointA, pointB), Math.Abs(pointA - pointB));
        }
    }
}
