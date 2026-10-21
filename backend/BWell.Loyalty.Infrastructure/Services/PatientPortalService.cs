using BWell.Loyalty.Application.DTOs.Portal;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Domain.Enums;
using BWell.Loyalty.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BWell.Loyalty.Infrastructure.Services;

public class PatientPortalService : IPatientPortalService
{
    private readonly BWellDbContext _db;

    public PatientPortalService(BWellDbContext db) => _db = db;

    public async Task<PatientPortalDto?> GetMyPortalAsync(Guid userId, CancellationToken ct = default)
    {
        var patient = await _db.Patients
            .Include(p => p.ChronicDiseases).ThenInclude(c => c.ChronicDisease)
            .Include(p => p.FamilyMembers)
            .Include(p => p.Visits).ThenInclude(v => v.MedicalService)
            .Include(p => p.Visits).ThenInclude(v => v.FamilyMember)
            .Include(p => p.Subscriptions).ThenInclude(s => s.LoyaltyProgram).ThenInclude(lp => lp.Benefits)
            .Include(p => p.Subscriptions).ThenInclude(s => s.LoyaltyProgram).ThenInclude(lp => lp.ServiceDiscounts)
                .ThenInclude(d => d.MedicalService)
            .Include(p => p.Subscriptions).ThenInclude(s => s.DeliveryAddress)
            .FirstOrDefaultAsync(p => p.AppUserId == userId, ct);

        if (patient is null) return null;

        var active = patient.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartDateUtc)
            .FirstOrDefault()
            ?? patient.Subscriptions.OrderByDescending(s => s.CreatedAtUtc).FirstOrDefault();

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var visits = patient.Visits.OrderByDescending(v => v.VisitDateUtc).ToList();

        PatientPlanDto? plan = null;
        if (active is not null)
        {
            var benefits = active.LoyaltyProgram.Benefits
                .Where(b => b.AppliesTo == active.PlanType)
                .OrderBy(b => b.SortOrder)
                .Select(b => b.Text)
                .ToList();

            var quotas = active.LoyaltyProgram.ServiceDiscounts.Select(d =>
            {
                var count = visits.Count(v =>
                    v.SubscriptionId == active.Id &&
                    v.MedicalServiceId == d.MedicalServiceId &&
                    v.FamilyMemberId == null);
                var last = visits
                    .Where(v => v.MedicalServiceId == d.MedicalServiceId && v.FamilyMemberId == null)
                    .OrderByDescending(v => v.VisitDateUtc)
                    .FirstOrDefault();
                return new PortalServiceQuotaDto(
                    d.MedicalService.Name,
                    count,
                    last?.DiscountAppliedPercent,
                    d.FirstVisitDiscountPercent,
                    d.SecondVisitDiscountPercent,
                    d.AdditionalVisitDiscountPercent);
            }).ToList();

            plan = new PatientPlanDto(
                active.LoyaltyProgram.Name,
                active.PlanType,
                active.BillingCycle,
                active.Status,
                active.AmountEgp,
                active.StartDateUtc,
                active.NextRenewalDateUtc,
                benefits,
                quotas);
        }

        var estimatedSavings = visits
            .Where(v => v.DiscountAppliedPercent.HasValue)
            .Sum(v => 200m * (v.DiscountAppliedPercent!.Value / 100m)); // illustrative avg visit value

        var family = patient.FamilyMembers.Select(m =>
        {
            var memberVisits = visits.Where(v => v.FamilyMemberId == m.Id).ToList();
            return new PortalFamilyMemberDto(
                m.Id,
                m.FullName,
                m.Age,
                m.Relationship,
                memberVisits.Count,
                memberVisits.Take(10).Select(MapVisit).ToList());
        }).ToList();

        PortalAddressDto? address = null;
        if (active?.DeliveryAddress is not null)
        {
            address = new PortalAddressDto(
                active.DeliveryAddress.Country,
                active.DeliveryAddress.Governorate,
                active.DeliveryAddress.PostalCode,
                active.DeliveryAddress.AddressDetails);
        }

        return new PatientPortalDto(
            patient.Id,
            patient.PatientCode,
            patient.FullName,
            patient.MobileNumber,
            patient.Email,
            patient.Age,
            patient.Nationality,
            patient.ChronicDiseases.Select(c => c.ChronicDisease.Name).ToList(),
            plan,
            new PatientUsageDto(
                visits.Count,
                visits.Count(v => v.VisitDateUtc >= monthStart),
                patient.FamilyMembers.Count,
                Math.Round(estimatedSavings, 2)),
            family,
            visits.Take(20).Select(MapVisit).ToList(),
            address);
    }

    private static PortalVisitDto MapVisit(Domain.Entities.Visit v) =>
        new(
            v.Id,
            v.VisitDateUtc,
            v.MedicalService?.Name,
            v.VisitSequenceForService,
            v.DiscountAppliedPercent,
            v.FamilyMember?.FullName ?? "Primary member",
            v.Notes);
}
