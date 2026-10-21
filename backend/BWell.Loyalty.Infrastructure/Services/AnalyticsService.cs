using BWell.Loyalty.Application.DTOs.Analytics;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Domain.Enums;
using BWell.Loyalty.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BWell.Loyalty.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly BWellDbContext _db;

    public AnalyticsService(BWellDbContext db) => _db = db;

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(CancellationToken ct = default)
    {
        var activeSubs = await _db.Subscriptions
            .Include(s => s.LoyaltyProgram)
            .Include(s => s.DeliveryAddress)
            .Where(s => s.Status == SubscriptionStatus.Active)
            .ToListAsync(ct);

        var programs = await _db.LoyaltyPrograms.Where(p => p.IsActive).OrderBy(p => p.SortOrder).ToListAsync(ct);
        var totalRevenue = activeSubs.Sum(s => s.AmountEgp);
        var totalSubs = activeSubs.Count;
        var avg = totalSubs == 0 ? 0 : Math.Round(totalRevenue / totalSubs, 2);

        var performance = programs.Select(p =>
        {
            var members = activeSubs.Where(s => s.LoyaltyProgramId == p.Id).ToList();
            var revenue = members.Sum(s => s.AmountEgp);
            var share = totalRevenue == 0 ? 0 : Math.Round(revenue / totalRevenue * 100m, 1);
            return new ProgramPerformanceDto(
                p.Id,
                p.Name,
                p.Icon.ToString(),
                p.ColorGradient.ToString(),
                members.Count,
                revenue,
                share);
        }).ToList();

        var patients = await _db.Patients
            .Include(p => p.ChronicDiseases).ThenInclude(c => c.ChronicDisease)
            .ToListAsync(ct);

        var withConditions = patients.Where(p => p.ChronicDiseases.Count > 0).ToList();
        var percent = patients.Count == 0 ? 0 : Math.Round((decimal)withConditions.Count / patients.Count * 100m, 1);

        var breakdown = await _db.ChronicDiseases
            .Select(d => new
            {
                d.Name,
                Count = d.Patients.Count
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        var diseaseStats = new ChronicDiseaseStatsDto(
            percent,
            withConditions.Count,
            patients.Count,
            breakdown.Select(b => new ChronicDiseaseBreakdownDto(
                b.Name,
                b.Count,
                withConditions.Count == 0 ? 0 : Math.Round((decimal)b.Count / withConditions.Count * 100m, 1))).ToList());

        var governorates = await _db.Governorates.ToListAsync(ct);
        var geo = governorates.Select(g =>
        {
            var count = activeSubs.Count(s =>
                s.DeliveryAddress != null &&
                string.Equals(s.DeliveryAddress.Governorate, g.Name, StringComparison.OrdinalIgnoreCase));

            var density = count switch
            {
                <= 0 => "Low",
                <= 10 => "Low",
                <= 100 => "Medium",
                _ => "High"
            };

            return new GeographicPointDto(g.Id, g.Name, g.NameAr, g.Latitude, g.Longitude, count, density);
        }).ToList();

        return new DashboardOverviewDto(
            totalRevenue,
            totalSubs,
            programs.Count,
            avg,
            performance,
            diseaseStats,
            geo);
    }

    public async Task<IReadOnlyList<ChronicDiseaseLookupDto>> GetChronicDiseasesAsync(CancellationToken ct = default) =>
        await _db.ChronicDiseases.Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new ChronicDiseaseLookupDto(d.Id, d.Name, d.Code))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<MedicalServiceLookupDto>> GetMedicalServicesAsync(CancellationToken ct = default) =>
        await _db.MedicalServices.Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new MedicalServiceLookupDto(s.Id, s.Name, s.Description))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<GovernorateLookupDto>> GetGovernoratesAsync(CancellationToken ct = default) =>
        await _db.Governorates
            .OrderBy(g => g.Name)
            .Select(g => new GovernorateLookupDto(g.Id, g.Name, g.NameAr))
            .ToListAsync(ct);
}
