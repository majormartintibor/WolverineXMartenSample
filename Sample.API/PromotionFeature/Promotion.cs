using static Sample.API.PromotionFeature.PromotionFact;

namespace Sample.API.PromotionFeature;

public record Promotion
{
    private Promotion() { }

    public record OpenedPromotion : Promotion;
    public record PassedSupervisorApproval : OpenedPromotion;
    public record PassedHRApproval : PassedSupervisorApproval;
    public record PassedCEOApproval : PassedHRApproval;
    public sealed record ApprovedPromotion : PassedCEOApproval;
    public sealed record RejectedPromotion : OpenedPromotion;


    public Guid Id { get; set; }
    public int Version { get; set; }
    public string Promotee { get; set; } = string.Empty;
    public bool Closed { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
    public DateTimeOffset? ApprovedBySupervisor { get; set; }
    public DateTimeOffset? ApprovedByHR { get; set; }
    public DateTimeOffset? ApprovedByCEO { get; set; }

    public Promotion Apply(PromotionFact @fact) =>
        @fact switch
        {
            PromotionRequested(Guid promotionId, string promotee) =>
                new OpenedPromotion { Id = promotionId, Promotee = promotee },

            ApprovedBySupervisor(DateTimeOffset approvedAt) =>
                this is OpenedPromotion openedPromotion
                    ? (PassedSupervisorApproval)openedPromotion with
                    { ApprovedBySupervisor = approvedAt }
                    : this,

            ApprovedByHR(DateTimeOffset approvedAt) =>
                this is PassedSupervisorApproval passedSupervisorApproval
                    ? (PassedHRApproval)passedSupervisorApproval with
                    { ApprovedByHR = approvedAt }
                    : this,

            ApprovedByCEO(DateTimeOffset approvedAt) =>
                this is PassedHRApproval passedHRApproval
                    ? (PassedCEOApproval)passedHRApproval with
                    { ApprovedByCEO = approvedAt }
                    : this,

            RejectedBySupervisor(DateTimeOffset rejectedAt) =>
                this is OpenedPromotion openedPromotion
                    ? (RejectedPromotion)openedPromotion with
                    { RejectedAt = rejectedAt }
                    : this,

            RejectedByHR(DateTimeOffset rejectedAt) =>
                this is PassedSupervisorApproval passedSupervisorApproval
                    ? (RejectedPromotion)(OpenedPromotion)passedSupervisorApproval with
                    { RejectedAt = rejectedAt }
                    : this,

            RejectedByCEO(DateTimeOffset rejectedAt) =>
                this is PassedHRApproval passedHRApproval
                    ? (RejectedPromotion)(OpenedPromotion)passedHRApproval with
                    { RejectedAt = rejectedAt }
                    : this,

            PromotionClosedWithAcceptance =>
                this is ApprovedPromotion approvedPromotion
                    ? approvedPromotion with { Closed = true }
                    : this,

            PromotionClosedWithRejection =>
                this is RejectedPromotion rejectedPromotion
                    ? rejectedPromotion with { Closed = true }
                    : this,

            _ => this
        };
}