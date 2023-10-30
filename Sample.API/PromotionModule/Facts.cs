namespace Sample.API.PromotionModule;

public abstract record PromotionFact
{
    public sealed record PromotionOpened(Guid PromotionId, string Promotee) : PromotionFact;
    public sealed record ApprovedBySupervisor(DateTimeOffset ApprovedAt) : PromotionFact;
    public sealed record RejectedBySupervisor(DateTimeOffset? RejectedAt) : PromotionFact;
    public sealed record ApprovedByHR(DateTimeOffset ApprovedAt) : PromotionFact;
    public sealed record RejectedByHR(DateTimeOffset RejectedAt) : PromotionFact;
    public sealed record ApprovedByCEO(DateTimeOffset ApprovedAt) : PromotionFact;
    public sealed record RejectedByCEO(DateTimeOffset RejectedAt) : PromotionFact;
    public sealed record PromotionClosedWithRejection(Guid PromotionId) : PromotionFact;
    public sealed record PromotionClosedWithAcceptance(Guid PromotionId) : PromotionFact;
}