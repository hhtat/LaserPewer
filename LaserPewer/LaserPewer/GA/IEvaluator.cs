using System.Collections.Generic;

namespace LaserPewer.GA
{
    public interface IEvaluator
    {
        double Evaluate(IReadOnlyList<int> chromosome);
    }
}
