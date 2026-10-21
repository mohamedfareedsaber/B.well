using BWell.Loyalty.Domain.Common;
using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Domain.Entities;

public class LoyaltyProgram : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProgramIcon Icon { get; set; } = ProgramIcon.Crown;
    public ColorGradient ColorGradient { get; set; } = ColorGradient.Purple;
    public decimal IndividualPriceEgp { get; set; }
    public decimal FamilyPriceEgp { get; set; }
    public DateTime? OfferExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public string? StripeProductId { get; set; }
    public string? StripeIndividualPriceId { get; set; }
    public string? StripeFamilyPriceId { get; set; }

    public ICollection<ProgramBenefit> Benefits { get; set; } = new List<ProgramBenefit>();
    public ICollection<ProgramServiceDiscount> ServiceDiscounts { get; set; } = new List<ProgramServiceDiscount>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
