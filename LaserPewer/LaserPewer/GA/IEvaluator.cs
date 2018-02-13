namespace LaserPewer.GA
{
    public interface IEvaluator
    {
        void Initialize(IReadOnlyPopulation population);
        double Evaluate(IReadOnlyIndividual individual);
    }
}
