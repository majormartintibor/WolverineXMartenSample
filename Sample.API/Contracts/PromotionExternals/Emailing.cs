namespace Sample.API.Contracts.PromotionExternals.Emailing;

public sealed record PromotionAccepted(string Promotee);

public sealed record PromotionRejected(string Promotee);