using Sample.API.Contracts.PromotionExternals.Marketing;
using System.Diagnostics;

namespace Sample.API.MarketingModule;

public static class DoMarketingStuffWhenPromotionAcceptedHandler
{
    public static void Handle(DoMarketingStuffWhenPromotionAccepted sendPromotionAcceptedNotification)
    {
        Debug.WriteLine($"Promotion for {sendPromotionAcceptedNotification.Promotee} has been approved!");
    }
}

public static class DoMarketingStuffWhenPromotionRejectedHandler
{
    public static void Handle(DoMarketingStuffWhenPromotionRejected sendPromotionRejectedNotification)
    {
        Debug.WriteLine($"Promotion for {sendPromotionRejectedNotification.Promotee} has been rejected!");
    }
}