using Alba;
//using Marten;
//using Microsoft.Extensions.DependencyInjection;
using Oakton;

namespace Sample.API.Tests.PromotionTests.Integration;
[SetUpFixture]
public class TestSetup
{
    public static IAlbaHost Host { get; private set; }

    [OneTimeSetUp]
    public async Task Init()
    {
        //This is mandatory!
        OaktonEnvironment.AutoStartHost = true;

        // This is bootstrapping the actual application using
        // its implied Program.Main() set up
        Host = await AlbaHost.For<Program>();

        //Showing that you can put logic here that
        //executes after each HTTP execution

        //Host.AfterEach(async h =>
        //{
        //    // Wiping out any leftover data in the database
        //    var store = Host.Services.GetRequiredService<IDocumentStore>();
        //    await store.Advanced.ResetAllData();
        //});
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        Host.Dispose();
    }

    internal static IAlbaHost GetAlbaHost()
    {
        return Host;
    }
}