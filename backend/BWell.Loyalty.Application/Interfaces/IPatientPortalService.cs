using BWell.Loyalty.Application.DTOs.Portal;

namespace BWell.Loyalty.Application.Interfaces;

public interface IPatientPortalService
{
    Task<PatientPortalDto?> GetMyPortalAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<BenefitRedemptionDto>> GetMyBenefitRedemptionsAsync(
        Guid userId,
        CancellationToken ct = default);
    Task<BenefitRedemptionDto> CreateBenefitRedemptionAsync(
        Guid userId,
        CreateBenefitRedemptionRequest request,
        CancellationToken ct = default);
}
