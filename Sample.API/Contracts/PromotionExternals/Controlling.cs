namespace Sample.API.Contracts.PromotionExternals.Controlling;

public sealed record DoControllingStuffWhenPromotionAccepted(
    string Promotee, 
    DateTimeOffset AcceptedBySupervisorDate,
    DateTimeOffset AcceptedByHRDate,
    DateTimeOffset AcceptedByCEODate);

public sealed record DoControllingStuffWhenPromotionRejected(
    string Promotee,
    DateTimeOffset RejectionDate);