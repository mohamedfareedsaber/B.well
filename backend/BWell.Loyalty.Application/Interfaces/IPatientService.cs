using BWell.Loyalty.Application.DTOs.Patients;

namespace BWell.Loyalty.Application.Interfaces;

public interface IPatientService
{
    Task<PatientsOverviewDto> GetOverviewAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PatientListItemDto>> SearchAsync(string? query, CancellationToken ct = default);
    Task<PatientDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
