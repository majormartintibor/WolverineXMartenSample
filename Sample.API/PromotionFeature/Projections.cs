using Marten;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using static Sample.API.PromotionFeature.PromotionFact;

namespace Sample.API.PromotionFeature;

public sealed record PromotionStatus(
    Guid Id,
    string Promotee,
    Status Status)
{
    public PromotionStatus()
        : this(Guid.Empty, string.Empty, Status.Pending)
    {
    }

    public PromotionStatus Apply(PromotionRequested requested)
        => this with { Promotee = requested.Promotee, Id = requested.PromotionId };

    public PromotionStatus Apply(PromotionClosedWithRejection rejected)
        => this with { Status = Status.ClosedAndRejected };

    public PromotionStatus Apply(PromotionClosedWithAcceptance accepted)
        => this with { Status = Status.ClosedAndApproved };
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
    public PromotionStatus Apply(PromotionRequested requested, PromotionStatus current)
        => current.Apply(requested);

    public PromotionStatus Apply(PromotionClosedWithRejection rejected, PromotionStatus current)
        => current.Apply(rejected);

    public PromotionStatus Apply(PromotionClosedWithAcceptance accepted, PromotionStatus current)
        => current.Apply(accepted);
}


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

    public PromotionDetails Apply(PromotionRequested requested)
        => this with
        {
            Id = requested.PromotionId,
            Promotee = requested.Promotee,
            RejectedAt = null,
            AcceptedAt = null,
            ApprovedBySupervisor = null,
            ApprovedByHR = null,
            ApprovedByCEO = null
        };

    public PromotionDetails Apply(RejectedBySupervisor rejected)
        => this with { RejectedAt = rejected.RejectedAt };
    public PromotionDetails Apply(ApprovedBySupervisor approved)
        => this with { ApprovedBySupervisor = approved.ApprovedAt };

    public PromotionDetails Apply(RejectedByHR rejected)
        => this with { RejectedAt = rejected.RejectedAt };

    public PromotionDetails Apply(ApprovedByHR approved)
        => this with { ApprovedByHR = approved.ApprovedAt };

    public PromotionDetails Apply(RejectedByCEO rejected)
        => this with { RejectedAt = rejected.RejectedAt };

    public PromotionDetails Apply(ApprovedByCEO approved)
        => this with { ApprovedByCEO = approved.ApprovedAt };

    public PromotionDetails Apply(PromotionClosedWithRejection rejected)
        => this with { Closed = true };

    public PromotionDetails Apply(PromotionClosedWithAcceptance approved)
        => this with { Closed = true };
}
public sealed class PromotionDetailsProjection
    : SingleStreamProjection<PromotionDetails>
{
    public PromotionDetails Apply(PromotionRequested requested, PromotionDetails current)
        => current.Apply(requested);

    public PromotionDetails Apply(RejectedBySupervisor rejected, PromotionDetails current)
        => current.Apply(rejected);

    public PromotionDetails Apply(ApprovedBySupervisor approved, PromotionDetails current)
        => current.Apply(approved);

    public PromotionDetails Apply(RejectedByHR rejected, PromotionDetails current)
        => current.Apply(rejected);

    public PromotionDetails Apply(ApprovedByHR approved, PromotionDetails current)
        => current.Apply(approved);

    public PromotionDetails Apply(RejectedByCEO rejected, PromotionDetails current)
        => current.Apply(rejected);

    public PromotionDetails Apply(ApprovedByCEO approved, PromotionDetails current)
        => current.Apply(approved);
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