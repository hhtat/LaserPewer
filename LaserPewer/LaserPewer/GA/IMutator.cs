using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public interface IMutator
    {
        void Mutate(List<int> chromosome, Random random);
    }
}
