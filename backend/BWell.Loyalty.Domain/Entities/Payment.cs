using BWell.Loyalty.Domain.Common;
using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;

    public decimal AmountEgp { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.CreditCard;

    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    public string? FailureReason { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}
