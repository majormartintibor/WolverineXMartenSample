using Sample.API.Contracts;
using Sample.API.PromotionModule;
using Shouldly;
using Wolverine;
using static Sample.API.PromotionModule.Promotion;
using static Sample.API.PromotionModule.PromotionFact;

namespace Sample.API.Tests.PromotionTests.UnitTests.Handlers;
public class CEOResponds
{
    [Test]
    public void Handle_CEOResponds_with_approval()
    {
        var id = Guid.NewGuid();
        var approvalDate = DateTime.UtcNow;

        var promotion = new PassedHRApproval() with { Promotee = "TestUser"};
        var message = new PromotionModule.CEOResponds(id, 4 /*can be anything here*/, approvalDate, true);        

        var messages = CEORespondsHandler.Handle(message, promotion);

        messages.Item1.ShouldHaveMessageOfType<ApprovedByCEO>()
            .ApprovedAt.ShouldBe(approvalDate);
        messages.Item1.ShouldHaveMessageOfType<PromotionClosedWithAcceptance>();
        messages.Item2.ShouldHaveMessageOfType<SendPromotionAcceptedNotification>()
            .Promotee.ShouldBe("TestUser");
    }

    [Test]
    public void Handle_CEOResponds_with_rejection()
    {
        var id = Guid.NewGuid();
        var approvalDate = DateTime.UtcNow;

        var promotion = new PassedHRApproval() with { Promotee = "TestUser" };
        var message = new PromotionModule.CEOResponds(id, 4, approvalDate, false);        

        var messages = CEORespondsHandler.Handle(message, promotion);

        messages.Item1.ShouldHaveMessageOfType<RejectedByCEO>()
            .RejectedAt.ShouldBe(approvalDate);
        messages.Item1.ShouldHaveMessageOfType<PromotionClosedWithRejection>();
        messages.Item2.ShouldHaveMessageOfType<SendPromotionRejectedNotification>()
            .Promotee.ShouldBe("TestUser");
    }
}