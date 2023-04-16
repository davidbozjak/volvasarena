namespace TestableAbstractions;

public interface IUserInputControl
{
    bool GetInputFromUser<T>(string prompt, Func<string, bool> validationFunc, Func<string, T> convertFunc, out T value);

    ConsoleKeyInfo ReadKey();
}

public class ConsoleInputControl : IUserInputControl
{
    private readonly IOutputControl outputControl;
    private readonly Func<IUserInputControl, bool>? retryPrompt;

    public ConsoleInputControl(IOutputControl outputControl, Func<IUserInputControl, bool>? retryPrompt = null)
    {
        this.outputControl = outputControl;
        this.retryPrompt = retryPrompt;
    }   

    public ConsoleKeyInfo ReadKey()
        => Console.ReadKey();

    public bool GetInputFromUser<T>(string prompt, Func<string, bool> validationFunc, Func<string, T> convertFunc, out T value)
    {
        bool retry;
        string input;

        do
        {
            this.outputControl.WriteLine(prompt);
            input = Console.ReadLine() ?? "";

            if (validationFunc(input))
            {
                value = convertFunc(input);
                return true;
            }
            else
            {
                retry = this.retryPrompt?.Invoke(this) ?? false;
                value = default;
            }
        } while (retry);

        return false;
    }
}