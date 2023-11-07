using static Sample.API.PromotionModule.PromotionFact;

namespace Sample.API.PromotionModule;

public record Promotion
{
    private Promotion() { } // Not to allow inheritance    
    
    public sealed record OpenPromotion : Promotion; //could be WaitingForSupervisor
    public sealed record PassedSupervisorApproval : Promotion; //could be WaitingForHR
    public sealed record PassedHRApproval : Promotion; //could be WaitingForCEO
    public sealed record PassedCEOApproval : Promotion; //could be WaitingForClosure    
    public sealed record ApprovedPromotion : Promotion;
    public sealed record RejectedPromotion : Promotion;
    
    public Guid Id { get; set; }
    public int Version { get; set; }
    public string Promotee { get; set; } = string.Empty;
    public bool Closed { get; set; }

    //Code smell primitive obsession, not scope for now
    public DateTimeOffset? RejectedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? ApprovedBySupervisor { get; set; }
    public DateTimeOffset? ApprovedByHR { get; set; }
    public DateTimeOffset? ApprovedByCEO { get; set; }

    public static Promotion GetEmptyPromotion() => new();


    /// <summary>
    /// By Marten convention this method needs to be named Apply.
    /// Restores state by applying the fact.
    /// Works as the documentation for state transition!
    /// </summary>
    /// <param name="fact">The fact that needs to be applied in order to restore next state</param>
    /// <returns>next state of the aggregate</returns>
    public Promotion Apply(PromotionFact @fact) =>
        @fact switch
        {
            PromotionOpened(Guid promotionId, string promotee) =>
                new OpenPromotion { Id = promotionId, Promotee = promotee },

            ApprovedBySupervisor(DateTimeOffset approvedAt) =>
                this is OpenPromotion openPromotion
                    ? new PassedSupervisorApproval
                    { 
                        Id = openPromotion.Id,
                        Version = openPromotion.Version,
                        Promotee = openPromotion.Promotee,                                                
                        ApprovedBySupervisor = approvedAt,
                    }
                    : this,

            ApprovedByHR(DateTimeOffset approvedAt) =>
                this is PassedSupervisorApproval passedSupervisorApproval
                    ? new PassedHRApproval
                    {
                        Id = passedSupervisorApproval.Id,
                        Version = passedSupervisorApproval.Version,
                        Promotee = passedSupervisorApproval.Promotee,
                        ApprovedBySupervisor = passedSupervisorApproval.ApprovedBySupervisor,
                        ApprovedByHR = approvedAt,
                    }
                    : this,

            ApprovedByCEO(DateTimeOffset approvedAt) =>
                this is PassedHRApproval passedHRApproval
                    ? new PassedCEOApproval
                    {
                        Id = passedHRApproval.Id,
                        Version = passedHRApproval.Version,
                        Promotee = passedHRApproval.Promotee,
                        ApprovedBySupervisor = passedHRApproval.ApprovedBySupervisor,
                        ApprovedByHR = passedHRApproval.ApprovedByHR,
                        ApprovedByCEO = approvedAt,
                        AcceptedAt = approvedAt,
                    }
                    : this,

            RejectedBySupervisor(DateTimeOffset rejectedAt) =>
                this is OpenPromotion openedPromotion
                    ? new RejectedPromotion
                    {
                        Id = openedPromotion.Id,
                        Version = openedPromotion.Version,
                        Promotee = openedPromotion.Promotee,
                        RejectedAt = rejectedAt,
                    }
                    : this,

            RejectedByHR(DateTimeOffset rejectedAt) =>
                this is PassedSupervisorApproval passedSupervisorApproval
                    ? new RejectedPromotion
                    {
                        Id = passedSupervisorApproval.Id,
                        Version = passedSupervisorApproval.Version,
                        Promotee = passedSupervisorApproval.Promotee,
                        ApprovedBySupervisor = passedSupervisorApproval.ApprovedBySupervisor,
                        RejectedAt = rejectedAt 
                    }
                    : this,

            RejectedByCEO(DateTimeOffset rejectedAt) =>
                this is PassedHRApproval passedHRApproval
                    ? new RejectedPromotion
                    {
                        Id = passedHRApproval.Id,
                        Version = passedHRApproval.Version,
                        Promotee = passedHRApproval.Promotee,
                        ApprovedBySupervisor = passedHRApproval.ApprovedBySupervisor,
                        ApprovedByHR = passedHRApproval.ApprovedByHR,
                        RejectedAt = rejectedAt 
                    }
                    : this,

            PromotionClosedWithAcceptance =>
                this is PassedCEOApproval passedCEOApproval
                    ? new ApprovedPromotion
                    {   Id = passedCEOApproval.Id,
                        Version = passedCEOApproval.Version,
                        Promotee = passedCEOApproval.Promotee,
                        ApprovedBySupervisor = passedCEOApproval.ApprovedBySupervisor,
                        ApprovedByHR = passedCEOApproval.ApprovedByHR,
                        ApprovedByCEO = passedCEOApproval.ApprovedByCEO,
                        AcceptedAt = passedCEOApproval.AcceptedAt,
                        Closed = true 
                    }
                    : this,

            PromotionClosedWithRejection =>
                this is RejectedPromotion rejectedPromotion
                    ? rejectedPromotion with { Closed = true }
                    : this,

            _ => this
        };
}