namespace Sample.API;

public interface IDateTimeOffsetProvider
{
    public DateTimeOffset GetCurrentDateTimeOffset();
}

public class DateTimeOffsetProvider : IDateTimeOffsetProvider
{
    public DateTimeOffset GetCurrentDateTimeOffset()
    {
        return DateTimeOffset.Now;
    }
}