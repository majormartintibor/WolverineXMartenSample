﻿namespace Sample.API.PromotionFeature;

public abstract record PromotionFact
{
    public sealed record PromotionRequested(Guid PromotionId, string Promotee) : PromotionFact;
    public sealed record ApprovedBySupervisor(DateTimeOffset ApprovedAt) : PromotionFact;
    public sealed record RejectedBySupervisor(DateTimeOffset? RejectedAt) : PromotionFact;
    public sealed record ApprovedByHR(DateTimeOffset ApprovedAt) : PromotionFact;
    public sealed record RejectedByHR(DateTimeOffset RejectedAt) : PromotionFact;
    public sealed record ApprovedByCEO(DateTimeOffset ApprovedAt) : PromotionFact;
    public sealed record RejectedByCEO(DateTimeOffset RejectedAt) : PromotionFact;
    public sealed record PromotionClosedWithRejection() : PromotionFact;
    public sealed record PromotionClosedWithAcceptance() : PromotionFact;
}