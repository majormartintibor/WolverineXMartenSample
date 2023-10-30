namespace Sample.API.PromotionModule;

public sealed record PromotionAccepted(Guid PromotionId);

public sealed record PromotionRejected(Guid PromotionId);