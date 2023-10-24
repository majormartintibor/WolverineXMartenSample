using Marten;
using NSubstitute;
using Sample.API.PromotionModule;

namespace Sample.API.Tests.PromotionTests.UnitTests.Handlers;
public class RequestPromotion
{
    [Test]
    public async Task Handle_RequestPromotion()
    {
        var message = new PromotionModule.RequestPromotion("TestUser");
        var session = Substitute.For<IDocumentSession>();

        var id = await RequestPromotionHandler.Handle(message, session);

        Assert.IsNotEmpty(id.ToString());
    }
}