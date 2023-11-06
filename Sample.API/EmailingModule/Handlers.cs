using Sample.API.Contracts.PromotionExternals.Emailing;
using System.Diagnostics;

namespace Sample.API.EmailingModule;

public static class DoEmailingStuffWhenPromotionAcceptedHandler
{
    public static void Handle(DoEmailingStuffWhenPromotionAccepted sendPromotionAcceptedNotification)
    {
        Debug.WriteLine($"Acceptance Email sent for for {sendPromotionAcceptedNotification.Promotee}!");
    }
}

public static class DoEmailingStuffWhenPromotionRejectedHandler
{
    public static void Handle(DoEmailingStuffWhenPromotionRejected sendPromotionRejectedNotification)
    {
        Debug.WriteLine($"Rejection Email sent for for {sendPromotionRejectedNotification.Promotee}!");
    }
}