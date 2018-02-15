using System;
using System.Collections.Generic;

namespace LaserPewer.GA
{
    public interface IProcreator
    {
        void Procreate(List<int> child, ISelector selector, Random random);
    }
}
