using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public interface IProcreator
    {
        void Procreate(List<int> childA, List<int> childB, ISelector selector, Random random);
    }
}
