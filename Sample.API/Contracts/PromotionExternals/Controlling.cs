namespace Sample.API.Contracts.PromotionExternals.Controlling;

public sealed record PromotionAccepted(
    string Promotee, 
    DateTimeOffset AcceptedBySupervisorDate,
    DateTimeOffset AcceptedByHRDate,
    DateTimeOffset AcceptedByCEODate);

public sealed record PromotionRejected(
    string Promotee,
    DateTimeOffset RejectionDate);