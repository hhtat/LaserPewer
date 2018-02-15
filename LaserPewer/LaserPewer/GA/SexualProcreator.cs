using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class SexualProcreator : IProcreator
    {
        private readonly ICrossover crossover;
        private readonly IMutator mutator;
        private readonly double mutationRate;

        public SexualProcreator(ICrossover crossover, IMutator mutator, double mutationRate)
        {
            this.crossover = crossover;
            this.mutator = mutator;
            this.mutationRate = mutationRate;
        }

        public void Procreate(List<int> child, ISelector selector, Random random)
        {
            crossover.Crossover(child, selector.Select(random).Chromosome, selector.Select(random).Chromosome, random);
            //child.AddRange(selector.Select(random).Chromosome);
            if (mutationRate == 1.0 || random.NextDouble() < mutationRate) mutator.Mutate(child, random);
        }
    }
}
