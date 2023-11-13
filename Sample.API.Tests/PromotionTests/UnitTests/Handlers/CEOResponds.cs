using Emailing = Sample.API.Contracts.PromotionExternals.Emailing;
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
        //Arrange
        var id = Guid.NewGuid();
        var approvalDate = DateTime.UtcNow;
        var promotion = new PassedHRApproval() with 
        { 
            Promotee = "TestUser", 
            ApprovedBySupervisor  = DateTimeOffset.MinValue,
            ApprovedByHR = DateTimeOffset.MinValue
        };        
        var message = new PromotionModule.CEOResponds(id, default, approvalDate, true);        

        //Act       
        (var events, var outgoingMessages) = CEORespondsHandler.Handle(message, promotion);

        //Assert
        events.ShouldHaveMessageOfType<ApprovedByCEO>()
            .ApprovedAt.ShouldBe(approvalDate);
        events.ShouldHaveMessageOfType<PromotionClosedWithAcceptance>();

        outgoingMessages.ShouldHaveMessageOfType<Emailing.DoEmailingStuffWhenPromotionAccepted>()
            .Promotee.ShouldBe(promotion.Promotee);        
    }

    [Test]
    public void Handle_CEOResponds_with_rejection()
    {
        //Arrange
        var id = Guid.NewGuid();
        var rejectionDate = DateTime.UtcNow;
        var promotion = new PassedHRApproval() with { Promotee = "TestUser" };
        var message = new PromotionModule.CEOResponds(id, default, rejectionDate, false);

        //Act
        (var events, var outgoingMessages) = CEORespondsHandler.Handle(message, promotion);

        //Assert
        events.ShouldHaveMessageOfType<RejectedByCEO>()
            .RejectedAt.ShouldBe(rejectionDate);
        events.ShouldHaveMessageOfType<PromotionClosedWithRejection>();

        outgoingMessages.ShouldHaveMessageOfType<Emailing.DoEmailingStuffWhenPromotionRejected>()
            .Promotee.ShouldBe(promotion.Promotee);        
    }
}