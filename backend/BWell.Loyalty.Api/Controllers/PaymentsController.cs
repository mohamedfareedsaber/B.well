using BWell.Loyalty.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BWell.Loyalty.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptions;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(ISubscriptionService subscriptions, ILogger<PaymentsController> logger)
    {
        _subscriptions = subscriptions;
        _logger = logger;
    }

    /// <summary>
    /// Paymob notification webhook.
    /// Configure as notification_url; HMAC arrives as ?hmac=...
    /// </summary>
    [HttpPost("paymob/webhook")]
    public async Task<IActionResult> PaymobWebhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(ct);
        var hmac = Request.Query["hmac"].ToString();

        try
        {
            await _subscriptions.HandlePaymobWebhookAsync(json, hmac, ct);
            return Ok(new { received = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Rejected Paymob webhook");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Paymob webhook processing failed");
            return BadRequest(new { error = ex.Message });
        }
    }
}
