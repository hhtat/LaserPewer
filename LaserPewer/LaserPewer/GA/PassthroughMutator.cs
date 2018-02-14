using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public class PassthroughMutator : IMutator
    {
        public List<int> Mutate(List<int> chromosome, Random random)
        {
            return chromosome;
        }
    }
}
