using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Sample.API.PromotionModule;

namespace Sample.API.Tests.PromotionTests.Integration;
public class RequestPromotionScenarios
{
    [Test]
    public async Task RequestPromotion_should_return_id()
    {
        var host = TestSetup.GetAlbaHost();

        var response = await host.Scenario(_ =>
        {
            _.Post.Json(new RequestPromotion("Promotee")).ToUrl("/promotion/new");
            _.StatusCodeShouldBeOk();
        });

        var id = await response.ReadAsJsonAsync<Guid>();

        Assert.That(id.ToString(), Is.Not.Empty);

        // Wiping out any leftover data in the database
        var store = host.Services.GetRequiredService<IDocumentStore>();
        await store.Advanced.ResetAllData();
    }
}