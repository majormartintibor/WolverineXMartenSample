using Sample.API.Contracts.PromotionExternals.Controlling;
using System.Diagnostics;

namespace Sample.API.ControllingModule;

public static class PromotionAcceptedHandler
{
    public static void Handle(PromotionAccepted promotionAccepted)
    {
        Debug.WriteLine($"Promotion for {promotionAccepted.Promotee} has been approved! " +
            $"Approval times: Supervisor - {promotionAccepted.AcceptedBySupervisorDate}, " +
            $"HR - {promotionAccepted.AcceptedByHRDate}, " +
            $"CEO - {promotionAccepted.AcceptedByCEODate}.");
    }
}

public static class PromotionRejectedHandler
{
    public static void Handle(PromotionRejected promotionRejected)
    {
        Debug.WriteLine($"Promotion for {promotionRejected.Promotee} has been rejected at {promotionRejected.RejectionDate}!");
    }
}