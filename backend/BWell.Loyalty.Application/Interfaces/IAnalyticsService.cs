using BWell.Loyalty.Application.DTOs.Analytics;

namespace BWell.Loyalty.Application.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ChronicDiseaseLookupDto>> GetChronicDiseasesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MedicalServiceLookupDto>> GetMedicalServicesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GovernorateLookupDto>> GetGovernoratesAsync(CancellationToken ct = default);
}
