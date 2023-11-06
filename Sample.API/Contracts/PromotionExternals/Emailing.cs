namespace Sample.API.Contracts.PromotionExternals.Emailing;

public sealed record DoEmailingStuffWhenPromotionAccepted(string Promotee);

public sealed record DoEmailingStuffWhenPromotionRejected(string Promotee);