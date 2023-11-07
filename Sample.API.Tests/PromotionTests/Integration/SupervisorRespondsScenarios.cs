using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Sample.API.PromotionModule;

namespace Sample.API.Tests.PromotionTests.Integration;
public class SupervisorRespondsScenarios
{
    [Test]
    public async Task When_Supervisor_Aprroves()
    {
        var host = TestSetup.GetAlbaHost();

        //Showing that you can make multiple HTTP calls within 1 test
        //An alternative would be to seed data before the test starts
        //Todo -> show example with seeded data
        var requestPromotionResponse = await host.Scenario(_ =>
        {
            _.Post.Json(new RequestPromotion("Promotee")).ToUrl("/promotion/new");
            _.StatusCodeShouldBeOk();
        });
        var id = await requestPromotionResponse.ReadAsJsonAsync<Guid>();

        var supervisorRespondsResponse = await host.Scenario(_ =>
        {
            _.Post.Json(new SupervisorResponds(id, 1, DateTimeOffset.UtcNow, true)).ToUrl("/promotion/supervisorResponse");
            _.StatusCodeShouldBeOk();
        });

        // Wiping out any leftover data in the database
        var store = host.Services.GetRequiredService<IDocumentStore>();
        await store.Advanced.ResetAllData();
    }

    [Test]
    public async Task When_promotion_does_not_exist()
    {
        var host = TestSetup.GetAlbaHost();

        var supervisorRespondsResponse = await host.Scenario(_ =>
        {
            _.Post.Json(new SupervisorResponds(Guid.NewGuid(), 1, DateTimeOffset.UtcNow, true)).ToUrl("/promotion/supervisorResponse");
            _.StatusCodeShouldBe(500);
        });
    }
}