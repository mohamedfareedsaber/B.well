using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Application.DTOs.Programs;

public record ProgramBenefitDto(Guid Id, PlanType AppliesTo, string Text, int SortOrder);

public record ServiceDiscountDto(
    Guid Id,
    Guid MedicalServiceId,
    string MedicalServiceName,
    decimal FirstVisitDiscountPercent,
    decimal SecondVisitDiscountPercent,
    decimal AdditionalVisitDiscountPercent);

public record LoyaltyProgramDto(
    Guid Id,
    string Name,
    string Description,
    ProgramIcon Icon,
    ColorGradient ColorGradient,
    decimal IndividualPriceEgp,
    decimal FamilyPriceEgp,
    DateTime? OfferExpiryDate,
    bool IsActive,
    int SortOrder,
    int MemberCount,
    decimal RevenueEgp,
    IReadOnlyList<ProgramBenefitDto> Benefits,
    IReadOnlyList<ServiceDiscountDto> ServiceDiscounts);

public record CreateLoyaltyProgramRequest(
    string Name,
    string Description,
    ProgramIcon Icon,
    ColorGradient ColorGradient,
    decimal IndividualPriceEgp,
    decimal FamilyPriceEgp,
    DateTime? OfferExpiryDate,
    IReadOnlyList<string> IndividualBenefits,
    IReadOnlyList<string> FamilyBenefits,
    IReadOnlyList<CreateServiceDiscountRequest>? ServiceDiscounts);

public record CreateServiceDiscountRequest(
    Guid MedicalServiceId,
    decimal FirstVisitDiscountPercent,
    decimal SecondVisitDiscountPercent,
    decimal AdditionalVisitDiscountPercent);

public record UpdateLoyaltyProgramRequest(
    string Name,
    string Description,
    ProgramIcon Icon,
    ColorGradient ColorGradient,
    decimal IndividualPriceEgp,
    decimal FamilyPriceEgp,
    DateTime? OfferExpiryDate,
    bool IsActive,
    IReadOnlyList<string> IndividualBenefits,
    IReadOnlyList<string> FamilyBenefits,
    IReadOnlyList<CreateServiceDiscountRequest>? ServiceDiscounts);

public record PricingCardDto(
    Guid Id,
    string Name,
    string Description,
    ProgramIcon Icon,
    ColorGradient ColorGradient,
    decimal PriceEgp,
    PlanType PlanType,
    IReadOnlyList<string> Benefits,
    int ExtraBenefitsCount);
