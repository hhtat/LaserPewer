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

        public void Mutate(List<int> chromosome, Random random)
        {
            for (int round = 0; round < Rounds; round++)
            {
                int indexA = random.Next(chromosome.Count);
                int indexB = random.Next(chromosome.Count);
                int temp = chromosome[indexA];
                chromosome[indexA] = chromosome[indexB];
                chromosome[indexB] = temp;
            }
        }
    }
}
