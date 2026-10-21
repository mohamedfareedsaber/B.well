using BWell.Loyalty.Domain.Common;
using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Domain.Entities;

public class Subscription : BaseEntity
{
    public string SubscriptionNumber { get; set; } = string.Empty;

    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid LoyaltyProgramId { get; set; }
    public LoyaltyProgram LoyaltyProgram { get; set; } = null!;

    public PlanType PlanType { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Annual;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.PendingPayment;

    public decimal AmountEgp { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? NextRenewalDateUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }

    public string? StripeSubscriptionId { get; set; }
    public string? StripePaymentIntentId { get; set; }

    public DeliveryAddress? DeliveryAddress { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
