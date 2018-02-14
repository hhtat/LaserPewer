using System.Collections.Generic;

namespace LaserPewer.GA
{
    public interface IMutator
    {
        List<int> Mutate(List<int> chromosome);
    }
}
