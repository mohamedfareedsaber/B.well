namespace BWell.Loyalty.Application.Interfaces;

public interface IPaymobPaymentGateway
{
    bool IsConfigured { get; }
    bool IsMockMode { get; }
    string PublicKey { get; }
    string Currency { get; }

    Task<PaymobIntentionResult> CreateIntentionAsync(PaymobIntentionRequest request, CancellationToken ct = default);

    Task<(bool Succeeded, string Status, string? FailureMessage)> GetIntentionStatusAsync(
        string clientSecret,
        CancellationToken ct = default);

    bool TryValidateWebhook(
        string json,
        string? hmacQuery,
        out string? specialReference,
        out string? transactionId,
        out bool succeeded,
        out string? failureMessage);

    string BuildCheckoutUrl(string clientSecret);

    IReadOnlyList<(Domain.Enums.PaymentMethodType Method, bool Available, string Label)> GetAvailableMethods();
}

public record PaymobIntentionRequest(
    decimal AmountEgp,
    string SpecialReference,
    string Description,
    string FullName,
    string Email,
    string PhoneNumber,
    string Country,
    string Governorate,
    string PostalCode,
    string AddressDetails,
    Domain.Enums.PaymentMethodType PaymentMethod = Domain.Enums.PaymentMethodType.CreditCard);

public record PaymobIntentionResult(
    string IntentionId,
    string ClientSecret,
    long? IntentionOrderId,
    string CheckoutUrl);
