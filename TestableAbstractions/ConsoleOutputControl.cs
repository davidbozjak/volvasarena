namespace TestableAbstractions;

public interface IOutputControl
{
    void Write(string value);

    void WriteLine(string value);

    void Clear();
}

public class ConsoleOutputControl : IOutputControl
{
    public void Write(string value)
    {
        Console.Write(value);
    }

    public void WriteLine(string value)
    {
        Console.WriteLine(value);
    }

    public void Clear()
    {
        Console.Clear();
    }
}