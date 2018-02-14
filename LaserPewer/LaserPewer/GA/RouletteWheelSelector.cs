using System;
using System.Collections.Generic;
using System.Linq;

namespace LaserPewer.GA
{
    public class RouletteWheelSelector : ISelector
    {
        private IReadOnlyList<IReadOnlyIndividual> individuals;

        private readonly List<double> probabilities;
        private readonly List<int> aliases;

        private readonly List<double> normalizedProbabilities;
        private readonly Queue<int> overfullGroup;
        private readonly Stack<int> underfullGroup;

        public RouletteWheelSelector()
        {
            probabilities = new List<double>();
            aliases = new List<int>();

            normalizedProbabilities = new List<double>();
            overfullGroup = new Queue<int>();
            underfullGroup = new Stack<int>();
        }

        public void Initialize(IReadOnlyPopulation population)
        {
            if (!population.Frozen) throw new ArgumentException();

            individuals = population.ReadOnlyIndividuals;

            normalizedProbabilities.Clear();
            foreach (Individual individual in individuals)
            {
                normalizedProbabilities.Add(individual.Fitness / population.TotalFitness);
            }

            buildAliasTables(normalizedProbabilities);
        }

        public IReadOnlyIndividual Select(Random random)
        {
            return individuals[sampleIndex(random)];
        }

        private void buildAliasTables(IReadOnlyList<double> normalizedProbabilities)
        {
            probabilities.Clear();
            aliases.Clear();

            overfullGroup.Clear();
            underfullGroup.Clear();

            for (int i = 0; i < normalizedProbabilities.Count; i++)
            {
                double probability = normalizedProbabilities.Count * normalizedProbabilities[i];

                probabilities.Add(probability);
                aliases.Add(i);

                if (probability > 1.0) overfullGroup.Enqueue(i);
                else if (probability < 1.0) underfullGroup.Push(i);
            }

            while (overfullGroup.Count > 0 && underfullGroup.Count > 0)
            {
                int i = overfullGroup.Peek();
                int j = underfullGroup.Pop();
                aliases[j] = i;
                probabilities[i] -= (1.0 - probabilities[j]);
                if (probabilities[i] == 1.0) overfullGroup.Dequeue();
                else if (probabilities[i] < 1.0) underfullGroup.Push(overfullGroup.Dequeue());
            }

            while (overfullGroup.Count > 0) probabilities[overfullGroup.Dequeue()] = 1.0;
            while (underfullGroup.Count > 0) probabilities[underfullGroup.Pop()] = 1.0;
        }

        private int sampleIndex(Random random)
        {
            int i = random.Next(probabilities.Count);
            if (probabilities[i] == 1.0) return i;
            if (probabilities[i] == 0.0) return aliases[i];
            return random.NextDouble() < probabilities[i] ? i : aliases[i];
        }
    }
}
