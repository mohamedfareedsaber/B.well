using BWell.Loyalty.Application.DTOs.Programs;
using BWell.Loyalty.Application.Interfaces;
using BWell.Loyalty.Domain.Entities;
using BWell.Loyalty.Domain.Enums;
using BWell.Loyalty.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BWell.Loyalty.Infrastructure.Services;

public class LoyaltyProgramService : ILoyaltyProgramService
{
    private readonly BWellDbContext _db;

    public LoyaltyProgramService(BWellDbContext db) => _db = db;

    public async Task<IReadOnlyList<LoyaltyProgramDto>> GetAllAsync(CancellationToken ct = default)
    {
        var programs = await _db.LoyaltyPrograms
            .Include(p => p.Benefits)
            .Include(p => p.ServiceDiscounts).ThenInclude(d => d.MedicalService)
            .Include(p => p.Subscriptions)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        return programs.Select(Map).ToList();
    }

    public async Task<LoyaltyProgramDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var program = await _db.LoyaltyPrograms
            .Include(p => p.Benefits)
            .Include(p => p.ServiceDiscounts).ThenInclude(d => d.MedicalService)
            .Include(p => p.Subscriptions)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return program is null ? null : Map(program);
    }

    public async Task<IReadOnlyList<PricingCardDto>> GetPricingCardsAsync(PlanType planType, CancellationToken ct = default)
    {
        var programs = await _db.LoyaltyPrograms
            .Include(p => p.Benefits)
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);

        return programs.Select(p =>
        {
            var benefits = p.Benefits
                .Where(b => b.AppliesTo == planType)
                .OrderBy(b => b.SortOrder)
                .Select(b => b.Text)
                .ToList();

            var visible = benefits.Take(4).ToList();
            return new PricingCardDto(
                p.Id,
                p.Name,
                p.Description,
                p.Icon,
                p.ColorGradient,
                planType == PlanType.Family ? p.FamilyPriceEgp : p.IndividualPriceEgp,
                planType,
                visible,
                Math.Max(0, benefits.Count - visible.Count));
        }).ToList();
    }

    public async Task<LoyaltyProgramDto> CreateAsync(CreateLoyaltyProgramRequest request, CancellationToken ct = default)
    {
        ValidatePrices(request.IndividualPriceEgp, request.FamilyPriceEgp);

        var maxOrder = await _db.LoyaltyPrograms.MaxAsync(p => (int?)p.SortOrder, ct) ?? 0;
        var program = new LoyaltyProgram
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Icon = request.Icon,
            ColorGradient = request.ColorGradient,
            IndividualPriceEgp = request.IndividualPriceEgp,
            FamilyPriceEgp = request.FamilyPriceEgp,
            OfferExpiryDate = request.OfferExpiryDate,
            SortOrder = maxOrder + 1,
            IsActive = true
        };

        AddBenefits(program, request.IndividualBenefits, PlanType.Individual);
        AddBenefits(program, request.FamilyBenefits, PlanType.Family);
        await AddServiceDiscountsAsync(program, request.ServiceDiscounts, ct);

        _db.LoyaltyPrograms.Add(program);
        await _db.SaveChangesAsync(ct);
        return (await GetByIdAsync(program.Id, ct))!;
    }

    public async Task<LoyaltyProgramDto?> UpdateAsync(Guid id, UpdateLoyaltyProgramRequest request, CancellationToken ct = default)
    {
        ValidatePrices(request.IndividualPriceEgp, request.FamilyPriceEgp);

        var program = await _db.LoyaltyPrograms
            .Include(p => p.Benefits)
            .Include(p => p.ServiceDiscounts)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (program is null) return null;

        program.Name = request.Name.Trim();
        program.Description = request.Description.Trim();
        program.Icon = request.Icon;
        program.ColorGradient = request.ColorGradient;
        program.IndividualPriceEgp = request.IndividualPriceEgp;
        program.FamilyPriceEgp = request.FamilyPriceEgp;
        program.OfferExpiryDate = request.OfferExpiryDate;
        program.IsActive = request.IsActive;
        program.UpdatedAtUtc = DateTime.UtcNow;

        _db.ProgramBenefits.RemoveRange(program.Benefits);
        _db.ProgramServiceDiscounts.RemoveRange(program.ServiceDiscounts);
        program.Benefits.Clear();
        program.ServiceDiscounts.Clear();

        AddBenefits(program, request.IndividualBenefits, PlanType.Individual);
        AddBenefits(program, request.FamilyBenefits, PlanType.Family);
        await AddServiceDiscountsAsync(program, request.ServiceDiscounts, ct);

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var program = await _db.LoyaltyPrograms.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (program is null) return false;

        var hasActiveSubs = await _db.Subscriptions.AnyAsync(
            s => s.LoyaltyProgramId == id && s.Status == SubscriptionStatus.Active, ct);
        if (hasActiveSubs)
            throw new InvalidOperationException("Cannot delete a program with active subscriptions. Deactivate it instead.");

        program.IsDeleted = true;
        program.IsActive = false;
        program.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static void ValidatePrices(decimal individual, decimal family)
    {
        if (individual < 0 || family < 0)
            throw new ArgumentException("Prices must be zero or greater.");
        if (family < individual)
            throw new ArgumentException("Family price should not be less than individual price.");
    }

    private static void AddBenefits(LoyaltyProgram program, IReadOnlyList<string>? benefits, PlanType planType)
    {
        if (benefits is null) return;
        var order = 0;
        foreach (var text in benefits.Where(b => !string.IsNullOrWhiteSpace(b)))
        {
            program.Benefits.Add(new ProgramBenefit
            {
                AppliesTo = planType,
                Text = text.Trim(),
                SortOrder = order++
            });
        }
    }

    private async Task AddServiceDiscountsAsync(
        LoyaltyProgram program,
        IReadOnlyList<CreateServiceDiscountRequest>? discounts,
        CancellationToken ct)
    {
        if (discounts is null || discounts.Count == 0) return;

        var serviceIds = discounts.Select(d => d.MedicalServiceId).Distinct().ToList();
        var existing = await _db.MedicalServices.Where(s => serviceIds.Contains(s.Id)).Select(s => s.Id).ToListAsync(ct);
        if (existing.Count != serviceIds.Count)
            throw new ArgumentException("One or more medical services were not found.");

        foreach (var d in discounts)
        {
            ValidateDiscount(d.FirstVisitDiscountPercent);
            ValidateDiscount(d.SecondVisitDiscountPercent);
            ValidateDiscount(d.AdditionalVisitDiscountPercent);

            program.ServiceDiscounts.Add(new ProgramServiceDiscount
            {
                MedicalServiceId = d.MedicalServiceId,
                FirstVisitDiscountPercent = d.FirstVisitDiscountPercent,
                SecondVisitDiscountPercent = d.SecondVisitDiscountPercent,
                AdditionalVisitDiscountPercent = d.AdditionalVisitDiscountPercent
            });
        }
    }

    private static void ValidateDiscount(decimal percent)
    {
        if (percent is < 0 or > 100)
            throw new ArgumentException("Discount percent must be between 0 and 100.");
    }

    private static LoyaltyProgramDto Map(LoyaltyProgram p)
    {
        var activeSubs = p.Subscriptions.Where(s => s.Status == SubscriptionStatus.Active).ToList();
        return new LoyaltyProgramDto(
            p.Id,
            p.Name,
            p.Description,
            p.Icon,
            p.ColorGradient,
            p.IndividualPriceEgp,
            p.FamilyPriceEgp,
            p.OfferExpiryDate,
            p.IsActive,
            p.SortOrder,
            activeSubs.Count,
            activeSubs.Sum(s => s.AmountEgp),
            p.Benefits.OrderBy(b => b.SortOrder).Select(b => new ProgramBenefitDto(b.Id, b.AppliesTo, b.Text, b.SortOrder)).ToList(),
            p.ServiceDiscounts.Select(d => new ServiceDiscountDto(
                d.Id,
                d.MedicalServiceId,
                d.MedicalService?.Name ?? string.Empty,
                d.FirstVisitDiscountPercent,
                d.SecondVisitDiscountPercent,
                d.AdditionalVisitDiscountPercent)).ToList());
    }
}
