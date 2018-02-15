using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class ReverseSequenceMutator : IMutator
    {
        public void Mutate(List<int> chromosome, Random random)
        {
            Interval interval = new Interval(random, chromosome.Count);
            chromosome.Reverse(interval.Start, interval.Length);
        }
    }
}
