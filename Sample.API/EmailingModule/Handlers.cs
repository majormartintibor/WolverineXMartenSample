using Sample.API.Contracts.PromotionExternals.Emailing;
using System.Diagnostics;

namespace Sample.API.EmailingModule;

public static class PromotionAcceptedHandler
{
    public static void Handle(PromotionAccepted sendPromotionAcceptedNotification)
    {
        Debug.WriteLine($"Acceptance Email sent for for {sendPromotionAcceptedNotification.Promotee}!");
    }
}

public static class PromotionRejectedHandler
{
    public static void Handle(PromotionRejected sendPromotionRejectedNotification)
    {
        Debug.WriteLine($"Rejection Email sent for for {sendPromotionRejectedNotification.Promotee}!");
    }
}