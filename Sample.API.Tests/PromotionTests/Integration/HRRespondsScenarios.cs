using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Sample.API.PromotionModule;
using static Sample.API.PromotionModule.Promotion;
using static Sample.API.PromotionModule.PromotionFact;

namespace Sample.API.Tests.PromotionTests.Integration;

public class HRRespondsScenarios
{
    [Test]
    public async Task When_HR_Aprroves()
    {
        var host = TestSetup.GetAlbaHost();
        var store = host.Services.GetRequiredService<IDocumentStore>();
        var session = host.Services.GetRequiredService<IDocumentSession>();

        var id = await SetupPromotionAsync(session);

        var supervisorRespondsResponse = await host.Scenario(_ =>
        {
            _.Post.Json(new HRResponds(id, 2, DateTimeOffset.UtcNow, true))
                .ToUrl("/promotion/hrResponse");
            _.StatusCodeShouldBeOk();
        });

        await store.Advanced.ResetAllData();
    }

    [Test]
    public async Task When_HR_Rejects()
    {
        var host = TestSetup.GetAlbaHost();
        var store = host.Services.GetRequiredService<IDocumentStore>();
        var session = host.Services.GetRequiredService<IDocumentSession>();

        var id = await SetupPromotionAsync(session);

        var supervisorRespondsResponse = await host.Scenario(_ =>
        {
            _.Post.Json(new HRResponds(id, 2, DateTimeOffset.UtcNow, false))
                .ToUrl("/promotion/hrResponse");
            _.StatusCodeShouldBeOk();
        });

        var promotion = await session.Events.AggregateStreamAsync<Promotion>(id);
        Assert.That(promotion is RejectedPromotion, Is.True);

        await store.Advanced.ResetAllData();
    }

    private async Task<Guid> SetupPromotionAsync(IDocumentSession session)
    {
        var id = Guid.NewGuid();
        session.Events.StartStream<Promotion>(id, new PromotionOpened(id, "JohDoe"));
        session.Events.Append(id, 1, new ApprovedBySupervisor(DateTimeOffset.UtcNow));
        await session.SaveChangesAsync();

        return id;
    }
}