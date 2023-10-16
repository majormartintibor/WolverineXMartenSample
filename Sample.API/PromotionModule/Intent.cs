using System.ComponentModel.DataAnnotations;

namespace Sample.API.PromotionModule;

public sealed record RequestPromotion([Required] string Promotee);

public sealed record SupervisorResponds(
    Guid PromotionId,
    int Version,
    DateTimeOffset DecisionMadeAt,
    bool Verdict);

public sealed record HRResponds(
    Guid PromotionId,
    int Version,
    DateTimeOffset DecisionMadeAt,
    bool Verdict);

public sealed record CEOResponds(
    Guid PromotionId,
    int Version,
    DateTimeOffset DecisionMadeAt,
    bool Verdict);

public sealed record RequestPromotionStatus(Guid PromotionId);

public sealed record RequestPromotionDetails(Guid PromotionId);

public sealed record RequestPromotionDetailsWithVersion(Guid PromotionId, int Version);