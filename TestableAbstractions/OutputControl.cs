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

public class FileOutputControl : IOutputControl
{
    private readonly StreamWriter streamWriter;

    public FileOutputControl(StreamWriter streamWriter)
    {
        this.streamWriter = streamWriter;
    }

    public void Clear()
    {
        // NA
    }

    public void Write(string value)
    {
        this.streamWriter.Write(value);
    }

    public void WriteLine(string value)
    {
        this.streamWriter.WriteLine(value);
    }
}