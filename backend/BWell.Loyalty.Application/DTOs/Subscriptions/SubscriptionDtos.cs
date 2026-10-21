using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Application.DTOs.Subscriptions;

public record FamilyMemberRequest(
    string FullName,
    int? Age,
    string? Relationship,
    string? NationalIdOrPassport);

public record DeliveryAddressRequest(
    string Country,
    string Governorate,
    string PostalCode,
    string AddressDetails);

public record StartCheckoutRequest(
    Guid LoyaltyProgramId,
    PlanType PlanType,
    BillingCycle BillingCycle,
    string FullName,
    string MobileNumber,
    int? Age,
    string? Email,
    string? Nationality,
    string? NationalIdOrPassport,
    IReadOnlyList<Guid>? ChronicDiseaseIds,
    string? MedicalNotes,
    IReadOnlyList<FamilyMemberRequest>? FamilyMembers);

public record CompletePaymentRequest(
    Guid SubscriptionId,
    DeliveryAddressRequest DeliveryAddress,
    PaymentMethodType PaymentMethod,
    string? PaymentMethodId);

public record CheckoutSessionDto(
    Guid SubscriptionId,
    string SubscriptionNumber,
    string PlanName,
    PlanType PlanType,
    BillingCycle BillingCycle,
    decimal AmountEgp,
    string? ClientSecret,
    string? PublicKey,
    string? CheckoutUrl,
    SubscriptionStatus Status);

public record OrderSummaryDto(
    Guid SubscriptionId,
    string SubscriptionNumber,
    string PlanName,
    PlanType PlanType,
    BillingCycle BillingCycle,
    decimal TotalPaidEgp,
    DateTime? NextRenewalDateUtc,
    SubscriptionStatus Status,
    PaymentStatus? PaymentStatus,
    string? FailureReason,
    PaymentMethodType? PaymentMethod);

public record PaymentFailedDto(
    Guid SubscriptionId,
    string PlanName,
    PlanType PlanType,
    decimal AmountEgp,
    PaymentStatus Status,
    PaymentMethodType PaymentMethod,
    string? FailureReason);
