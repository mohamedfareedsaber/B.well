using BWell.Loyalty.Application.DTOs.Portal;

namespace BWell.Loyalty.Application.Interfaces;

public interface IPatientPortalService
{
    Task<PatientPortalDto?> GetMyPortalAsync(Guid userId, CancellationToken ct = default);
}
