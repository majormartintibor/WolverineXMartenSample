using static Sample.API.PromotionModule.Promotion;
using static Sample.API.PromotionModule.PromotionFact;

namespace Sample.API.Tests.PromotionTests.UnitTests;
public class Promotion
{
    [Test]
    public void Apply_PromotionOpened_returns_OpenedPromotion()
    {
        var id = Guid.NewGuid();

        var promotion = GetEmptyPromotion();
        var openPromotion = promotion.Apply(new PromotionOpened(id, "TestUser"));

        Assert.IsTrue(openPromotion is OpenPromotion);
        Assert.That(openPromotion.Id, Is.EqualTo(id));
        Assert.That(openPromotion.Promotee, Is.EqualTo("TestUser"));
    }

    [Test]
    public void Apply_ApprovedBySupervisor_returns_PassedSupervisorApproval()
    {
        var approvedAt = DateTimeOffset.UtcNow;

        var promotion = new OpenPromotion() with { Id = Guid.NewGuid(), Promotee = "TestUser" };
        var passedSupervisorApproval = promotion.Apply(new ApprovedBySupervisor(approvedAt));

        Assert.IsTrue(passedSupervisorApproval is PassedSupervisorApproval);
        Assert.That(passedSupervisorApproval.ApprovedBySupervisor, Is.EqualTo(approvedAt));
    }

    [Test]
    public void When_this_is_not_OpenedPromotion_apply_ApprovedBySupervisor_returns_this()
    {
        var approvedAt = DateTimeOffset.UtcNow;

        var @this = new RejectedPromotion();
        var result = @this.Apply(new ApprovedBySupervisor(approvedAt));

        Assert.IsTrue(@this is RejectedPromotion);
        Assert.IsNull(@this.ApprovedBySupervisor);
    }
}