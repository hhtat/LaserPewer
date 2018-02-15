using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public interface ICrossover
    {
        void Crossover(List<int> childA, List<int> childB, IReadOnlyList<int> parentA, IReadOnlyList<int> parentB, Random random);
    }
}
