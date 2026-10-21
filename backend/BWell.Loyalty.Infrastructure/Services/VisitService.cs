using BWell.Loyalty.Application.DTOs.Visits;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Domain.Entities;
using BWell.Loyalty.Domain.Enums;
using BWell.Loyalty.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BWell.Loyalty.Infrastructure.Services;

public class VisitService : IVisitService
{
    private readonly BWellDbContext _db;

    public VisitService(BWellDbContext db) => _db = db;

    public async Task<RecordVisitResultDto> RecordVisitAsync(RecordVisitRequest request, CancellationToken ct = default)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == request.PatientId, ct)
            ?? throw new InvalidOperationException("Patient not found.");

        var service = await _db.MedicalServices.FirstOrDefaultAsync(s => s.Id == request.MedicalServiceId && s.IsActive, ct)
            ?? throw new InvalidOperationException("Medical service not found.");

        var subscription = request.SubscriptionId.HasValue
            ? await _db.Subscriptions.Include(s => s.LoyaltyProgram).ThenInclude(p => p.ServiceDiscounts)
                .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.PatientId == patient.Id, ct)
            : await _db.Subscriptions.Include(s => s.LoyaltyProgram).ThenInclude(p => p.ServiceDiscounts)
                .Where(s => s.PatientId == patient.Id && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.StartDateUtc)
                .FirstOrDefaultAsync(ct);

        if (subscription is null || subscription.Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Patient does not have an active subscription.");

        var previousCount = await _db.Visits.CountAsync(v =>
            v.PatientId == patient.Id &&
            v.MedicalServiceId == service.Id &&
            v.SubscriptionId == subscription.Id, ct);

        var sequence = previousCount + 1;

        var discountRule = subscription.LoyaltyProgram.ServiceDiscounts
            .FirstOrDefault(d => d.MedicalServiceId == service.Id);

        decimal discount = 0;
        if (discountRule is not null)
        {
            discount = sequence switch
            {
                1 => discountRule.FirstVisitDiscountPercent,
                2 => discountRule.SecondVisitDiscountPercent,
                _ => discountRule.AdditionalVisitDiscountPercent
            };
        }

        var visit = new Visit
        {
            PatientId = patient.Id,
            MedicalServiceId = service.Id,
            SubscriptionId = subscription.Id,
            VisitDateUtc = DateTime.UtcNow,
            VisitSequenceForService = sequence,
            DiscountAppliedPercent = discount,
            Notes = request.Notes?.Trim()
        };

        _db.Visits.Add(visit);
        await _db.SaveChangesAsync(ct);

        var message = discountRule is null
            ? $"Visit recorded. No tiered discount configured for {service.Name} on {subscription.LoyaltyProgram.Name}."
            : $"Visit #{sequence} for {service.Name}: {discount}% discount applied ({subscription.LoyaltyProgram.Name}).";

        return new RecordVisitResultDto(visit.Id, patient.Id, service.Name, sequence, discount, message);
    }
}
