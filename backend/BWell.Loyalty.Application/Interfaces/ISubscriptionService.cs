using BWell.Loyalty.Application.DTOs.Subscriptions;

namespace BWell.Loyalty.Application.Interfaces;

public interface ISubscriptionService
{
    Task<CheckoutSessionDto> StartCheckoutAsync(StartCheckoutRequest request, CancellationToken ct = default);
    Task<CheckoutSessionDto> CompletePaymentAsync(CompletePaymentRequest request, CancellationToken ct = default);
    Task<OrderSummaryDto> ConfirmPaymentAsync(ConfirmPaymentRequest request, CancellationToken ct = default);
    Task<OrderSummaryDto> MarkPaymentFailedAsync(MarkPaymentFailedRequest request, CancellationToken ct = default);
    Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid subscriptionId, CancellationToken ct = default);
    Task HandlePaymobWebhookAsync(string json, string? hmacQuery, CancellationToken ct = default);
    PaymentConfigDto GetPaymentConfig();
}
