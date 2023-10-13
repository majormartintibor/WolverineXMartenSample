namespace Sample.API;

public sealed record PromotionRequested(Guid PromotionId, string Promotee);
public sealed record ApprovedBySupervisor(DateTimeOffset ApprovedAt);
public sealed record RejectedBySupervisor(DateTimeOffset? RejectedAt);
public sealed record ApprovedByHR(DateTimeOffset ApprovedAt);
public sealed record RejectedByHR(DateTimeOffset RejectedAt);
public sealed record ApprovedByCEO(DateTimeOffset ApprovedAt);
public sealed record RejectedByCEO(DateTimeOffset RejectedAt);
public sealed record PromotionClosedWithRejection();
public sealed record PromotionClosedWithAcceptance();