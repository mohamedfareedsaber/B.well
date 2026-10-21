using BWell.Loyalty.Application.Common;
using BWell.Loyalty.Application.DTOs.Subscriptions;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BWell.Loyalty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(DependencyInjection.PaymentRateLimit)]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptions;

    public SubscriptionsController(ISubscriptionService subscriptions) => _subscriptions = subscriptions;

    [HttpGet("payment-config")]
    public ActionResult<ApiResponse<PaymentConfigDto>> PaymentConfig()
        => Ok(ApiResponse<PaymentConfigDto>.Ok(_subscriptions.GetPaymentConfig()));

    [HttpPost("checkout/start")]
    public async Task<ActionResult<ApiResponse<CheckoutSessionDto>>> StartCheckout([FromBody] StartCheckoutRequest request, CancellationToken ct)
    {
        var result = await _subscriptions.StartCheckoutAsync(request, ct);
        return Ok(ApiResponse<CheckoutSessionDto>.Ok(result));
    }

    [HttpPost("checkout/complete-payment")]
    public async Task<ActionResult<ApiResponse<CheckoutSessionDto>>> CompletePayment([FromBody] CompletePaymentRequest request, CancellationToken ct)
    {
        var result = await _subscriptions.CompletePaymentAsync(request, ct);
        return Ok(ApiResponse<CheckoutSessionDto>.Ok(result));
    }

    /// <summary>
    /// After Paymob Unified Checkout redirect, optionally confirm status.
    /// Authoritative activation still comes from the Paymob webhook.
    /// </summary>
    [HttpPost("checkout/confirm-payment")]
    public async Task<ActionResult<ApiResponse<OrderSummaryDto>>> ConfirmPayment([FromBody] ConfirmPaymentRequest request, CancellationToken ct)
    {
        var result = await _subscriptions.ConfirmPaymentAsync(request, ct);
        return Ok(ApiResponse<OrderSummaryDto>.Ok(result));
    }

    [HttpPost("checkout/mark-failed")]
    public async Task<ActionResult<ApiResponse<OrderSummaryDto>>> MarkFailed([FromBody] MarkPaymentFailedRequest request, CancellationToken ct)
    {
        var result = await _subscriptions.MarkPaymentFailedAsync(request, ct);
        return Ok(ApiResponse<OrderSummaryDto>.Ok(result));
    }

    [HttpGet("{id:guid}/order-summary")]
    public async Task<ActionResult<ApiResponse<OrderSummaryDto>>> GetOrderSummary(Guid id, CancellationToken ct)
    {
        var summary = await _subscriptions.GetOrderSummaryAsync(id, ct);
        if (summary is null)
            return NotFound(ApiResponse<OrderSummaryDto>.Fail("Subscription not found."));
        return Ok(ApiResponse<OrderSummaryDto>.Ok(summary));
    }
}
