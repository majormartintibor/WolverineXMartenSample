namespace Sample.API;

public record Promotion
{ 
    public Guid Id { get; set; }

    public int Version { get; set; }

    public string Promotee { get; set; } = string.Empty;

    public DateTimeOffset? RejectedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }

    public DateTimeOffset? ApprovedBySupervisor { get; set; }
    public DateTimeOffset? ApprovedByHR { get; set; }
    public DateTimeOffset? ApprovedByCEO { get; set; }

    public bool Closed { get; set; }

    public bool ApprovedByAll =>
        ApprovedBySupervisor != null &&
        ApprovedByHR != null &&
        ApprovedByCEO != null;

    public void Apply(PromotionRequested requested)
    {
        Id = requested.PromotionId;
        Promotee = requested.Promotee;
        ApprovedBySupervisor = null;
        ApprovedByHR = null;
        ApprovedByCEO = null;
        RejectedAt = null;
        AcceptedAt = null;
    }

    public void Apply(ApprovedBySupervisor approved)
    {
        ApprovedBySupervisor = approved.ApprovedAt;
    }

    public void Apply(RejectedBySupervisor rejected)
    {
        RejectedAt = rejected.RejectedAt;
    }

    public void Apply(ApprovedByHR approved)
    {
        ApprovedByHR = approved.ApprovedAt;
    }

    public void Apply(RejectedByHR rejected)
    {
        RejectedAt = rejected.RejectedAt;
    }

    public void Apply(ApprovedByCEO approved)
    {
        ApprovedByCEO = approved.ApprovedAt;
    }

    public void Apply(RejectedByCEO rejected)
    {
        RejectedAt = rejected.RejectedAt;
    }

    public void Apply(PromotionClosedWithRejection closed)
    {
        Closed = true;
    }

    public void Apply(PromotionClosedWithAcceptance closed)
    {
        Closed = true;
    }
}