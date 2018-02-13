namespace LaserPewer.GA
{
    public interface ISelector
    {
        void Initialize(IReadOnlyPopulation population);
        IReadOnlyIndividual Select();
    }
}
