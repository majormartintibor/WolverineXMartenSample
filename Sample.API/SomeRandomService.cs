using System.Diagnostics;

namespace Sample.API;

public interface ISomeRandomService
{
    void DoSomething();
}

public class SomeRandomService : ISomeRandomService
{
    public void DoSomething()
    {
        Debug.WriteLine("I did something");
    }
}