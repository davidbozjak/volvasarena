namespace TestableAbstractions;

public interface IRandomProvider
{
    public int Next(int maxValue);
    
    public double NextDouble();
}

public class RandomProvider : IRandomProvider
{
    private readonly Random random = new();

    public int Next(int maxValue)
    {
        return this.random.Next(maxValue);
    }

    public double NextDouble()
    {
        return this.random.NextDouble();
    }
}