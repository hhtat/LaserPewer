using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class SwapMutator : IMutator
    {
        public int Rounds { get; set; }

        public SwapMutator(int rounds)
        {
            Rounds = rounds;
        }

        public List<int> Mutate(List<int> chromosome, Random random)
        {
            for (int i = 0; i < Rounds; i++)
            {
                int indexA = random.Next(chromosome.Count);
                int indexB = random.Next(chromosome.Count);
                int temp = chromosome[indexA];
                chromosome[indexA] = chromosome[indexB];
                chromosome[indexB] = temp;
            }

            return chromosome;
        }
    }
}
