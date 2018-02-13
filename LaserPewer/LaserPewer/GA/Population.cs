using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class Population : IReadOnlyPopulation
    {
        private readonly List<Individual> _individuals;
        public IReadOnlyList<Individual> Individuals { get; private set; }
        public IReadOnlyList<IReadOnlyIndividual> ReadOnlyIndividuals { get; private set; }

        private double _maxFitness;
        public double MaxFitness { get { if (frozen) return _maxFitness; else throw new InvalidOperationException(); } }

        private double _minFitness;
        public double MinFitness { get { if (frozen) return _minFitness; else throw new InvalidOperationException(); } }

        private bool frozen;
        private readonly Stack<Individual> pool;

        public Population()
        {
            _individuals = new List<Individual>();
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
            frozen = false;
        }

        public void Freeze(IEvaluator evaluator)
        {
            if (frozen) throw new InvalidOperationException();

            evaluator.Initialize(this);

            _maxFitness = double.MinValue;
            _minFitness = double.MaxValue;

            foreach (Individual individual in ReadOnlyIndividuals)
            {
                individual.Freeze(evaluator);

                if (individual.Fitness > _maxFitness) _maxFitness = individual.Fitness;
                if (individual.Fitness < _minFitness) _minFitness = individual.Fitness;
            }

            frozen = true;
        }

        public Individual CreateIndividual()
        {
            Individual individual;
            if (pool.Count > 0)
            {
                individual = pool.Pop();
                individual.Clear();
            }
            else
            {
                individual = new Individual();
            }
            return individual;
        }

        public void AddIndividual(Individual individual)
        {
            if (frozen) throw new InvalidOperationException();
            _individuals.Add(individual);
        }
    }

    public interface IReadOnlyPopulation
    {
        IReadOnlyList<IReadOnlyIndividual> ReadOnlyIndividuals { get; }
        double MaxFitness { get; }
        double MinFitness { get; }
    }
}
