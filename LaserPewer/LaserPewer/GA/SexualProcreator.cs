using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class SexualProcreator : IProcreator
    {
        private readonly ICrossover crossover;
        private readonly IMutator mutator;

        public SexualProcreator(ICrossover crossover, IMutator mutator)
        {
            this.crossover = crossover;
            this.mutator = mutator;
        }

        public void Procreate(List<int> childA, List<int> childB, ISelector selector, Random random)
        {
            crossover.Crossover(childA, childB, selector.Select(random).Chromosome, selector.Select(random).Chromosome, random);
            mutator.Mutate(childA, random);
            mutator.Mutate(childB, random);
        }
    }
}
