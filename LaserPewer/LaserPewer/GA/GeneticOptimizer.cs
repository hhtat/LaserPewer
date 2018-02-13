using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class GeneticOptimizer
    {
        public IEvaluator Evaluator { get; set; }
        public IReadOnlyPopulation CurrentPopulation { get; private set; }

        private Population pooledPopulation;

        public GeneticOptimizer(IEvaluator evaluator)
        {
            Evaluator = evaluator;
            CurrentPopulation = new Population();
            pooledPopulation = new Population();
        }

        public void Step(IEnumerable<IGenerator> generators)
        {
            pooledPopulation.Clear();

            foreach (IGenerator generator in generators)
            {
                generator.Initialize(CurrentPopulation);
                while (generator.HasMore())
                {
                    Individual individual = pooledPopulation.CreateIndividual();
                    generator.Generate(individual.GetChromosome());
                    pooledPopulation.AddIndividual(individual);
                }
            }

            pooledPopulation.Freeze(Evaluator);

            IReadOnlyPopulation temp = CurrentPopulation;
            CurrentPopulation = pooledPopulation;
            pooledPopulation = (Population)temp;
        }
    }
}
