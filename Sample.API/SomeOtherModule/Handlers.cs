using Sample.API.Contracts;
using System.Diagnostics;

namespace Sample.API.SomeOtherModule;

public static class SendPromotionAcceptedConfirmationHandler
{
    public static void Handle(SendPromotionAcceptedNotification sendPromotionAcceptedNotification)
    {
        Debug.WriteLine($"Promotion for {sendPromotionAcceptedNotification.Promotee} has been approved!");
    }
}

public static class SendPromotionRejectedNotificationHandler
{
    public static void Handle(SendPromotionRejectedNotification sendPromotionRejectedNotification)
    {
        Debug.WriteLine($"Promotion for {sendPromotionRejectedNotification.Promotee} has been rejected!");
    }
}