using BWell.Loyalty.Application.Common;
using BWell.Loyalty.Application.DTOs.Analytics;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BWell.Loyalty.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;

    public AnalyticsController(IAnalyticsService analytics) => _analytics = analytics;

    [Authorize(Policy = DependencyInjection.AdminPolicy)]
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<DashboardOverviewDto>>> Dashboard(CancellationToken ct)
        => Ok(ApiResponse<DashboardOverviewDto>.Ok(await _analytics.GetDashboardOverviewAsync(ct)));

    [AllowAnonymous]
    [HttpGet("lookups/chronic-diseases")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChronicDiseaseLookupDto>>>> ChronicDiseases(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<ChronicDiseaseLookupDto>>.Ok(await _analytics.GetChronicDiseasesAsync(ct)));

    [Authorize(Policy = DependencyInjection.AdminPolicy)]
    [HttpGet("lookups/medical-services")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MedicalServiceLookupDto>>>> MedicalServices(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<MedicalServiceLookupDto>>.Ok(await _analytics.GetMedicalServicesAsync(ct)));

    [AllowAnonymous]
    [HttpGet("lookups/governorates")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GovernorateLookupDto>>>> Governorates(CancellationToken ct)
        => Ok(ApiResponse<IReadOnlyList<GovernorateLookupDto>>.Ok(await _analytics.GetGovernoratesAsync(ct)));
}
