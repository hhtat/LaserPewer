﻿using System;

namespace LaserPewer.GA
{
    public class GeneticOptimizer
    {
        public IReadOnlyPopulation CurrentPopulation { get; private set; }

        private Population pooledPopulation;

        public GeneticOptimizer(Population genesis)
        {
            if (!genesis.Frozen) throw new ArgumentException();

            CurrentPopulation = genesis;
            pooledPopulation = new Population();
        }

        public void Step(int survivors, ISelector selector, IProcreator procreator, IMutator mutator, IEvaluator evaluator, Random random)
        {
            pooledPopulation.Clear();

            for (int i = 0; i < survivors; i++)
            {
                pooledPopulation.Append().GetChromosome().AddRange(CurrentPopulation.ReadOnlyIndividuals[i].Chromosome);
            }

            selector.Initialize(CurrentPopulation);
            while (pooledPopulation.Individuals.Count < CurrentPopulation.ReadOnlyIndividuals.Count)
            {
                mutator.Mutate(procreator.Procreate(pooledPopulation.Append().GetChromosome(), selector, random), random);
            }

            pooledPopulation.Freeze(evaluator);

            Population temp = (Population)CurrentPopulation;
            CurrentPopulation = pooledPopulation;
            pooledPopulation = temp;
        }
    }
}
