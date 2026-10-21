using BWell.Loyalty.Domain.Common;
using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Domain.Entities;

public class BenefitRedemption : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;

    public Guid? FamilyMemberId { get; set; }
    public FamilyMember? FamilyMember { get; set; }

    public string BenefitName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public BenefitRedemptionStatus Status { get; set; } = BenefitRedemptionStatus.Pending;
}
