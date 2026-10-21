using BWell.Loyalty.Domain.Common;

namespace BWell.Loyalty.Domain.Entities;

/// <summary>
/// Tiered visit discounts: 1st visit, 2nd visit, additional visits.
/// Values are percentage discounts (0-100).
/// </summary>
public class ProgramServiceDiscount : BaseEntity
{
    public Guid LoyaltyProgramId { get; set; }
    public LoyaltyProgram LoyaltyProgram { get; set; } = null!;

    public Guid MedicalServiceId { get; set; }
    public MedicalService MedicalService { get; set; } = null!;

    public decimal FirstVisitDiscountPercent { get; set; }
    public decimal SecondVisitDiscountPercent { get; set; }
    public decimal AdditionalVisitDiscountPercent { get; set; }
}
