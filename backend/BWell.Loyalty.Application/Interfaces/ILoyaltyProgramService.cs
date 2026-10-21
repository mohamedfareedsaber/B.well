using BWell.Loyalty.Application.DTOs.Programs;
using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Application.Interfaces;

public interface ILoyaltyProgramService
{
    Task<IReadOnlyList<LoyaltyProgramDto>> GetAllAsync(CancellationToken ct = default);
    Task<LoyaltyProgramDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<PricingCardDto>> GetPricingCardsAsync(PlanType planType, CancellationToken ct = default);
    Task<LoyaltyProgramDto> CreateAsync(CreateLoyaltyProgramRequest request, CancellationToken ct = default);
    Task<LoyaltyProgramDto?> UpdateAsync(Guid id, UpdateLoyaltyProgramRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
