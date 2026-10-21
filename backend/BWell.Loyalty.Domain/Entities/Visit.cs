using BWell.Loyalty.Domain.Common;

namespace BWell.Loyalty.Domain.Entities;

public class Visit : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid? MedicalServiceId { get; set; }
    public MedicalService? MedicalService { get; set; }

    public Guid? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }

    /// <summary>Null = primary patient visit; set when visit is for a family member.</summary>
    public Guid? FamilyMemberId { get; set; }
    public FamilyMember? FamilyMember { get; set; }

    public DateTime VisitDateUtc { get; set; } = DateTime.UtcNow;
    public int VisitSequenceForService { get; set; } = 1;
    public decimal? DiscountAppliedPercent { get; set; }
    public string? Notes { get; set; }
}
