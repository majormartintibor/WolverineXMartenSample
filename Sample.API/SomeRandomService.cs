using System.Diagnostics;

namespace Sample.API;

public interface ISomeRandomService
{
    void DoSomething();
}

//Normally I would make implementation internal, but Wolverine requires it to be public.
//There is some code generation going on with Wolverine that increases performance but
//the trade off is that implementations have to be public.
public sealed class SomeRandomService : ISomeRandomService
{
    public void DoSomething()
    {
        Debug.WriteLine("I did something");
    }
}