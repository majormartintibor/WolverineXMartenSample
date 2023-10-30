using Sample.API.Contracts.PromotionExternals.Marketing;
using System.Diagnostics;

namespace Sample.API.MarketingModule;

public static class PromotionAcceptedHandler
{
    public static void Handle(PromotionAccepted sendPromotionAcceptedNotification)
    {
        Debug.WriteLine($"Promotion for {sendPromotionAcceptedNotification.Promotee} has been approved!");
    }
}

public static class PromotionRejectedHandler
{
    public static void Handle(PromotionRejected sendPromotionRejectedNotification)
    {
        Debug.WriteLine($"Promotion for {sendPromotionRejectedNotification.Promotee} has been rejected!");
    }
}