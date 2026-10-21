using BWell.Loyalty.Domain.Common;
using BWell.Loyalty.Domain.Enums;

namespace BWell.Loyalty.Domain.Entities;

public class ProgramBenefit : BaseEntity
{
    public Guid LoyaltyProgramId { get; set; }
    public LoyaltyProgram LoyaltyProgram { get; set; } = null!;
    public PlanType AppliesTo { get; set; } = PlanType.Individual;
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
