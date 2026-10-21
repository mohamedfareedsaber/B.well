using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Application.DTOs.Portal;

public record PatientPortalDto(
    Guid PatientId,
    string PatientCode,
    string FullName,
    string MobileNumber,
    string? Email,
    int? Age,
    string? Nationality,
    IReadOnlyList<string> ChronicDiseases,
    PatientPlanDto? Plan,
    PatientUsageDto Usage,
    IReadOnlyList<PortalFamilyMemberDto> FamilyMembers,
    IReadOnlyList<PortalVisitDto> RecentVisits,
    PortalAddressDto? DeliveryAddress);

public record PatientPlanDto(
    string PlanName,
    PlanType PlanType,
    BillingCycle BillingCycle,
    SubscriptionStatus Status,
    decimal AmountEgp,
    DateTime? StartDateUtc,
    DateTime? NextRenewalDateUtc,
    IReadOnlyList<string> Benefits,
    IReadOnlyList<PortalServiceQuotaDto> ServiceQuotas);

public record PortalServiceQuotaDto(
    string ServiceName,
    int VisitCount,
    decimal? LastDiscountPercent,
    decimal FirstVisitDiscountPercent,
    decimal SecondVisitDiscountPercent,
    decimal AdditionalVisitDiscountPercent);

public record PatientUsageDto(
    int TotalVisits,
    int VisitsThisMonth,
    int FamilyMemberCount,
    decimal EstimatedSavingsEgp);

public record PortalFamilyMemberDto(
    Guid Id,
    string FullName,
    int? Age,
    string? Relationship,
    int VisitCount,
    IReadOnlyList<PortalVisitDto> Visits);

public record PortalVisitDto(
    Guid Id,
    DateTime VisitDateUtc,
    string? MedicalServiceName,
    int VisitSequenceForService,
    decimal? DiscountAppliedPercent,
    string? MemberName,
    string? Notes);

public record PortalAddressDto(
    string Country,
    string Governorate,
    string PostalCode,
    string AddressDetails);
