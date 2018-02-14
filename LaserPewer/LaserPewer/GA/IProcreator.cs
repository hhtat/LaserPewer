using System.Collections.Generic;

namespace LaserPewer.GA
{
    public interface IProcreator
    {
        List<int> Procreate(List<int> chromosome, ISelector selector);
    }
}
