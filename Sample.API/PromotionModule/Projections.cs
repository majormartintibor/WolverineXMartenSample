using Marten;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using static Sample.API.PromotionModule.PromotionFact;

namespace Sample.API.PromotionModule;

/// <summary>
/// Serves to show the Promotee the status of their Promotion request
/// </summary>
public sealed record PromotionStatus(
    Guid Id,
    string Promotee,
    Status Status)
{
    public PromotionStatus()
        : this(Guid.Empty, string.Empty, Status.Pending)
    {
    }

    public PromotionStatus Apply(PromotionFact @fact) =>
        @fact switch
        {
            PromotionOpened(Guid promotionId, string promotee) =>
                this with { Id = promotionId, Promotee = promotee },

            PromotionClosedWithRejection =>
                this with { Status = Status.ClosedAndRejected},

            PromotionClosedWithAcceptance =>
                this with { Status = Status.ClosedAndApproved},

            _ => this
        };
}
public enum Status
{
    Pending,
    ClosedAndRejected,
    ClosedAndApproved
}

//This pattern to call the Apply method of the Detail is useful
//as aggregate Stream gives option to pass either timestamp or version
//to do time travel. Aggregate stream will call the apply method of
//PromotionStatus while the .Query calls will call the Apply method
//of PromotionStatusProjection
public sealed class PromotionStatusProjection
    : SingleStreamProjection<PromotionStatus>
{
    public PromotionStatus Apply(PromotionFact fact, PromotionStatus current)
        => current.Apply(fact);
}


/// <summary>
/// This would require some elevated rights to see, not meant for the Promotee
/// </summary>
public sealed record PromotionDetails(
    Guid Id,
    string Promotee,
    DateTimeOffset? RejectedAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? ApprovedBySupervisor,
    DateTimeOffset? ApprovedByHR,
    DateTimeOffset? ApprovedByCEO,
    bool Closed)
{
    public PromotionDetails()
        : this(Guid.Empty, string.Empty,
              null, null, null, null, null, false)
    {
    }

    public PromotionDetails Apply(PromotionFact @fact) =>
        fact switch
        {
            PromotionOpened(Guid promotionId, string promotee) =>
                this with { Id = promotionId, Promotee = promotee },

            ApprovedBySupervisor(DateTimeOffset approvedAt) =>
                this with { ApprovedBySupervisor = approvedAt },

            ApprovedByHR(DateTimeOffset approvedAt) =>
                this with { ApprovedByHR = approvedAt },

            ApprovedByCEO(DateTimeOffset approvedAt) =>
                this with { ApprovedByCEO = approvedAt, AcceptedAt = approvedAt },

            RejectedBySupervisor(DateTimeOffset rejectedAt) =>
                this with { RejectedAt = rejectedAt },

            RejectedByHR(DateTimeOffset rejectedAt) =>
                this with { RejectedAt = rejectedAt},

            RejectedByCEO(DateTimeOffset rejectedAt) =>
                this with { RejectedAt = rejectedAt },

            PromotionClosedWithRejection =>
                this with { Closed = true },

            PromotionClosedWithAcceptance => 
                this with { Closed = true },

            _ => this
        };
}

public sealed class PromotionDetailsProjection
    : SingleStreamProjection<PromotionDetails>
{
    public PromotionDetails Apply(PromotionFact fact, PromotionDetails current)
        => current.Apply(fact);
}


public static class PromotionStoreOptionsExtensions
{
    public static void AddPromotionProjections(this StoreOptions options)
    {
        options.Projections.LiveStreamAggregation<Promotion>();
        options.Projections.Add<PromotionStatusProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<PromotionDetailsProjection>(ProjectionLifecycle.Inline);
    }
}