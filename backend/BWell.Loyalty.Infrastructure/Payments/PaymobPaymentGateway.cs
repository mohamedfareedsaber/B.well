using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BWell.Loyalty.Infrastructure.Payments;

public class PaymobPaymentGateway : IPaymobPaymentGateway
{
    private readonly PaymobOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PaymobPaymentGateway> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public PaymobPaymentGateway(
        IOptions<PaymobOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<PaymobPaymentGateway> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.SecretKey) &&
        !string.IsNullOrWhiteSpace(_options.PublicKey);

    public bool IsMockMode => _options.UseMockPayments || !IsConfigured;
    public string PublicKey => _options.PublicKey;
    public string Currency => _options.Currency;

    public async Task<PaymobIntentionResult> CreateIntentionAsync(PaymobIntentionRequest request, CancellationToken ct = default)
    {
        if (IsMockMode)
        {
            var id = $"int_mock_{Guid.NewGuid():N}";
            var secret = $"egy_csk_mock_{Guid.NewGuid():N}";
            return new PaymobIntentionResult(id, secret, null, BuildCheckoutUrl(secret));
        }

        var amountCents = ToCents(request.AmountEgp);
        var (firstName, lastName) = SplitName(request.FullName);

        var paymentMethods = ResolvePaymentMethods(request.PaymentMethod);

        if (paymentMethods.Count == 0)
        {
            throw new InvalidOperationException(
                $"Paymob has no integration configured for {request.PaymentMethod}. " +
                "Add it under Dashboard → Settings → Payment Integrations, then set CardIntegrationId / WalletIntegrationId / BankTransferIntegrationId.");
        }

        var payload = new Dictionary<string, object?>
        {
            ["amount"] = amountCents,
            ["currency"] = _options.Currency,
            ["payment_methods"] = paymentMethods,
            ["items"] = new[]
            {
                new Dictionary<string, object?>
                {
                    ["name"] = request.Description,
                    ["amount"] = amountCents,
                    ["description"] = request.Description,
                    ["quantity"] = 1
                }
            },
            ["billing_data"] = new Dictionary<string, object?>
            {
                ["first_name"] = firstName,
                ["last_name"] = lastName,
                ["email"] = request.Email,
                ["phone_number"] = NormalizePhone(request.PhoneNumber),
                ["country"] = "EGY",
                ["state"] = string.IsNullOrWhiteSpace(request.Governorate) ? "Cairo" : request.Governorate,
                ["city"] = string.IsNullOrWhiteSpace(request.Governorate) ? "Cairo" : request.Governorate,
                ["street"] = string.IsNullOrWhiteSpace(request.AddressDetails) ? "NA" : request.AddressDetails,
                ["building"] = "NA",
                ["floor"] = "NA",
                ["apartment"] = "NA",
                ["postal_code"] = string.IsNullOrWhiteSpace(request.PostalCode) ? "NA" : request.PostalCode
            },
            ["customer"] = new Dictionary<string, object?>
            {
                ["first_name"] = firstName,
                ["last_name"] = lastName,
                ["email"] = request.Email
            },
            ["special_reference"] = request.SpecialReference,
            ["extras"] = new Dictionary<string, object?>
            {
                ["subscription_id"] = request.SpecialReference
            },
            ["notification_url"] = $"{_options.NotificationBaseUrl.TrimEnd('/')}/api/payments/paymob/webhook",
            ["redirection_url"] = _options.RedirectionUrl
        };

        var client = _httpClientFactory.CreateClient("Paymob");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/intention/")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Token", _options.SecretKey);

        using var response = await client.SendAsync(httpRequest, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Paymob intention failed ({Status}): {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException($"Paymob intention failed: {ExtractError(body)}");
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var intentionId = root.TryGetProperty("id", out var idEl) ? idEl.ToString() : string.Empty;
        var clientSecret = root.GetProperty("client_secret").GetString()
            ?? throw new InvalidOperationException("Paymob response missing client_secret.");
        long? orderId = null;
        if (root.TryGetProperty("intention_order_id", out var orderEl) && orderEl.ValueKind == JsonValueKind.Number)
            orderId = orderEl.GetInt64();

        _logger.LogInformation("Created Paymob intention {IntentionId} order {OrderId}", intentionId, orderId);
        return new PaymobIntentionResult(intentionId, clientSecret, orderId, BuildCheckoutUrl(clientSecret));
    }

    public async Task<(bool Succeeded, string Status, string? FailureMessage)> GetIntentionStatusAsync(
        string clientSecret,
        CancellationToken ct = default)
    {
        if (IsMockMode)
        {
            if (clientSecret.Contains("mock", StringComparison.OrdinalIgnoreCase))
                return (true, "succeeded", null);
            return (false, "failed", "Unknown mock intention.");
        }

        var client = _httpClientFactory.CreateClient("Paymob");
        var path = $"/v1/intention/element/{Uri.EscapeDataString(_options.PublicKey)}/{Uri.EscapeDataString(clientSecret)}/";
        using var response = await client.GetAsync(path, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return (false, "unknown", ExtractError(body));

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.TryGetProperty("status", out var statusEl)
            ? statusEl.GetString() ?? "unknown"
            : "unknown";

        var confirmed = doc.RootElement.TryGetProperty("confirmed", out var confEl) && confEl.ValueKind == JsonValueKind.True;
        var succeeded = confirmed || status.Equals("paid", StringComparison.OrdinalIgnoreCase)
            || status.Equals("successful", StringComparison.OrdinalIgnoreCase)
            || status.Equals("success", StringComparison.OrdinalIgnoreCase);

        return (succeeded, status, succeeded ? null : $"Paymob status: {status}");
    }

    public bool TryValidateWebhook(
        string json,
        string? hmacQuery,
        out string? specialReference,
        out string? transactionId,
        out bool succeeded,
        out string? failureMessage)
    {
        specialReference = null;
        transactionId = null;
        succeeded = false;
        failureMessage = null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Paymob often wraps payload as { "obj": { ... }, "type": "TRANSACTION" }
            var obj = root.TryGetProperty("obj", out var objEl) ? objEl : root;

            if (!string.IsNullOrWhiteSpace(_options.HmacSecret))
            {
                if (string.IsNullOrWhiteSpace(hmacQuery))
                {
                    _logger.LogWarning("Paymob webhook missing hmac query.");
                    return false;
                }

                var calculated = CalculateTransactionHmac(obj, _options.HmacSecret);
                if (!CryptographicOperations.FixedTimeEquals(
                        Encoding.UTF8.GetBytes(calculated.ToLowerInvariant()),
                        Encoding.UTF8.GetBytes(hmacQuery.Trim().ToLowerInvariant())))
                {
                    _logger.LogWarning("Paymob webhook HMAC mismatch.");
                    return false;
                }
            }
            else if (!IsMockMode)
            {
                _logger.LogWarning("Paymob:HmacSecret is empty — webhook accepted without HMAC (configure ASAP).");
            }

            succeeded = obj.TryGetProperty("success", out var successEl) &&
                        (successEl.ValueKind == JsonValueKind.True ||
                         string.Equals(successEl.ToString(), "true", StringComparison.OrdinalIgnoreCase));

            if (obj.TryGetProperty("id", out var idEl))
                transactionId = idEl.ToString();

            if (obj.TryGetProperty("order", out var orderEl))
            {
                if (orderEl.TryGetProperty("merchant_order_id", out var merchantEl))
                    specialReference = merchantEl.GetString();
                else if (orderEl.TryGetProperty("merchant_order_id", out _))
                    specialReference = orderEl.GetProperty("merchant_order_id").ToString();
            }

            if (string.IsNullOrWhiteSpace(specialReference) &&
                obj.TryGetProperty("merchant_order_id", out var directMerchant))
            {
                specialReference = directMerchant.GetString();
            }

            // Intention extras / special_reference fallback
            if (string.IsNullOrWhiteSpace(specialReference) &&
                obj.TryGetProperty("payment_key_claims", out var claims) &&
                claims.TryGetProperty("extra", out var extra) &&
                extra.TryGetProperty("subscription_id", out var subId))
            {
                specialReference = subId.GetString();
            }

            if (!succeeded)
                failureMessage = obj.TryGetProperty("data", out var dataEl) && dataEl.TryGetProperty("message", out var msgEl)
                    ? msgEl.GetString()
                    : "Payment failed";

            return !string.IsNullOrWhiteSpace(specialReference) || !string.IsNullOrWhiteSpace(transactionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse Paymob webhook");
            return false;
        }
    }

    public string BuildCheckoutUrl(string clientSecret) =>
        $"{_options.BaseUrl.TrimEnd('/')}/unifiedcheckout/?publicKey={Uri.EscapeDataString(_options.PublicKey)}&clientSecret={Uri.EscapeDataString(clientSecret)}";

    private static int ToCents(decimal amountEgp) =>
        (int)Math.Round(amountEgp * 100m, MidpointRounding.AwayFromZero);

    private static (string First, string Last) SplitName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1 ? (parts[0], "Customer") : (parts[0], parts[1]);
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("20") && digits.Length >= 12) return "+" + digits;
        if (digits.StartsWith("0") && digits.Length >= 10) return "+20" + digits[1..];
        if (digits.Length == 10) return "+20" + digits;
        return phone.StartsWith('+') ? phone : "+20" + digits;
    }

    private static string ExtractError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("detail", out var detail))
                return detail.ToString();
            if (doc.RootElement.TryGetProperty("message", out var message))
                return message.ToString();
            return body.Length > 400 ? body[..400] : body;
        }
        catch
        {
            return body.Length > 400 ? body[..400] : body;
        }
    }

    /// <summary>
    /// Paymob TRANSACTION hmac concatenation (common Egypt Accept format).
    /// </summary>
    private static string CalculateTransactionHmac(JsonElement obj, string hmacSecret)
    {
        string S(params string[] path)
        {
            JsonElement current = obj;
            foreach (var p in path)
            {
                if (!current.TryGetProperty(p, out current))
                    return string.Empty;
            }

            return current.ValueKind switch
            {
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Number => current.GetRawText(),
                JsonValueKind.Null => string.Empty,
                _ => current.ToString()
            };
        }

        var concat =
            S("amount_cents") +
            S("created_at") +
            S("currency") +
            S("error_occured") +
            S("has_parent_transaction") +
            S("id") +
            S("integration_id") +
            S("is_3d_secure") +
            S("is_auth") +
            S("is_capture") +
            S("is_refunded") +
            S("is_standalone_payment") +
            S("is_voided") +
            S("order", "id") +
            S("owner") +
            S("pending") +
            S("source_data", "pan") +
            S("source_data", "sub_type") +
            S("source_data", "type") +
            S("success");

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hmacSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(concat));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private List<object> ResolvePaymentMethods(Domain.Enums.PaymentMethodType method)
    {
        int? preferred = method switch
        {
            Domain.Enums.PaymentMethodType.CreditCard =>
                _options.CardIntegrationId ?? (_options.IntegrationIds is { Length: > 0 } ? _options.IntegrationIds[0] : null),
            Domain.Enums.PaymentMethodType.DigitalWallet => _options.WalletIntegrationId,
            Domain.Enums.PaymentMethodType.BankTransfer => _options.BankTransferIntegrationId,
            _ => null
        };

        if (preferred is > 0)
            return [preferred.Value];

        // Fallback: only CreditCard may use shared IntegrationIds / named methods.
        if (method == Domain.Enums.PaymentMethodType.CreditCard)
        {
            if (_options.IntegrationIds is { Length: > 0 })
                return _options.IntegrationIds.Cast<object>().ToList();
            if (_options.PaymentMethodNames is { Length: > 0 })
                return _options.PaymentMethodNames.Cast<object>().ToList();
        }

        return [];
    }

    public IReadOnlyList<(Domain.Enums.PaymentMethodType Method, bool Available, string Label)> GetAvailableMethods()
    {
        return
        [
            (Domain.Enums.PaymentMethodType.CreditCard,
                ResolvePaymentMethods(Domain.Enums.PaymentMethodType.CreditCard).Count > 0 || IsMockMode,
                "Credit Card"),
            (Domain.Enums.PaymentMethodType.DigitalWallet,
                ResolvePaymentMethods(Domain.Enums.PaymentMethodType.DigitalWallet).Count > 0 || IsMockMode,
                "Digital Wallet"),
            (Domain.Enums.PaymentMethodType.BankTransfer,
                ResolvePaymentMethods(Domain.Enums.PaymentMethodType.BankTransfer).Count > 0 || IsMockMode,
                "Bank Transfer"),
        ];
    }
}
