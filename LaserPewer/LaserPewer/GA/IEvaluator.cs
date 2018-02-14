namespace LaserPewer.GA
{
    public interface IEvaluator
    {
        double Evaluate(IReadOnlyIndividual individual);
    }
}
