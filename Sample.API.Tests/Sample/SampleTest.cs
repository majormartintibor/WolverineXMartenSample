using Alba;

namespace Sample.API.Tests.Sample;

[SetUpFixture]
public class TestSetup
{
    public static IAlbaHost Host { get; private set; }

    [OneTimeSetUp]
    public async Task Init()
    {
        Host = await AlbaHost.For<Program>();
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        Host.Dispose();
    }

    public static IAlbaHost GetAlbaHost()
    {
        return Host;
    }
}

public class SampleTest
{
    [Test]
    public async Task Should_say_hello_world()
    {
        var host = TestSetup.GetAlbaHost();

        // This runs an HTTP request and makes an assertion
        // about the expected content of the response
        await host.Scenario(_ =>
        {
            _.Get.Url("/");
            _.ContentShouldBe("Hello World!");
            _.StatusCodeShouldBeOk();
        });
    }
}