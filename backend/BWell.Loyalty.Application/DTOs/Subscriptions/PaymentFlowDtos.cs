namespace BWell.Loyalty.Application.DTOs.Subscriptions;

public record ConfirmPaymentRequest(Guid SubscriptionId, string? FailureReason = null);

public record MarkPaymentFailedRequest(Guid SubscriptionId, string? FailureReason = null);

public record PaymentConfigDto(
    string Provider,
    string PublicKey,
    bool IsConfigured,
    bool UseMockPayments,
    string Currency,
    string CheckoutBaseHint,
    IReadOnlyList<PaymentMethodOptionDto> Methods);

public record PaymentMethodOptionDto(
    string Method,
    string Label,
    bool Available,
    string? UnavailableReason);
