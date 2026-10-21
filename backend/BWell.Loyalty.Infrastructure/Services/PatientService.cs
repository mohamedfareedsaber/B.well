using BWell.Loyalty.Application.DTOs.Patients;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Domain.Enums;
using BWell.Loyalty.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BWell.Loyalty.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly BWellDbContext _db;

    public PatientService(BWellDbContext db) => _db = db;

    public async Task<PatientsOverviewDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var totalPatients = await _db.Patients.CountAsync(ct);
        var totalVisits = await _db.Visits.CountAsync(ct);
        return new PatientsOverviewDto(totalPatients, totalVisits);
    }

    public async Task<IReadOnlyList<PatientListItemDto>> SearchAsync(string? query, CancellationToken ct = default)
    {
        var q = query?.Trim().ToLowerInvariant();

        var patients = await _db.Patients
            .Include(p => p.ChronicDiseases).ThenInclude(c => c.ChronicDisease)
            .Include(p => p.Subscriptions).ThenInclude(s => s.LoyaltyProgram)
            .Include(p => p.Visits)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);

        if (!string.IsNullOrWhiteSpace(q))
        {
            patients = patients.Where(p =>
                p.FullName.ToLowerInvariant().Contains(q) ||
                p.MobileNumber.Contains(q) ||
                (p.NationalIdOrPassport?.ToLowerInvariant().Contains(q) ?? false) ||
                p.PatientCode.ToLowerInvariant().Contains(q) ||
                (p.Email?.ToLowerInvariant().Contains(q) ?? false)).ToList();
        }

        return patients.Select(p =>
        {
            var active = p.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.StartDateUtc)
                .FirstOrDefault();

            return new PatientListItemDto(
                p.Id,
                p.PatientCode,
                p.FullName,
                p.MobileNumber,
                p.Email,
                active?.StartDateUtc ?? p.CreatedAtUtc,
                p.Visits.Count,
                active?.LoyaltyProgram.Name,
                active?.PlanType,
                p.ChronicDiseases.Select(c => c.ChronicDisease.Name).ToList());
        }).ToList();
    }

    public async Task<PatientDetailsDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _db.Patients
            .Include(x => x.ChronicDiseases).ThenInclude(c => c.ChronicDisease)
            .Include(x => x.FamilyMembers)
            .Include(x => x.Visits).ThenInclude(v => v.MedicalService)
            .Include(x => x.Subscriptions).ThenInclude(s => s.LoyaltyProgram)
            .Include(x => x.Subscriptions).ThenInclude(s => s.DeliveryAddress)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (p is null) return null;

        var active = p.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.StartDateUtc)
            .FirstOrDefault()
            ?? p.Subscriptions.OrderByDescending(s => s.CreatedAtUtc).FirstOrDefault();

        DeliveryAddressDto? address = null;
        if (active?.DeliveryAddress is not null)
        {
            address = new DeliveryAddressDto(
                active.DeliveryAddress.Country,
                active.DeliveryAddress.Governorate,
                active.DeliveryAddress.PostalCode,
                active.DeliveryAddress.AddressDetails);
        }

        return new PatientDetailsDto(
            p.Id,
            p.PatientCode,
            p.FullName,
            p.MobileNumber,
            p.Email,
            p.Age,
            p.Nationality,
            p.NationalIdOrPassport,
            p.MedicalNotes,
            p.CreatedAtUtc,
            active?.LoyaltyProgram.Name,
            active?.PlanType,
            active?.Status,
            active?.NextRenewalDateUtc,
            p.ChronicDiseases.Select(c => c.ChronicDisease.Name).ToList(),
            p.FamilyMembers.Select(m => new FamilyMemberDto(m.Id, m.FullName, m.Age, m.Relationship, m.NationalIdOrPassport)).ToList(),
            p.Visits.OrderByDescending(v => v.VisitDateUtc).Select(v => new VisitDto(
                v.Id,
                v.VisitDateUtc,
                v.MedicalService?.Name,
                v.VisitSequenceForService,
                v.DiscountAppliedPercent,
                v.Notes)).ToList(),
            address);
    }
}
