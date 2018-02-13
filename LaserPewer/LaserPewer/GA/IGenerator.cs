using System.Collections.Generic;

namespace LaserPewer.GA
{
    public interface IGenerator
    {
        void Initialize(IReadOnlyPopulation population);
        bool HasMore();
        void Generate(List<int> chromosome);
    }
}
