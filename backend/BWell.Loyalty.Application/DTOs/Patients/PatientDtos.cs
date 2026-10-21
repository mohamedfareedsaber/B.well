using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Application.DTOs.Patients;

public record PatientListItemDto(
    Guid Id,
    string PatientCode,
    string FullName,
    string MobileNumber,
    string? Email,
    DateTime SubscribedAtUtc,
    int VisitCount,
    string? PlanName,
    PlanType? PlanType,
    IReadOnlyList<string> ChronicDiseases);

public record PatientDetailsDto(
    Guid Id,
    string PatientCode,
    string FullName,
    string MobileNumber,
    string? Email,
    int? Age,
    string? Nationality,
    string? NationalIdOrPassport,
    string? MedicalNotes,
    DateTime CreatedAtUtc,
    string? ActivePlanName,
    PlanType? ActivePlanType,
    SubscriptionStatus? SubscriptionStatus,
    DateTime? NextRenewalDateUtc,
    IReadOnlyList<string> ChronicDiseases,
    IReadOnlyList<FamilyMemberDto> FamilyMembers,
    IReadOnlyList<VisitDto> Visits,
    DeliveryAddressDto? DeliveryAddress);

public record FamilyMemberDto(
    Guid Id,
    string FullName,
    int? Age,
    string? Relationship,
    string? NationalIdOrPassport);

public record VisitDto(
    Guid Id,
    DateTime VisitDateUtc,
    string? MedicalServiceName,
    int VisitSequenceForService,
    decimal? DiscountAppliedPercent,
    string? Notes);

public record DeliveryAddressDto(
    string Country,
    string Governorate,
    string PostalCode,
    string AddressDetails);

public record PatientsOverviewDto(int TotalPatients, int TotalVisits);
