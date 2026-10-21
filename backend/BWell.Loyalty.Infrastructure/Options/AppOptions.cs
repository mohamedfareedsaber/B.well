namespace BWell.Loyalty.Infrastructure.Options;

public class PaymobOptions
{
    public const string SectionName = "Paymob";

    /// <summary>Classic Paymob API key (JWT) used for auth token / utilities.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>egy_sk_test_... / egy_sk_live_...</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>egy_pk_test_... / egy_pk_live_...</summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>HMAC secret from Paymob dashboard for webhook verification.</summary>
    public string HmacSecret { get; set; } = string.Empty;

    /// <summary>Card/wallet integration IDs from Paymob → Settings → Payment Integrations. Used as Card fallback.</summary>
    public int[] IntegrationIds { get; set; } = [];

    /// <summary>Primary Online Card integration ID.</summary>
    public int? CardIntegrationId { get; set; }

    /// <summary>Mobile wallet (Vodafone Cash, etc.) integration ID.</summary>
    public int? WalletIntegrationId { get; set; }

    /// <summary>Bank installment / bank transfer style integration ID when available in Paymob.</summary>
    public int? BankTransferIntegrationId { get; set; }

    /// <summary>Named methods when IntegrationIds is empty, e.g. card.</summary>
    public string[] PaymentMethodNames { get; set; } = ["card"];

    public string BaseUrl { get; set; } = "https://accept.paymob.com";
    public string Currency { get; set; } = "EGP";

    /// <summary>Public backend URL for Paymob notification_url, e.g. https://api.example.com</summary>
    public string NotificationBaseUrl { get; set; } = "http://localhost:5068";

    /// <summary>Frontend redirect after payment, e.g. http://localhost:5173/checkout/result</summary>
    public string RedirectionUrl { get; set; } = "http://localhost:5173/checkout/result";

    public bool UseMockPayments { get; set; }
    public bool AutoConfirmMockPayments { get; set; } = true;
}

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "BWell.Loyalty";
    public string Audience { get; set; } = "BWell.Loyalty.Client";
    public string Key { get; set; } = string.Empty;
    public int ExpiryHours { get; set; } = 8;
}

public class CorsOptions
{
    public const string SectionName = "Cors";
    public string[] AllowedOrigins { get; set; } = ["http://localhost:5173", "http://localhost:3000"];
}

public class SecurityOptions
{
    public const string SectionName = "Security";
    public int LoginPermitLimit { get; set; } = 10;
    public int PaymentPermitLimit { get; set; } = 30;
    public int GlobalPermitLimit { get; set; } = 200;
}
