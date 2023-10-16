namespace Sample.API.Contracts;

public sealed record SendPromotionAcceptedNotification(string Promotee);

public sealed record SendPromotionRejectedNotification(string Promotee);