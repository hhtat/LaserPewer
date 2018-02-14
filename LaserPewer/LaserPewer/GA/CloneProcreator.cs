using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class CloneProcreator : IProcreator
    {
        public List<int> Procreate(List<int> chromosome, ISelector selector, Random random)
        {
            chromosome.AddRange(selector.Select(random).Chromosome);
            return chromosome;
        }
    }
}
