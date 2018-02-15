using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public interface ICrossover
    {
        void Crossover(List<int> child, IReadOnlyList<int> parentA, IReadOnlyList<int> parentB, Random random);
    }
}
