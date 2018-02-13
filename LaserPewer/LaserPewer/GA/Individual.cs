using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class Individual : IReadOnlyIndividual, IComparable<Individual>
    {
        private readonly List<int> _chromosome;
        public IReadOnlyList<int> Chromosome { get; private set; }
        public double Fitness { get; private set; }

        bool frozen;

        public Individual()
        {
            _chromosome = new List<int>();
            Chromosome = _chromosome;
        }

        public void Clear()
        {
            _chromosome.Clear();
            Fitness = 0;
            frozen = false;
        }

        public void Freeze(IEvaluator evaluator)
        {
            if (frozen) throw new InvalidOperationException();
            Fitness = evaluator.Evaluate(this);
            frozen = true;
        }

        public List<int> GetChromosome()
        {
            if (frozen) throw new InvalidOperationException();
            return _chromosome;
        }

        public int CompareTo(Individual other)
        {
            return Fitness.CompareTo(other.Fitness);
        }
    }

    public interface IReadOnlyIndividual
    {
        IReadOnlyList<int> Chromosome { get; }
        double Fitness { get; }
    }
}
