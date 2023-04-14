namespace TestableAbstractions;

public interface IDateTimeProvider
{
    public DateTime Now { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now { get => DateTime.Now; }
}