using BWell.Loyalty.Application.DTOs.Subscriptions;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Domain.Entities;
using BWell.Loyalty.Domain.Enums;
using BWell.Loyalty.Infrastructure.Data;
using BWell.Loyalty.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BWell.Loyalty.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly BWellDbContext _db;
    private readonly IPaymobPaymentGateway _paymob;
    private readonly PaymobOptions _paymobOptions;
    private readonly IAuthService _auth;

    public SubscriptionService(
        BWellDbContext db,
        IPaymobPaymentGateway paymob,
        IOptions<PaymobOptions> paymobOptions,
        IAuthService auth)
    {
        _db = db;
        _paymob = paymob;
        _paymobOptions = paymobOptions.Value;
        _auth = auth;
    }

    public PaymentConfigDto GetPaymentConfig()
    {
        var methods = _paymob.GetAvailableMethods().Select(m =>
        {
            var reason = m.Available
                ? null
                : m.Method switch
                {
                    PaymentMethodType.DigitalWallet =>
                        "Add a Wallet integration in Paymob and set Paymob:WalletIntegrationId.",
                    PaymentMethodType.BankTransfer =>
                        "Add a Bank Transfer / Installments integration in Paymob and set Paymob:BankTransferIntegrationId.",
                    _ => "Add a Card integration in Paymob and set Paymob:CardIntegrationId."
                };
            return new PaymentMethodOptionDto(m.Method.ToString(), m.Label, m.Available, reason);
        }).ToList();

        return new PaymentConfigDto(
            "Paymob",
            _paymob.PublicKey,
            _paymob.IsConfigured,
            _paymob.IsMockMode,
            _paymob.Currency,
            $"{_paymobOptions.BaseUrl.TrimEnd('/')}/unifiedcheckout/",
            methods);
    }

    public async Task<CheckoutSessionDto> StartCheckoutAsync(StartCheckoutRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required.");
        if (string.IsNullOrWhiteSpace(request.MobileNumber))
            throw new ArgumentException("Mobile number is required.");

        var program = await _db.LoyaltyPrograms
            .Include(p => p.Benefits)
            .FirstOrDefaultAsync(p => p.Id == request.LoyaltyProgramId && p.IsActive, ct)
            ?? throw new InvalidOperationException("Selected loyalty program was not found or is inactive.");

        if (request.PlanType == PlanType.Family)
        {
            var memberCount = request.FamilyMembers?.Count ?? 0;
            if (memberCount > 4)
                throw new InvalidOperationException("Family plans support up to 4 family members.");
        }
        else if (request.FamilyMembers is { Count: > 0 })
        {
            throw new InvalidOperationException("Family members can only be added on Family plans.");
        }

        var amount = request.PlanType == PlanType.Family ? program.FamilyPriceEgp : program.IndividualPriceEgp;

        var patient = new Patient
        {
            PatientCode = await GeneratePatientCodeAsync(ct),
            FullName = request.FullName.Trim(),
            MobileNumber = request.MobileNumber.Trim(),
            Email = request.Email?.Trim(),
            Age = request.Age,
            Nationality = request.Nationality?.Trim(),
            NationalIdOrPassport = request.NationalIdOrPassport?.Trim(),
            MedicalNotes = request.MedicalNotes?.Trim()
        };

        if (request.ChronicDiseaseIds is { Count: > 0 })
        {
            var diseases = await _db.ChronicDiseases
                .Where(d => request.ChronicDiseaseIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync(ct);

            foreach (var diseaseId in diseases)
                patient.ChronicDiseases.Add(new PatientChronicDisease { ChronicDiseaseId = diseaseId });
        }

        if (request.FamilyMembers is { Count: > 0 })
        {
            foreach (var m in request.FamilyMembers)
            {
                patient.FamilyMembers.Add(new FamilyMember
                {
                    FullName = m.FullName.Trim(),
                    Age = m.Age,
                    Relationship = m.Relationship?.Trim(),
                    NationalIdOrPassport = m.NationalIdOrPassport?.Trim()
                });
            }
        }

        var subscription = new Subscription
        {
            SubscriptionNumber = await GenerateSubscriptionNumberAsync(ct),
            Patient = patient,
            LoyaltyProgramId = program.Id,
            PlanType = request.PlanType,
            BillingCycle = request.BillingCycle,
            Status = SubscriptionStatus.PendingPayment,
            AmountEgp = amount
        };

        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync(ct);

        return ToCheckoutDto(subscription, program.Name, null, null);
    }

    public async Task<CheckoutSessionDto> CompletePaymentAsync(CompletePaymentRequest request, CancellationToken ct = default)
    {
        if (!_paymob.IsMockMode && !_paymob.IsConfigured)
            throw new InvalidOperationException("Paymob is not configured. Set Paymob:SecretKey and Paymob:PublicKey.");

        var subscription = await _db.Subscriptions
            .Include(s => s.Patient)
            .Include(s => s.LoyaltyProgram)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, ct)
            ?? throw new InvalidOperationException("Subscription not found.");

        if (subscription.Status == SubscriptionStatus.Active)
            throw new InvalidOperationException("Subscription is already active.");

        if (string.IsNullOrWhiteSpace(request.DeliveryAddress.Governorate) ||
            string.IsNullOrWhiteSpace(request.DeliveryAddress.AddressDetails) ||
            string.IsNullOrWhiteSpace(request.DeliveryAddress.PostalCode))
        {
            throw new ArgumentException("Delivery address is incomplete.");
        }

        await UpsertDeliveryAddressAsync(subscription.Id, request.DeliveryAddress, ct);

        var payment = new Payment
        {
            SubscriptionId = subscription.Id,
            AmountEgp = subscription.AmountEgp,
            Currency = _paymob.Currency,
            Status = PaymentStatus.Pending,
            PaymentMethod = request.PaymentMethod,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}"
        };

        string? clientSecret = null;
        string? checkoutUrl = null;

        var email = string.IsNullOrWhiteSpace(subscription.Patient.Email)
            ? $"{subscription.Patient.PatientCode.ToLowerInvariant()}@bwell.local"
            : subscription.Patient.Email;

        // Card, wallet, and bank transfer all go through Paymob Unified Checkout when configured.
        if (request.PaymentMethod is PaymentMethodType.CreditCard
            or PaymentMethodType.DigitalWallet
            or PaymentMethodType.BankTransfer)
        {
            var intention = await _paymob.CreateIntentionAsync(new PaymobIntentionRequest(
                subscription.AmountEgp,
                subscription.Id.ToString(),
                $"B.Well {subscription.LoyaltyProgram.Name} ({subscription.PlanType})",
                subscription.Patient.FullName,
                email,
                subscription.Patient.MobileNumber,
                request.DeliveryAddress.Country,
                request.DeliveryAddress.Governorate,
                request.DeliveryAddress.PostalCode,
                request.DeliveryAddress.AddressDetails,
                request.PaymentMethod), ct);

            payment.StripePaymentIntentId = intention.ClientSecret;
            payment.StripeChargeId = intention.IntentionId;
            subscription.StripePaymentIntentId = intention.ClientSecret;
            subscription.StripeSubscriptionId = intention.IntentionOrderId?.ToString();
            subscription.Patient.StripeCustomerId ??= $"paymob_{subscription.Patient.Id:N}";

            clientSecret = intention.ClientSecret;
            checkoutUrl = intention.CheckoutUrl;
            subscription.Status = SubscriptionStatus.PendingPayment;

            if (_paymob.IsMockMode && _paymobOptions.AutoConfirmMockPayments)
                await ActivateSubscriptionAsync(subscription, payment, ct);
        }
        else
        {
            payment.Status = PaymentStatus.Pending;
            subscription.Status = SubscriptionStatus.PendingPayment;
        }

        _db.Payments.Add(payment);
        subscription.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return ToCheckoutDto(subscription, subscription.LoyaltyProgram.Name, clientSecret, checkoutUrl);
    }

    public async Task<OrderSummaryDto> ConfirmPaymentAsync(ConfirmPaymentRequest request, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .Include(s => s.LoyaltyProgram)
            .Include(s => s.Patient)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, ct)
            ?? throw new InvalidOperationException("Subscription not found.");

        if (subscription.Status == SubscriptionStatus.Active)
            return (await GetOrderSummaryAsync(subscription.Id, ct))!;

        var payment = subscription.Payments
            .OrderByDescending(p => p.CreatedAtUtc)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("No payment found for this subscription.");

        if (string.IsNullOrWhiteSpace(payment.StripePaymentIntentId))
            throw new InvalidOperationException("Payment has no Paymob client secret.");

        var (succeeded, status, failureMessage) = await _paymob.GetIntentionStatusAsync(payment.StripePaymentIntentId, ct);

        if (succeeded)
        {
            await ActivateSubscriptionAsync(subscription, payment, ct);
            await _db.SaveChangesAsync(ct);
            return (await GetOrderSummaryAsync(subscription.Id, ct))!;
        }

        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = failureMessage ?? $"Paymob status: {status}";
        subscription.Status = SubscriptionStatus.Failed;
        subscription.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        throw new InvalidOperationException(payment.FailureReason);
    }

    public async Task<OrderSummaryDto> MarkPaymentFailedAsync(MarkPaymentFailedRequest request, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .Include(s => s.LoyaltyProgram)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, ct)
            ?? throw new InvalidOperationException("Subscription not found.");

        if (subscription.Status == SubscriptionStatus.Active)
            throw new InvalidOperationException("Cannot mark an active subscription as failed.");

        var payment = subscription.Payments.OrderByDescending(p => p.CreatedAtUtc).FirstOrDefault();
        if (payment is null)
        {
            payment = new Payment
            {
                SubscriptionId = subscription.Id,
                AmountEgp = subscription.AmountEgp,
                Currency = _paymob.Currency,
                PaymentMethod = PaymentMethodType.CreditCard,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}"
            };
            _db.Payments.Add(payment);
        }

        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = string.IsNullOrWhiteSpace(request.FailureReason)
            ? "Payment failed on client"
            : request.FailureReason.Trim();
        subscription.Status = SubscriptionStatus.Failed;
        subscription.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return (await GetOrderSummaryAsync(subscription.Id, ct))!;
    }

    public async Task<OrderSummaryDto?> GetOrderSummaryAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .Include(s => s.LoyaltyProgram)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId, ct);

        if (subscription is null) return null;

        var latestPayment = subscription.Payments.OrderByDescending(p => p.CreatedAtUtc).FirstOrDefault();

        return new OrderSummaryDto(
            subscription.Id,
            subscription.SubscriptionNumber,
            subscription.LoyaltyProgram.Name,
            subscription.PlanType,
            subscription.BillingCycle,
            subscription.AmountEgp,
            subscription.NextRenewalDateUtc,
            subscription.Status,
            latestPayment?.Status,
            latestPayment?.FailureReason,
            latestPayment?.PaymentMethod);
    }

    public async Task HandlePaymobWebhookAsync(string json, string? hmacQuery, CancellationToken ct = default)
    {
        if (!_paymob.TryValidateWebhook(json, hmacQuery, out var specialReference, out var transactionId, out var succeeded, out var failureMessage))
            throw new UnauthorizedAccessException("Invalid Paymob webhook signature or payload.");

        Subscription? subscription = null;

        if (Guid.TryParse(specialReference, out var subscriptionId))
        {
            subscription = await _db.Subscriptions
                .Include(s => s.Payments)
                .Include(s => s.Patient)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, ct);
        }

        if (subscription is null && !string.IsNullOrWhiteSpace(specialReference))
        {
            subscription = await _db.Subscriptions
                .Include(s => s.Payments)
                .Include(s => s.Patient)
                .FirstOrDefaultAsync(s => s.SubscriptionNumber == specialReference, ct);
        }

        if (subscription is null)
            return;

        var payment = subscription.Payments.OrderByDescending(p => p.CreatedAtUtc).FirstOrDefault();
        if (payment is null)
        {
            payment = new Payment
            {
                SubscriptionId = subscription.Id,
                AmountEgp = subscription.AmountEgp,
                Currency = _paymob.Currency,
                PaymentMethod = PaymentMethodType.CreditCard,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}"
            };
            _db.Payments.Add(payment);
        }

        if (!string.IsNullOrWhiteSpace(transactionId))
            payment.StripeChargeId = transactionId;

        if (subscription.Status == SubscriptionStatus.Active && payment.Status == PaymentStatus.Succeeded)
            return;

        if (succeeded)
        {
            await ActivateSubscriptionAsync(subscription, payment, ct);
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = failureMessage ?? "Payment failed";
            subscription.Status = SubscriptionStatus.Failed;
            subscription.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task UpsertDeliveryAddressAsync(Guid subscriptionId, DeliveryAddressRequest request, CancellationToken ct)
    {
        var country = string.IsNullOrWhiteSpace(request.Country) ? "Egypt" : request.Country.Trim();
        var existingAddress = await _db.DeliveryAddresses.FirstOrDefaultAsync(a => a.SubscriptionId == subscriptionId, ct);

        if (existingAddress is null)
        {
            _db.DeliveryAddresses.Add(new DeliveryAddress
            {
                SubscriptionId = subscriptionId,
                Country = country,
                Governorate = request.Governorate.Trim(),
                PostalCode = request.PostalCode.Trim(),
                AddressDetails = request.AddressDetails.Trim()
            });
        }
        else
        {
            existingAddress.Country = country;
            existingAddress.Governorate = request.Governorate.Trim();
            existingAddress.PostalCode = request.PostalCode.Trim();
            existingAddress.AddressDetails = request.AddressDetails.Trim();
            existingAddress.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    private CheckoutSessionDto ToCheckoutDto(Subscription subscription, string planName, string? clientSecret, string? checkoutUrl) =>
        new(
            subscription.Id,
            subscription.SubscriptionNumber,
            planName,
            subscription.PlanType,
            subscription.BillingCycle,
            subscription.AmountEgp,
            clientSecret,
            _paymobOptions.PublicKey,
            checkoutUrl,
            subscription.Status);

    private async Task ActivateSubscriptionAsync(Subscription subscription, Payment payment, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        payment.Status = PaymentStatus.Succeeded;
        payment.PaidAtUtc = now;
        payment.FailureReason = null;

        subscription.Status = SubscriptionStatus.Active;
        subscription.StartDateUtc ??= now;
        subscription.NextRenewalDateUtc = subscription.BillingCycle == BillingCycle.Monthly
            ? now.AddMonths(1)
            : now.AddYears(1);
        subscription.UpdatedAtUtc = now;

        if (subscription.Patient is not null)
            await _auth.EnsurePatientUserAsync(subscription.Patient, AuthService.DefaultPatientPassword, ct);
    }

    private async Task<string> GeneratePatientCodeAsync(CancellationToken ct)
    {
        var count = await _db.Patients.IgnoreQueryFilters().CountAsync(ct);
        return $"P{(count + 1).ToString().PadLeft(3, '0')}";
    }

    private async Task<string> GenerateSubscriptionNumberAsync(CancellationToken ct)
    {
        var count = await _db.Subscriptions.IgnoreQueryFilters().CountAsync(ct);
        return $"SUB-{DateTime.UtcNow:yyyy}-{count + 1:D5}";
    }
}
