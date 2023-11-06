namespace Sample.API.Contracts.PromotionExternals.Marketing;

public sealed record DoMarketingStuffWhenPromotionAccepted(string Promotee);

public sealed record DoMarketingStuffWhenPromotionRejected(string Promotee);