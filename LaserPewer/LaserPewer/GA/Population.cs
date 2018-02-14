using System;
using System.Collections.Generic;
using System.Linq;

namespace LaserPewer.GA
{
    public class Population : IReadOnlyPopulation
    {
        public bool Frozen { get; private set; }

        private readonly List<Individual> _individuals;
        public IReadOnlyList<Individual> Individuals { get; private set; }
        public IReadOnlyList<IReadOnlyIndividual> ReadOnlyIndividuals { get; private set; }

        private double _maxFitness;
        public double MaxFitness { get { if (Frozen) return _maxFitness; else throw new InvalidOperationException(); } }

        private double _totalFitness;
        public double TotalFitness { get { if (Frozen) return _totalFitness; else throw new InvalidOperationException(); } }

        private readonly Stack<Individual> pool;

        public Population()
        {
            _individuals = new List<Individual>();
            Individuals = _individuals;
            ReadOnlyIndividuals = _individuals;
            pool = new Stack<Individual>();
        }

        public void Clear()
        {
            foreach (Individual individual in _individuals)
            {
                pool.Push(individual);
            }

            _individuals.Clear();

            Frozen = false;
        }

        public void Freeze(IEvaluator evaluator)
        {
            if (Frozen) throw new InvalidOperationException();

            _maxFitness = double.MinValue;
            _totalFitness = 0.0;
            foreach (Individual individual in ReadOnlyIndividuals)
            {
                individual.Freeze(evaluator);
                if (individual.Fitness > _maxFitness) _maxFitness = individual.Fitness;
                _totalFitness += individual.Fitness;
            }

            _individuals.Sort();

            Frozen = true;
        }

        public Individual Append()
        {
            if (Frozen) throw new InvalidOperationException();
            Individual individual = createIndividual();
            _individuals.Add(individual);
            return individual;
        }

        private Individual createIndividual()
        {
            if (pool.Count > 0)
            {
                Individual individual = pool.Pop();
                individual.Clear();
                return individual;
            }

            return new Individual();
        }
    }

    public interface IReadOnlyPopulation
    {
        bool Frozen { get; }
        IReadOnlyList<IReadOnlyIndividual> ReadOnlyIndividuals { get; }
        double MaxFitness { get; }
        double TotalFitness { get; }
    }
}
